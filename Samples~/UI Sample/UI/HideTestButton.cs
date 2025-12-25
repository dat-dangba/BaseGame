namespace UISample
{
    public class HideTestButton : BaseButton
    {
        protected override void OnClick()
        {
            UIManager.Instance.Hide<TestUI>();
        }
    }
}