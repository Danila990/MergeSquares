using System;
using System.Collections.Generic;
using Hellmade;
using UnityEngine;
using Utils;
using Zenject;
using Random = UnityEngine.Random;

namespace Core.Audio
{
    [CreateAssetMenu(fileName = "SoundSource", menuName = "Repositories/SoundSource")]
    public class SoundSource : ScriptableObject
    {
        public List<AudioClip> variants = new();
        public FloatRange pitchRange;
        public bool usePitch = true;
        public float baseVolume;
        public Hellmade.Audio.AudioType audioType;
        public bool loop;
        public bool useIgnoreDuplicate;
        public bool ignoreDuplicateSounds;
        public float fadeIn;
        public float fadeOut;
        public float min3DDistance = 100f;

        private EazySoundManager _soundManager;

        private Func<int, Hellmade.Audio>[] _audioGetters;
        private Func<AudioClip, float, bool, Transform, int>[] _audioInitializers;
        private Func<AudioClip, float, bool, Transform, bool, int>[] _audioDuplicateInitializers;

        [Inject]
        public void Construct(EazySoundManager soundManager)
        {
            _soundManager = soundManager;
            InitAudioGetters();
            InitAudioInitializers();
        }

        private void InitAudioInitializers()
        {
            _audioInitializers = new Func<AudioClip, float, bool, Transform, int>[]
            {
                (clip, volume, aLoop, transform) => _soundManager.PrepareMusic(clip, baseVolume, loop, transform),
                (clip, volume, aLoop, transform) => _soundManager.PrepareSound(clip, baseVolume, loop, transform),
                (clip, volume, aLoop, transform) => _soundManager.PrepareUISound(clip, baseVolume, loop, transform)
            };
            _audioDuplicateInitializers = new Func<AudioClip, float, bool, Transform, bool, int>[]
            {
                (clip, volume, aLoop, transform, ignoreDuplicateSounds) => _soundManager.PrepareMusic(clip, baseVolume, loop, transform, ignoreDuplicateSounds),
                (clip, volume, aLoop, transform, ignoreDuplicateSounds) => _soundManager.PrepareSound(clip, baseVolume, loop, transform, ignoreDuplicateSounds),
                (clip, volume, aLoop, transform, ignoreDuplicateSounds) => _soundManager.PrepareUISound(clip, baseVolume, loop, transform, ignoreDuplicateSounds)
            };
        }

        private void InitAudioGetters()
        {
            _audioGetters = new Func<int, Hellmade.Audio>[]
            {
                id => _soundManager.GetMusicAudio(id),
                id => _soundManager.GetSoundAudio(id),
                id => _soundManager.GetUISoundAudio(id)
            };
        }

        public void Play(float pitch = -1, Transform transform = null)
        {
            var audio = GetAudio();
            if (audio != null)
            {
                if (pitch >= 0)
                {
                    audio.Pitch = pitch;
                }
                if (audio.Paused)
                {
                    audio.UnPause();
                }
                else
                {
                    audio.Play();
                }
            }
        }

        public void Stop(Transform transform = null)
        {
            GetAudio()?.Stop();
        }
        
        public void Pause(Transform transform = null)
        {
            GetAudio()?.Pause();
        }
        
        public bool IsPlaying(Transform transform = null)
        {
            var audio = GetAudio();
            if (audio != null && audio.AudioSource != null)
            {
                return audio.AudioSource.isPlaying;
            }
            return  audio?.IsPlaying ?? false;
        }

        private Hellmade.Audio GetAudio(Transform transform = null)
        {
            if (variants.Count == 0) return null;
            var clip = variants[Random.Range(0, variants.Count)];
            var pitch = Random.Range(pitchRange.min, pitchRange.max);
            
            var soundId = useIgnoreDuplicate ?
                _audioDuplicateInitializers[(int) audioType].Invoke(clip, baseVolume, loop, transform, ignoreDuplicateSounds)
                : _audioInitializers[(int) audioType].Invoke(clip, baseVolume, loop, transform);
            var audio = _audioGetters[(int) audioType].Invoke(soundId);
            if (audioType != Hellmade.Audio.AudioType.Music && usePitch) audio.Pitch = pitch;
            audio.FadeInSeconds = fadeIn;
            audio.FadeOutSeconds = fadeOut;
            audio.Min3DDistance = min3DDistance;
            return audio;
        }
    }
}