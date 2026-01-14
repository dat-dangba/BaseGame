using System.Collections;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DBD.BaseGame
{
    public abstract class BaseButton : BaseMonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField, Get] protected Button button;

        protected override void OnAfterSyncAttribute()
        {
            button.transition = Selectable.Transition.None;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (button)
            {
                button.transform.localScale = Vector3.one;
            }
        }

        protected override void Start()
        {
            base.Start();
            AddOnClickEvent();
        }

        protected virtual void AddOnClickEvent()
        {
            button.onClick.AddListener(OnClickListener);
        }

        protected virtual void OnClickListener()
        {
            OnClick();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            OnPointerDown(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            OnPointerUp(eventData);
        }

        protected virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!IsAnim()) return;

            StopAllCoroutines();
            StartCoroutine(AnimPointer(true));
        }

        protected virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!IsAnim()) return;

            StopAllCoroutines();
            StartCoroutine(AnimPointer(false));
        }

        private bool IsAnim()
        {
            return button.interactable && IsUseAnim();
        }

        protected virtual bool IsUseAnim()
        {
            return true;
        }

        private IEnumerator AnimPointer(bool isDown)
        {
            float temp = 0f;
            while (temp < 0.1f)
            {
                temp += Time.unscaledDeltaTime;
                temp = Mathf.Clamp(temp, 0, 0.1f);

                float f;
                if (isDown)
                {
                    f = 1 - temp;
                }
                else
                {
                    f = 0.9f + temp;
                }

                transform.localScale = Vector3.one * f;
                yield return null;
            }
        }

        protected abstract void OnClick();
    }
}