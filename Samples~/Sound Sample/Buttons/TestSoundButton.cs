namespace DBD.BaseGame.Sample
{
    public class TestSoundButton : BaseButton
    {
        protected override void OnClick()
        {
            SoundManager.Instance.Play(SoundType.Click);
        }
    }
}