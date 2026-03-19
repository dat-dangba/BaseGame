#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DBD.BaseGame.Editor
{
    public static class RectTransformContextMenu
    {
        [MenuItem("CONTEXT/RectTransform/Anchor View")]
        private static void AnchorView(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null || rectTransform.parent == null) return;
            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null) return;

            SetAnchorView(parent, rectTransform);
        }

        [MenuItem("CONTEXT/RectTransform/Anchor View And All Child")]
        private static void AnchorAllView(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null || rectTransform.parent == null) return;
            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null) return;

            SetAnchorViewAndAllChild(parent, rectTransform);
        }

        [MenuItem("CONTEXT/RectTransform/Anchor All Child")]
        private static void AnchorAllChild(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null || rectTransform.parent == null) return;
            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null) return;

            SetAnchorAllChild(rectTransform);
        }

        private static void SetAnchorViewAndAllChild(RectTransform parent, RectTransform rectTransform)
        {
            SetAnchorView(parent, rectTransform);
            SetAnchorAllChild(rectTransform);
        }

        private static void SetAnchorAllChild(RectTransform rectTransform)
        {
            if (!CanAnchorChild(rectTransform.gameObject)) return;

            foreach (RectTransform rect in rectTransform)
            {
                if (rect.childCount > 0)
                {
                    SetAnchorViewAndAllChild(rectTransform, rect);
                }
                else
                {
                    SetAnchorView(rectTransform, rect);
                }
            }
        }

        private static void SetAnchorView(RectTransform parent, RectTransform rectTransform)
        {
            if (!CanAnchorView(rectTransform.gameObject))
            {
                return;
            }

            Undo.RecordObject(rectTransform, $"Anchor View - {rectTransform.gameObject.name}");
            //set anchor center
            Vector3 pos = rectTransform.position;
            Vector2 size = rectTransform.rect.size;
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.position = pos;
            rectTransform.sizeDelta = size;

            // set anchor bound view 
            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            rectTransform.anchorMin = Vector2.one * 0.5f;
            rectTransform.anchorMax = Vector2.one * 0.5f;

            rectTransform.sizeDelta = new Vector2(width, height);
            float widthParent = parent.rect.size.x;
            float heightParent = parent.rect.size.y;

            float left = (widthParent - width) / 2 + rectTransform.anchoredPosition.x;
            float right = left + width;
            float top = (heightParent - height) / 2 + rectTransform.anchoredPosition.y;
            float bot = top + height;

            rectTransform.anchorMin = new Vector2(left / widthParent, top / heightParent);
            rectTransform.anchorMax = new Vector2(right / widthParent, bot / heightParent);

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            EditorUtility.SetDirty(rectTransform);
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(rectTransform.gameObject.scene);
            }
        }

        private static bool CanAnchorChild(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<ScrollRect>(out var scrollRect))
            {
                return false;
            }

            if (gameObject.TryGetComponent<LayoutGroup>(out var layoutGroup))
            {
                return false;
            }

            return true;
        }

        private static bool CanAnchorView(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<SafeArea>(out var safeArea))
            {
                return false;
            }

            if (gameObject.TryGetComponent<AspectRatioFitter>(out var aspectRatio))
            {
                return false;
            }

            return true;
        }
    }
}
#endif