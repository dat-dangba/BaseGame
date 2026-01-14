#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DBD.BaseGame.Editor
{
    public static class RectTransformContextMenu
    {
        [MenuItem("CONTEXT/RectTransform/Anchor View")]
        private static void StretchToParent(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null || rectTransform.parent == null) return;
            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null) return;

            Undo.RecordObject(rectTransform, "Anchor View");

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
        }
    }
}
#endif