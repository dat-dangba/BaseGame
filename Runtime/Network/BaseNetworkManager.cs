using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace DBD.BaseGame
{
    public abstract class BaseNetworkManager<INSTANCE> : BaseMonoBehaviour where INSTANCE : MonoBehaviour
    {
        private bool isRequesting;
        private bool isAvailable;
        private bool isStartCheckInternet;

        public bool IsAvailable => isAvailable;

        #region Singleton

        private static INSTANCE instance;

        public static INSTANCE Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindFirstObjectByType<INSTANCE>();
                if (instance != null) return instance;
                GameObject singleton = new(typeof(INSTANCE).Name);
                instance = singleton.AddComponent<INSTANCE>();
                DontDestroyOnLoad(singleton);

                return instance;
            }
        }

        protected override void Awake()
        {
            if (instance == null)
            {
                instance = this as INSTANCE;
                Transform root = transform.root;
                if (root != transform)
                {
                    DontDestroyOnLoad(root);
                }
                else
                {
                    DontDestroyOnLoad(gameObject);
                }

                isAvailable = IsNetworkAvailable();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        protected override void Update()
        {
            base.Update();
            if (!isStartCheckInternet) return;

            if (!IsNetworkAvailable())
            {
                SetNetworkAvailable(false);
            }
        }

        public void StartCheckInternet()
        {
            isStartCheckInternet = true;
            StartCoroutine(CheckInternetRoutine());
            InvokeRepeating(nameof(CheckInternet), 5, 5);
        }

        public void Refresh()
        {
            if (isRequesting) return;
            StartCoroutine(CheckInternetRoutine());
        }

        private void CheckInternet()
        {
            if (!isAvailable || isRequesting) return;
            StartCoroutine(CheckInternetRoutine());
        }

        protected virtual string GetCheckUrl()
        {
#if UNITY_ANDROID
            return "https://connectivitycheck.gstatic.com/generate_204";
#elif UNITY_IOS
            return "https://captive.apple.com/hotspot-detect.html";
#else
            return "https://clients3.google.com/generate_204";
#endif
        }

        protected virtual bool IsNetworkAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        protected virtual IEnumerator CheckInternetRoutine()
        {
            if (isRequesting)
            {
                yield break;
            }

            Debug.Log($"Network - CheckInternetRoutine");
            isRequesting = true;

            if (!IsNetworkAvailable())
            {
                SetNetworkAvailable(false);
                yield break;
            }

            string url = GetCheckUrl();
            Debug.Log($"Network - url {url}");
            using UnityWebRequest request = UnityWebRequest.Head(url);
            request.timeout = GetTimeOutRequest();
            yield return request.SendWebRequest();
            Debug.Log(
                $"Network - Request Completed {request.result is not (UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)}");
            SetNetworkAvailable(!(request.result == UnityWebRequest.Result.ConnectionError ||
                                  request.result == UnityWebRequest.Result.ProtocolError));
        }

        protected virtual int GetTimeOutRequest()
        {
            return 2;
        }

        protected virtual void SetNetworkAvailable(bool isAvailable)
        {
            if (this.isAvailable == isAvailable) return;

            this.isAvailable = isAvailable;

            Debug.Log($"Network - OnNetworkStateChanged {this.isAvailable}");
            OnNetworkStateChanged(this.isAvailable);
        }

        protected abstract void OnNetworkStateChanged(bool isAvailable);
    }
}