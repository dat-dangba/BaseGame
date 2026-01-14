namespace DBD.BaseGame.Sample
{
    public class ShowTestButton : BaseButton
    {
        protected override void OnClick()
        {
            UIManager.Instance.Show<TestUI>();
        }
    }
}