using Teo.AutoReference;
using UnityEngine;
using UnityEngine.UI;


namespace DBD.BaseGame.Sample
{
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class GridLayoutItemSize : BaseMonoBehaviour
    {
        [SerializeField, Get] private RectTransform rect;
        [SerializeField, Get] private GridLayoutGroup gridLayoutGroup;

        protected void OnRectTransformDimensionsChange()
        {
            CalculateIconSize();
        }

        protected override void Awake()
        {
            base.Awake();
            CalculateIconSize();
        }

        private void CalculateIconSize()
        {
            float width = rect.rect.width;
            Vector2 size = new Vector2(width, width);
            gridLayoutGroup.cellSize = size;
        }
    }
}