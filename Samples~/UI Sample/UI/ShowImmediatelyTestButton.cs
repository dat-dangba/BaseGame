namespace DBD.BaseGame.Sample
{
    public class ShowImmediatelyTestButton : BaseButton
    {
        protected override void OnClick()
        {
            UIManager.Instance.ShowImmediately<TestUI>();
        }
    }
}