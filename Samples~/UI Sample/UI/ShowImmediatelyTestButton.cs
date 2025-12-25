namespace UISample
{
    public class ShowImmediatelyTestButton : BaseButton
    {
        protected override void OnClick()
        {
            UIManager.Instance.ShowImmediately<TestUI>();
        }
    }
}