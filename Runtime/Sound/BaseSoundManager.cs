using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DBD.BaseGame
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseSoundManager<INSTANCE> : BaseMonoBehaviour where INSTANCE : MonoBehaviour
    {
        [SerializeField, Get] private AudioSource bgmSource;

        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private List<AudioSource> sfxPool = new();

        protected override void Reset()
        {
            base.Reset();
#if UNITY_EDITOR
            audioMixer =
                AssetDatabase.LoadAssetAtPath<AudioMixer>("Packages/com.datdb.basegame/Runtime/Sound/AudioMixer.mixer");
#endif
        }

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

        public void PlayMusic(AudioClip clip, float volume = 0)
        {
            if (!IsMusicOn()) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            if (bgmSource.outputAudioMixerGroup == null)
            {
                AudioMixerGroup group = FindAudioMixerGroupByName("Music");
                bgmSource.outputAudioMixerGroup = group;
            }

            // Debug.Log($"datdb - {bgmSource.outputAudioMixerGroup.name}");
            SetVolumeAudioMixerGroup(bgmSource.outputAudioMixerGroup, volume);
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

        public void PlaySFX(AudioClip clip, float volume = 0)
        {
            if (!IsSfxOn()) return;

            var source = GetFreeSFXSource();
            SetVolumeAudioMixerGroup(source.outputAudioMixerGroup, volume);
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
            if (source.outputAudioMixerGroup == null)
            {
                source.outputAudioMixerGroup = FindAudioMixerGroupByName($"SFX {sfxPool.Count}");
            }

            sfxPool.Add(source);

            return source;
        }

        private AudioMixerGroup FindAudioMixerGroupByName(string name)
        {
            return audioMixer.FindMatchingGroups("Master").First(group => group.name == name);
        }

        private void SetVolumeAudioMixerGroup(AudioMixerGroup audioMixerGroup, float volume)
        {
            // float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
            // float dB = Mathf.Log10(clampedVolume) * 20;
            // Debug.Log($"datdb - {dB}");
            audioMixer.SetFloat(audioMixerGroup.name, volume);
        }
    }
}