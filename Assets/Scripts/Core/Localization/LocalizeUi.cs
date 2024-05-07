using System;
using TMPro;
using UnityEngine;
using Utils;
using Zenject;

namespace Core.Localization
{
    public class LocalizeUi : MonoBehaviour
    {
        [SerializeField] private string fieldName;

        private TextMeshProUGUI _text;
        private bool _hidden;
        private object[] _args;
        private LocalizationRepository _localizationRepository;

        [Inject]
        public void Construct
        (
            LocalizationRepository localizationRepository
        )
        {
            _localizationRepository = localizationRepository;
            _localizationRepository.LanguageChanged += ChangeLanguage;
        }

        private void OnDestroy()
        {
            if(_localizationRepository != null)
            {
                _localizationRepository.LanguageChanged -= ChangeLanguage;
            }
            else
            {
                Debug.LogWarning($"[LocalizeUi][OnDestroy] cant find _localizationRepository in: {gameObject.GetPath()}");
            }
        }

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            ChangeLanguage();
        }

        public void Hide()
        {
            _hidden = true;
            _text.text = string.Empty;
        }

        public void Show()
        {
            _hidden = false;
            ChangeLanguage();
        }

        public void SetLocalizationKey(string fieldName, object[] args = null)
        {
            this.fieldName = fieldName;
            _args = args;
            ChangeLanguage();
        }

        public string GetLocalizedText(string textKey)
        {
            if (_localizationRepository != null)
            {
                if (!string.IsNullOrEmpty(textKey))
                {
                    return _localizationRepository.GetTextInCurrentLocale(textKey);
                }
            }
            else
            {
                Debug.LogWarning($"[LocalizeUi][OnDestroy] cant find _localizationRepository in: {gameObject.GetPath()}");
            }
            
            return "";
        }
        
        public void UpdateArgs(object[] args)
        {
            _args = args;
            ChangeLanguage();
        }
        
        public void ResetArgs()
        {
            _args = null;
            ChangeLanguage();
        }

        private void ChangeLanguage()
        {
            if (!_hidden && _text != null && !string.IsNullOrEmpty(fieldName) && _localizationRepository != null)
            {
                var localeText = _localizationRepository.GetTextInCurrentLocale(fieldName);
                if (_args != null)
                {
                    localeText = String.Format(localeText, _args);
                }
                _text.text = localeText;
            }
        }
    }
}