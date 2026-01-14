using System.Collections.Generic;
using Teo.AutoReference;
using UnityEngine;

namespace DBD.BaseGame
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseSoundManager<INSTANCE> : BaseMonoBehaviour where INSTANCE : MonoBehaviour
    {
        [SerializeField, Get] private AudioSource bgmSource;

        private List<AudioSource> sfxPool = new();

        #region Abstract

        public abstract bool IsMusicOn();
        public abstract bool IsSfxOn();

        protected abstract void UpdateMusic(bool active);
        protected abstract void UpdateSfx(bool active);

        #endregion

        #region Singleton

        private static INSTANCE instance;

        public static INSTANCE Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindAnyObjectByType<INSTANCE>();
                if (instance != null) return instance;
                GameObject singleton = new(typeof(INSTANCE).Name);
                instance = singleton.AddComponent<INSTANCE>();
                DontDestroyOnLoad(singleton);

                return instance;
            }
        }

        protected override void Awake()
        {
            if (instance == null)
            {
                instance = this as INSTANCE;

                Transform root = transform.root;
                if (root != transform)
                {
                    DontDestroyOnLoad(root);
                }
                else
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        public void SetMusic(bool active)
        {
            UpdateMusic(active);
            if (!active)
            {
                StopMusic();
            }
            else
            {
                PlayMusic();
            }
        }

        public void SetSfx(bool active)
        {
            UpdateSfx(active);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (!IsMusicOn()) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void PlayMusic()
        {
            if (bgmSource.clip != null && !bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }

        public void StopMusic()
        {
            if (bgmSource.clip != null && bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (!IsSfxOn()) return;

            var source = GetFreeSFXSource();
            source.clip = clip;
            source.loop = false;
            source.Play();
        }

        private AudioSource GetFreeSFXSource()
        {
            foreach (var s in sfxPool)
            {
                if (!s.isPlaying) return s;
            }

            if (sfxPool.Count >= 10) return sfxPool[0];

            var go = new GameObject($"Sfx {sfxPool.Count}");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sfxPool.Add(source);

            return source;
        }
    }
}