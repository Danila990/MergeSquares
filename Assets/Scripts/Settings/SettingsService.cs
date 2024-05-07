using Core.Localization;
using Core.SaveLoad;
using Hellmade;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CloudServices;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Settings
{
    [Serializable]
    public class SettingsServiceData
    {
        public float musicVolume = 0.5f;
        public float soundsVolume = 0.5f;
        public float musicSavedVolume = 0.5f;
        public float soundsSavedVolume = 0.5f;
        public float volume = 1;
        public bool vibration;
        public bool notifications;
        public string language;
    }

    public class SettingsService : MonoBehaviour
    {
        [SerializeField] private Saver saver;
        [SerializeField] private string defaultLanguage = "en";
        [SerializeField] private bool forceDefaultLanguageOnStart;

        public event Action SettingsChanged;
        [Space]
        public UnityEvent UnknownLanguageLoaded;

        public bool MusicState => _data.musicVolume > 0;
        public bool SoundState => _data.soundsVolume > 0;
        public bool VibrationState => _data.vibration;
        public bool NotificationsState => _data.notifications;

        public float SoundsSavedVolume
        {
            get { return _data.soundsSavedVolume; }
        }
        
        public float MusicVolume 
        {
            get { return _data.musicVolume; }
            set { SetMusicVolume(value); }
        }

        public float SoundVolume
        {
            get { return _data.soundsVolume; }
            set { SetSoundsVolume(value); }
        }
        public float MusicSavedVolume
        {
            get { return _data.musicSavedVolume; }
        }

        public float SoundSavedVolume
        {
            get { return _data.soundsSavedVolume; }
        }

        public float Volume
        {
            get { return _data.volume; }
            set { SetVolume(value); }
        }

        public string Language
        {
            get { return _data.language; }
            set { SetLanguage(value); }
        }

        private SettingsServiceData _data = new SettingsServiceData();
        private bool _soundOff;
        private bool _hasFocus = true;

        private EazySoundManager _eazySoundManager;
        private LocalizationRepository _localizationRepository;
        private CloudService _cloudService;

        [Inject]
        public void Construct(
            EazySoundManager eazySoundManager,
            LocalizationRepository localizationRepository,
            CloudService cloudService
        )
        {
            _eazySoundManager = eazySoundManager;
            _localizationRepository = localizationRepository;
            _cloudService = cloudService;

            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
            _cloudService.CloudProvider.OnPause += OnPause;
            _cloudService.CloudProvider.OnResume += OnResume;
        }

        private void OnDestroy()
        {
            _cloudService.CloudProvider.OnPause -= OnPause;
            _cloudService.CloudProvider.OnResume -= OnResume;
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
            if(_soundOff)
            {
                return;
            }
            SetGlobalVolume(hasFocus ? Volume : 0);
        }
        
        private void OnPause()
        {
            SetGlobalVolume(0);
        }
        
        private void OnResume()
        {
            SetGlobalVolume(Volume);
        }

        public void Init(SettingsServiceData data)
        {
            _data = data;
            
            _eazySoundManager.GlobalMusicVolume = _data.musicVolume;
            _eazySoundManager.GlobalSoundsVolume = _data.soundsVolume;
            _eazySoundManager.GlobalVolume = _data.volume;
            
            Language = _cloudService.CloudProvider.GetLanguage();
            
            if (!IsLanguageValid(Language))
            {
                if(TryGetValidSystemLanguage(out var validLanguage) && !forceDefaultLanguageOnStart)
                {
                    SetLanguage(validLanguage);
                }
                else
                {
                    SetLanguage(defaultLanguage);
                    UnknownLanguageLoaded?.Invoke();
                }
            }

            _localizationRepository.SetLanguage(Language);
        }

        public List<string> GetLanguageList()
        {
            return _localizationRepository.GetLanguagesList();
        }

        public bool InvertSoundState()
        {
            var volume = 0f;
            if(_data.soundsVolume < float.Epsilon)
            {
                volume = SoundSavedVolume;
            }
            else
            {
                SaveSoundsVolume();
            }
            SetSoundsVolume(volume);
            return volume > 0;
        }
        
        public bool InvertMusicState()
        {
            var volume = 0f;
            if(_data.musicVolume < float.Epsilon)
            {
                volume = MusicSavedVolume;
            }
            else
            {
                SaveMusicVolume();
            }
            SetMusicVolume(volume);
            return volume > 0;
        }

        public bool InvertVibrationState()
        {
            _data.vibration = !_data.vibration;
            // TODO: remove this
            var inst = IngameDebugConsole.DebugLogManager.Instance;
            if(inst != null)
            {
                inst.PopupEnabled = _data.vibration;
            }
            return _data.vibration;
        }

        public bool InvertNotificationsState()
        {
            _data.notifications = !_data.notifications;
            return _data.notifications;
        }
        
        public void SetMusicVolume(float volume)
        {
            SetClampedVolume(volume, ref _data.musicVolume, (float newVolume) => _eazySoundManager.GlobalMusicVolume = newVolume);
        }

        public void SetSoundsVolume(float volume)
        {
            SetClampedVolume(volume, ref _data.soundsVolume, (float newVolume) => _eazySoundManager.GlobalSoundsVolume = newVolume);
        }

        public void SaveMusicVolume()
        {
            SetClampedVolume(_data.musicVolume, ref _data.musicSavedVolume);
        }
        public void SaveSoundsVolume()
        {
            SetClampedVolume(_data.soundsVolume, ref _data.soundsSavedVolume);
        }

        public void SetVolume(float volume)
        {
            SetClampedVolume(volume, ref _data.volume, (float newVolume) => _eazySoundManager.GlobalSoundsVolume = newVolume);
        }

        public void TurnOffGlobalVolume()
        {
            _soundOff = true;
            SetGlobalVolume(0);
        }

        public void TurnOnGlobalVolume()
        {
            _soundOff = false;
            if(_hasFocus)
            {
                SetGlobalVolume(Volume);
            }
        }

        public void SetGlobalVolume(float volume)
        {
            _eazySoundManager.GlobalVolume = volume;
        }
        
        public void SetLanguage(string language)
        {
            if (IsLanguageValid(language))
            {
                _data.language = language;
                _localizationRepository.SetLanguage(language);

                saver.SaveNeeded.Invoke(true);
                SettingsChanged?.Invoke();
            }
        }

        private void SetClampedVolume(float volume, ref float dataValue, Action<float> setManagerVolume)
        {
            var clampedVolume = Mathf.Clamp01(volume);
            dataValue = clampedVolume;

            setManagerVolume(clampedVolume);

            saver.SaveNeeded(true);
            SettingsChanged?.Invoke();
        }

        private void SetClampedVolume(float volume, ref float dataValue)
        {
            var clampedVolume = Mathf.Clamp01(volume);
            dataValue = clampedVolume;

            saver.SaveNeeded(true);
            SettingsChanged?.Invoke();
        }

        private bool IsLanguageValid(string language)
        {
            var languageList = GetLanguageList();
            return !string.IsNullOrEmpty(language) && languageList.Contains(language);
        }

        private bool TryGetValidSystemLanguage(out string language)
        {
            if(TryGetSystemLanguageCountryCode(out var countryCode))
            {
                if (IsLanguageValid(countryCode))
                {
                    language = countryCode;
                    return true;
                }
            }

            language = null;
            return false;
        }

        private bool TryGetSystemLanguageCountryCode(out string countryCode)
        {
            var allCultures = CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            var systemLanguage = Application.systemLanguage.ToString();

            var languageCultureInfo = allCultures.FirstOrDefault(c => c.EnglishName == systemLanguage);
            if (languageCultureInfo != null)
            {
                var languageCountryCode = languageCultureInfo.TwoLetterISOLanguageName;
                languageCountryCode = languageCountryCode.Substring(0, 1).ToUpper() + languageCountryCode.Remove(0, 1);

                countryCode = languageCountryCode;
                return true;
            }

            countryCode = null;
            return false;
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new SettingsServiceData()));
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}
