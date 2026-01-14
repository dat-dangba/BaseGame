using Teo.AutoReference;
using UnityEngine;

namespace DBD.BaseGame.Sample
{
    public class UIManager : BaseUIManager<UIManager>
    {
        [SerializeField, FindInScene, Name("UICanvas")]
        private Transform uiCanvas;

        protected override Transform GetParent()
        {
            return uiCanvas;
        }

        protected override string GetFolderPrefabs()
        {
            return "Assets/Samples/UI Sample/UI";
        }

        protected override void OnInitCompleted()
        {
            //show ui loading
            // Show<LoadingUI>(ui => { });
            // Show(typeof(LoadingUI), ui =>
            // {
            //     
            // });
        }

        [ContextMenu("Test UI Loaded")]
        private void TestUILoaded()
        {
            Debug.Log($"datdb - {IsUILoaded<TestUI>()} {IsUILoaded(typeof(TestUI))}");
        }

        [ContextMenu("Test UI Displayed")]
        private void TestUIDisplayed()
        {
            Debug.Log($"datdb - {IsUIDisplayed<LoadingUI>()} {IsUIDisplayed(typeof(LoadingUI))}");
        }

        [ContextMenu("Test UI Only Display")]
        private void TestUIOnlyDisplay()
        {
            Debug.Log($"datdb - {IsUIOnlyDisplay<LoadingUI>()} {IsUIOnlyDisplay(typeof(LoadingUI))}");
        }

        [ContextMenu("Test UI On Top")]
        private void TestUIOnTop()
        {
            Debug.Log($"datdb - {IsUIOnTop<LoadingUI>()} {IsUIOnTop(typeof(LoadingUI))}");
        }
    }
}