using UnityEngine;

namespace DBD.BaseGame
{
    public abstract class BaseVibrateManager<INSTANCE> : BaseMonoBehaviour where INSTANCE : MonoBehaviour
    {
        #region Singleton

        private static INSTANCE instance;

        public static INSTANCE Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindAnyObjectByType<INSTANCE>();
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
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        protected abstract void UpdateVibrate(bool active);

        public abstract bool IsVibrateOn();

        public void SetVibrate(bool active)
        {
            UpdateVibrate(active);
        }

        public void Vibrate()
        {
            if (!IsVibrateOn()) return;

            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
            else
            {
                Debug.Log($"(SoundManager) : Device is not support Vibration");
            }
        }
    }
}