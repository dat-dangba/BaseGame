using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
public static class CreateFitterContextMenu
{
    [MenuItem("CONTEXT/RectTransform/Create Fitter All")]
    private static void CreateFitter(MenuCommand command)
    {
        RectTransform rectTransform = GetRectTransform(command);
        if (rectTransform == null) return;

        Create(rectTransform, "FitterTop", new Vector2(0.5f, 1f));
        Create(rectTransform, "FitterLeft", new Vector2(0f, 1f));
        Create(rectTransform, "FitterRight", new Vector2(1f, 1f));
        Create(rectTransform, "FitterBot", new Vector2(0.5f, 0f));
        Create(rectTransform, "FitterCenter", new Vector2(0.5f, 0.5f));
    }

    [MenuItem("CONTEXT/RectTransform/Create Fitter Center")]
    private static void CreateFitterCenter(MenuCommand command)
    {
        RectTransform rectTransform = GetRectTransform(command);
        if (rectTransform == null) return;

        Create(rectTransform, "FitterCenter", new Vector2(0.5f, 0.5f));
    }

    [MenuItem("CONTEXT/RectTransform/Create Fitter Top")]
    private static void CreateFitterTop(MenuCommand command)
    {
        RectTransform rectTransform = GetRectTransform(command);
        if (rectTransform == null) return;

        Create(rectTransform, "FitterTop", new Vector2(0.5f, 1f));
    }

    [MenuItem("CONTEXT/RectTransform/Create Fitter Bot")]
    private static void CreateFitterBot(MenuCommand command)
    {
        RectTransform rectTransform = GetRectTransform(command);
        if (rectTransform == null) return;

        Create(rectTransform, "FitterBot", new Vector2(0.5f, 0f));
    }

    [MenuItem("CONTEXT/RectTransform/Create Fitter Left")]
    private static void CreateFitterLeft(MenuCommand command)
    {
        RectTransform rectTransform = GetRectTransform(command);
        if (rectTransform == null) return;

        Create(rectTransform, "FitterLeft", new Vector2(0f, 1f));
    }

    [MenuItem("CONTEXT/RectTransform/Create Fitter Right")]
    private static void CreateFitterRight(MenuCommand command)
    {
        RectTransform rectTransform = GetRectTransform(command);
        if (rectTransform == null) return;

        Create(rectTransform, "FitterRight", new Vector2(1f, 1f));
    }

    private static RectTransform GetRectTransform(MenuCommand command)
    {
        RectTransform rectTransform = command.context as RectTransform;
        if (rectTransform == null)
        {
            Debug.LogError($"datdb - RectTransform null");
            return null;
        }

        Vector2 size = rectTransform.rect.size;
        if (size != new Vector2(1080, 1920))
        {
            Debug.LogError($"datdb - Size chưa chuẩn 1080x1920. Chỉnh size về 1080x1920");
            return null;
        }

        return rectTransform;
    }

    private static void Create(RectTransform rectTransform, string name, Vector2 pivot)
    {
        var fitterTop = new GameObject(name, typeof(RectTransform),
            typeof(AspectRatioFitter))
        {
            transform =
            {
                parent = rectTransform,
                localScale = Vector3.one,
                position = Vector3.zero
            }
        };
        RectTransform rectFitterTop = fitterTop.GetComponent<RectTransform>();
        rectFitterTop.anchorMin = Vector2.zero;
        rectFitterTop.anchorMax = Vector2.one;

        rectFitterTop.offsetMin = Vector2.zero;
        rectFitterTop.offsetMax = Vector2.zero;

        rectFitterTop.pivot = pivot;

        rectFitterTop.anchoredPosition3D = Vector2.zero;

        AspectRatioFitter aspectRatioFitter = fitterTop.GetComponent<AspectRatioFitter>();
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
    }
}
#endif