using Core.Pool;
using Core.Windows;
using System.Collections.Generic;
using Tutorial;
using UnityEngine;
using Utils;
using Zenject;

namespace Settings
{
    public class ChooseLanguagePopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private ChooseLanguageButton prefabButton;
        [SerializeField] private Transform contentRoot;
        [SortingLayer]
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;

        private Pool<ChooseLanguageButton> _itemPool;

        private Dictionary<ChooseLanguageButton, string> _itemParams =
            new Dictionary<ChooseLanguageButton, string>();

        private SettingsService _settingsService;
        private TutorialService _tutorialService;

        [Inject]
        public void Construct(SettingsService settingsService, TutorialService tutorialService)
        {
            _settingsService = settingsService;
            _tutorialService = tutorialService;

            _itemPool = new Pool<ChooseLanguageButton>(
                () =>
                {
                    var item = Instantiate(prefabButton, contentRoot);
                    item.gameObject.SetActive(false);
                    return item;
                },
                0
            );

            popupBase.Inited += Init;
        }

        private void OnDestroy()
        {
            popupBase.Inited -= Init;
        }

        private void Init()
        {
            if (_tutorialService.HasActiveTutorial)
            {
                popupBase.SetCanvasSorting(sortingLayerName, sortingOrder);
            }
            
            _itemParams.Clear();

            foreach (var languageItem in _itemPool.GetActive())
            {
                Recycle(languageItem);
            }

            var languageList = _settingsService.GetLanguageList();
            for(int i = 0; i < languageList.Count; i++)
            {
                var language = languageList[i];

                var item = _itemPool.Take();
                _itemParams.Add(item, language);

                item.Init(language);
                item.ButtonClicked += OnItemClicked;

                item.transform.SetSiblingIndex(i);
                item.gameObject.SetActive(true);
            }
        }

        private void Recycle(ChooseLanguageButton button)
        {
            button.ButtonClicked -= OnItemClicked;

            button.gameObject.SetActive(false);
            _itemPool.Recycle(button);
        }

        private void OnItemClicked(ChooseLanguageButton button)
        {
            var language = _itemParams[button as ChooseLanguageButton];
            _settingsService.SetLanguage(language);

            popupBase.CloseWindow();
        }
    }
}
