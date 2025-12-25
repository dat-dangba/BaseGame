using System.Collections.Generic;
using UnityEngine;

namespace SoundSample
{
    public class SoundManager : BaseSoundManager<SoundManager>
    {
        [SerializeField] private SoundData soundData;

        private Dictionary<SoundType, AudioClip> dict = new();

        public override bool IsMusicOn()
        {
            return PlayerPrefs.GetInt("music", 1) == 1;
        }

        public override bool IsSfxOn()
        {
            return PlayerPrefs.GetInt("sfx", 1) == 1;
        }

        protected override void UpdateMusic(bool active)
        {
            PlayerPrefs.SetInt("music", active ? 1 : 0);
        }

        protected override void UpdateSfx(bool active)
        {
            PlayerPrefs.SetInt("sfx", active ? 1 : 0);
        }

        protected override void Start()
        {
            base.Start();
            foreach (var item in soundData.SoundItems)
            {
                dict.Add(item.SoundType, item.Clip);
            }

            Play(SoundType.BackgroundMusic);
            // Play(SoundType.Test);
        }

        public void Play(SoundType soundType)
        {
            bool b = dict.TryGetValue(soundType, out AudioClip clip);
            if (!b) return;
            if (soundType == SoundType.BackgroundMusic)
            {
                PlayMusic(clip);
            }
            else
            {
                PlaySFX(clip);
            }
        }
    }
}