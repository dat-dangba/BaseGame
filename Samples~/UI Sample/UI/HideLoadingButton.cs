namespace DBD.BaseGame.Sample
{
    public class HideLoadingButton : BaseButton
    {
        protected override void OnClick()
        {
            UIManager.Instance.Hide<LoadingUI>(() => { });
        }
    }
}