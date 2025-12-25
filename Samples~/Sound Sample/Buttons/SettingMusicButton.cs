using Teo.AutoReference;
using TMPro;
using UnityEngine;

namespace SoundSample
{
    public class SettingMusicButton : BaseButton
    {
        [SerializeField, GetInChildren] private TextMeshProUGUI text;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateState();
        }

        protected override void OnClick()
        {
            SoundManager.Instance.SetMusic(!SoundManager.Instance.IsMusicOn());
        }

        private void UpdateState()
        {
            string state = SoundManager.Instance.IsMusicOn() ? "on" : "off";
            text.text = $"Music: {state}";
        }
    }
}