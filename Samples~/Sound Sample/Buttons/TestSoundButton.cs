namespace SoundSample
{
    public class TestSoundButton : BaseButton
    {
        protected override void OnClick()
        {
            SoundManager.Instance.Play(SoundType.Click);
        }
    }
}