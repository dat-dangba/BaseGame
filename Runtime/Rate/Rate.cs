using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DBD.BaseGame
{
    public class Rate : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private List<GameObject> stars;

        private int star = -1;

        private void Reset()
        {
            stars = new List<GameObject>();
            foreach (Transform item in transform)
            {
                stars.Add(item.gameObject);
            }
        }

        private void OnEnable()
        {
            star = -1;
            FillStar();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            CheckStar();
        }

        public void OnDrag(PointerEventData eventData)
        {
            CheckStar();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CheckStar();
        }

        private void CheckStar()
        {
            PointerEventData ped = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(ped, results);
            int s = GetStar(results);
            if (s == star) return;
            star = s;
            FillStar();
        }

        private void FillStar()
        {
            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].transform.GetChild(0).gameObject.SetActive(i <= star);
            }
        }

        private int GetStar(List<RaycastResult> results)
        {
            for (int i = 0; i < stars.Count; i++)
            {
                for (int j = 0; j < results.Count; j++)
                {
                    if (stars[i].name == results[j].gameObject.name)
                    {
                        return i;
                    }
                }
            }

            return star;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId">Android = Application.identifier | iOS = app store id</param>
        public bool GoToStore(string appId)
        {
            if (star < 2) return false;
#if UNITY_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + appId);
#elif UNITY_IOS
            // bool popupShown = Device.RequestStoreReview();
            // if (!popupShown)
            // {
            // Application.OpenURL($"https://apps.apple.com/app/id{applicationId}?action=write-review");
            // }
            Application.OpenURL($"https://apps.apple.com/app/id{appId}?action=write-review");
#endif
            return true;
        }
    }
}