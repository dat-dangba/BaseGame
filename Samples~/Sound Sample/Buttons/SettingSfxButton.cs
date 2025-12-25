using Teo.AutoReference;
using TMPro;
using UnityEngine;

namespace SoundSample
{
    public class SettingSfxButton : BaseButton
    {
        [SerializeField, GetInChildren] private TextMeshProUGUI text;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateState();
        }

        protected override void OnClick()
        {
            SoundManager.Instance.SetSfx(!SoundManager.Instance.IsSfxOn());
            UpdateState();
        }

        private void UpdateState()
        {
            string state = SoundManager.Instance.IsSfxOn() ? "on" : "off";
            text.text = $"Sfx: {state}";
        }
    }
}