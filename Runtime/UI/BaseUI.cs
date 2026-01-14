using System;
using System.Collections;
using Teo.AutoReference;
using UnityEngine;

namespace DBD.BaseGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseUI : BaseMonoBehaviour
    {
        [SerializeField] private bool destroyOnHide;

        [SerializeField, GetInChildren, Name("Container")]
        private Transform container;

        [SerializeField, Get] private CanvasGroup canvasGroup;

        private const float durationAnim = 0.3f;

        protected virtual void SetUp()
        {
        }

        protected virtual bool IsUseAnim()
        {
            return true;
        }

        private void SetInteractable(bool b)
        {
            if (!canvasGroup) return;
            canvasGroup.interactable = b;
        }

        #region Show

        public void Show()
        {
            if (!IsUseAnim())
            {
                ShowImmediately();
                return;
            }

            SetInteractable(false);

            SetUp();
            gameObject.SetActive(true);
            StartCoroutine(AnimShow());
        }

        public void ShowImmediately()
        {
            SetUp();
            gameObject.SetActive(true);

            SetInteractable(true);
            if (!container) return;
            container.localScale = Vector3.one;
        }

        private IEnumerator AnimShow()
        {
            if (container)
            {
                container.localScale = Vector3.zero;
                float time = 0;
                while (time < durationAnim)
                {
                    float t = time / durationAnim;
                    t = EaseOutBack(t);
                    container.localScale = new Vector3(t, t, t);
                    time += Time.unscaledDeltaTime;
                    yield return null;
                }

                container.localScale = Vector3.one;
            }

            SetInteractable(true);
        }

        private float EaseOutBack(float t, float s = 1.70158f)
        {
            return 1 + (t - 1) * (t - 1) * ((s + 1) * (t - 1) + s);
        }

        #endregion

        #region Hide

        public void Hide(Action OnInvisible = null)
        {
            SetInteractable(false);
            if (!IsUseAnim())
            {
                HideImmediately();
                return;
            }

            StartCoroutine(AnimHide(OnInvisible));
        }

        public void HideImmediately()
        {
            Invisible();
        }

        private void Invisible()
        {
            SetInteractable(true);
            if (destroyOnHide)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator AnimHide(Action OnInvisible)
        {
            if (container)
            {
                float time = durationAnim;
                while (time > 0)
                {
                    float t = time / durationAnim;
                    t = EaseInBack(t);
                    container.localScale = new Vector3(t, t, t);
                    time -= Time.unscaledDeltaTime;
                    yield return null;
                }

                container.localScale = Vector3.zero;
            }

            Invisible();
            OnInvisible?.Invoke();
        }

        private float EaseInBack(float t, float s = 1.70158f)
        {
            return 1 - ((1 - t) * (1 - t) * ((s + 1) * (1 - t) - s));
        }

        #endregion
    }
}