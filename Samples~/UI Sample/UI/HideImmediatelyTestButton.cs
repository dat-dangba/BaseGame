namespace DBD.BaseGame.Sample
{
    public class HideImmediatelyTestButton : BaseButton
    {
        protected override void OnClick()
        {
            UIManager.Instance.HideImmediately<TestUI>();
        }
    }
}