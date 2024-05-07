using System;
using Core.SaveLoad;
using Core.Windows;
using TMPro;
using Tutorial;
using UnityEngine;
using Zenject;
using UI;
using Utils;
using UnityEngine.UI;

namespace Settings
{
    [Serializable]
    public class ContackMailData
    {
        public string email;
        public string subject;
    }
    public class SettingsPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private SwitchImageButton soundButton;
        [SerializeField] private SwitchImageButton musicButton;
        [SerializeField] private Slider soundSlider;
        [SerializeField] private Slider musicSlider;
        [SortingLayer]
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;
        [SerializeField] private ContackMailData mailData;
        [SerializeField] private TextMeshProUGUI mailText;
        [SerializeField] private TextMeshProUGUI versionText;

        private SettingsService _settingsService;

        [Inject]
        public void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;
            versionText.text = $"Version {Application.version}";

            popupBase.Inited += OnInit;
            _settingsService.SettingsChanged += OnSettingsChanged;
        }

        public void OnSaveReset()
        {
            SaveService.ResetAllSaveData();
        }
        
        public void OnSoundStateChanged()
        {
            var state = _settingsService.InvertSoundState();
            soundButton.UpdateStateImage(state);
        }

        public void OnSoundSliderChanged()
        {
            var value = soundSlider.value;
            if (value > 0)
            {
                _settingsService.SetSoundsVolume(value);
            }
            else
            {
                _settingsService.InvertSoundState();
            }
        }

        public void OnMusicStateChanged()
        {
            var state = _settingsService.InvertMusicState();
            musicButton.UpdateStateImage(state);
        }

        public void OnMusicSliderChanged()
        {
            var value = musicSlider.value;
            if (value > 0)
            {
                _settingsService.SetMusicVolume(value);
            }
            else
            {
                _settingsService.InvertMusicState();
            }
        }

        public void ContactUs()
        {
            Application.OpenURL("mailto:" + mailData.email + "?subject=" + mailData.subject);
        }

        private string MyEscapeURL (string url) 
        {
            return WWW.EscapeURL(url).Replace("+","%20");
        }

        private void OnDestroy()
        {
            popupBase.Inited -= OnInit;
            _settingsService.SettingsChanged -= OnSettingsChanged;
        }
        
        private void OnInit()
        {
            UpdateState();
            mailText.text = mailData.email;
        }

        private void UpdateState()
        {
            soundButton.UpdateStateImage(_settingsService.SoundState);
            musicButton.UpdateStateImage(_settingsService.MusicState);
            musicSlider.SetValueWithoutNotify(_settingsService.MusicVolume);
            soundSlider.SetValueWithoutNotify(_settingsService.SoundVolume);
        }
        
        private void OnSettingsChanged()
        {
            UpdateState();
        }
    }
}
