using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Core.Services
{
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        
        [Header("UI Sounds")]
        [SerializeField] private AudioClip _clickSound;
        [SerializeField] private AudioClip _hoverSound;
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _errorSound;
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _notificationSound;
        [SerializeField] private AudioClip _typingSound;
        
        [Header("Settings")]
        [SerializeField] private float _masterVolume = 1f;
        [SerializeField] private float _musicVolume = 0.7f;
        [SerializeField] private float _sfxVolume = 1f;
        
        [Inject] private IEventService _eventService;
        
        private Dictionary<string, AudioClip> _musicLibrary = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> _sfxLibrary = new Dictionary<string, AudioClip>();
        private Coroutine _musicFadeCoroutine;
        
        private void Awake()
        {
            CreateAudioSources();
            LoadAudioLibraries();
        }
        
        private void Start()
        {
            Dependencies.Inject(this);
        }
        
        private void CreateAudioSources()
        {
            if (_musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                _musicSource = musicObj.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }
            
            if (_sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                _sfxSource = sfxObj.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }
            
            UpdateVolumes();
        }
        
        private void LoadAudioLibraries()
        {
            AudioClip[] musicClips = Resources.LoadAll<AudioClip>("Audio/Music");
            foreach (var clip in musicClips)
            {
                _musicLibrary[clip.name] = clip;
            }
            
            AudioClip[] sfxClips = Resources.LoadAll<AudioClip>("Audio/SFX");
            foreach (var clip in sfxClips)
            {
                _sfxLibrary[clip.name] = clip;
            }
        }
        
        public void PlayMusic(string musicId, bool loop = true, float fadeInDuration = 1f)
        {
            if (!_musicLibrary.TryGetValue(musicId, out AudioClip clip))
            {
                Debug.LogWarning($"[AudioService] Music '{musicId}' not found");
                return;
            }
            
            if (_musicFadeCoroutine != null) StopCoroutine(_musicFadeCoroutine);
            
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicFadeCoroutine = StartCoroutine(FadeMusic(_musicSource, 0f, _musicVolume * _masterVolume, fadeInDuration));
            _musicSource.Play();
            
            _eventService?.Publish(new MusicStartedEvent { MusicId = musicId });
        }
        
        public void StopMusic(float fadeOutDuration = 1f)
        {
            if (_musicSource.isPlaying)
            {
                if (_musicFadeCoroutine != null) StopCoroutine(_musicFadeCoroutine);
                _musicFadeCoroutine = StartCoroutine(FadeMusic(_musicSource, _musicSource.volume, 0f, fadeOutDuration, () => _musicSource.Stop()));
            }
        }
        
        public void PlaySound(string soundId, float volume = 1f)
        {
            if (!_sfxLibrary.TryGetValue(soundId, out AudioClip clip))
            {
                Debug.LogWarning($"[AudioService] Sound '{soundId}' not found");
                return;
            }
            
            _sfxSource.PlayOneShot(clip, volume * _sfxVolume * _masterVolume);
            _eventService?.Publish(new SoundPlayedEvent { SoundId = soundId });
        }
        
        public void PlayUISound(UISoundType soundType)
        {
            AudioClip clip = soundType switch
            {
                UISoundType.Click => _clickSound,
                UISoundType.Hover => _hoverSound,
                UISoundType.Open => _openSound,
                UISoundType.Close => _closeSound,
                UISoundType.Error => _errorSound,
                UISoundType.Success => _successSound,
                UISoundType.Notification => _notificationSound,
                UISoundType.Typing => _typingSound,
                _ => null
            };
            
            if (clip != null) _sfxSource.PlayOneShot(clip, _sfxVolume * _masterVolume);
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public float GetMasterVolume() => _masterVolume;
        public float GetMusicVolume() => _musicVolume;
        public float GetSFXVolume() => _sfxVolume;
        
        private void UpdateVolumes()
        {
            if (_musicSource != null) _musicSource.volume = _musicVolume * _masterVolume;
        }
        
        private IEnumerator FadeMusic(AudioSource source, float startVolume, float targetVolume, float duration, System.Action onComplete = null)
        {
            float elapsed = 0f;
            source.volume = startVolume;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }
            
            source.volume = targetVolume;
            onComplete?.Invoke();
        }
    }
}