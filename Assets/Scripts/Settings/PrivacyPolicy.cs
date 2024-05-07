using System;
using System.Collections.Generic;
using System.Linq;
using Core.Localization;
using Core.SaveLoad;
using Core.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace Settings
{
    [Serializable]
    public class PrivacyLang
    {
        [SerializeField] public string lang;
        [SerializeField] public TextAsset privacyPolicy;
    }
    public class PrivacyPolicy : MonoBehaviour
    {
        [SerializeField] private Button okButton;
        [SerializeField] private Button agreeButton;
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private LocalizeUi localizeUI;
        [SerializeField] private string gameName;
        [SerializeField] private List<PrivacyLang> privacyPolicys;
        // [SerializeField] private TextAsset privacyPolicy;
        [SerializeField] private ScrollRect scrollRect;
        [SortingLayer] [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;
        [Space] [SerializeField] private bool isTermsOfUse;

        private SaveService _saveService;
        private SettingsService _settingsService;

        [Inject]
        public void Construct(SaveService saveService, SettingsService settingsService)
        {
            _saveService = saveService;
            _settingsService = settingsService;
            popupBase.Inited += OnInit;
            popupBase.Disposed += OnDisposed;
        }

        private void OnDestroy()
        {
            popupBase.Inited -= OnInit;
            popupBase.Disposed -= OnDisposed;
        }

        private void OnEnable()
        {
            if (_saveService.GetAgreementState(isTermsOfUse))
            {
                agreeButton.gameObject.SetActive(false);
                okButton.gameObject.SetActive(true);
            }

            var privacyLang = privacyPolicys.Find(p => p.lang == _settingsService.Language);
            if (privacyLang == null)
            {
                privacyLang = privacyPolicys.First();
            }
            contentText.text = privacyLang.privacyPolicy.text;
        }

        private void Start()
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
            localizeUI.UpdateArgs(new[] { gameName });
        }

        private void OnInit()
        {
            popupBase.SetCanvasSorting(sortingLayerName, sortingOrder);
        }

        private void OnDisposed(PopupBaseCloseType obj)
        {
            _saveService.SetAgreementState(isTermsOfUse);
        }
    }
}