using Core.Windows;
using UnityEngine;
using Zenject;

namespace Core.SaveLoad.Popups
{
    public class SaveLoadTestPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;

        [SerializeField] private SaveLoadContentView saveLoadContentView;
        [SerializeField] private CreateSavePanelView createSavePanelView;

        private SaveService _saveService;
        
        [Inject]
        public void Construct(SaveService saveService)
        {
            _saveService = saveService;
            popupBase.Inited += Init;
        }

        private void Init()
        {
            var keys = _saveService.SaveKeys;
            
            InitSaveLoadContent();
            InitCreateSavePanel();

            void InitSaveLoadContent()
            {
                saveLoadContentView.Clear();
                saveLoadContentView.Init(
                    _saveService.CurrentKey,
                    keys,
                    _saveService.SaveTo,
                    key =>
                    {
                        if (_saveService.TryChooseSave(key))
                        {
                            createSavePanelView.UpdateCurrentKey(key);
                        }
                    },
                    key =>
                    {
                        if (_saveService.TryChooseSave(key))
                        {
                            createSavePanelView.UpdateCurrentKey(key);
                        }
                    }
                );
            }

            void InitCreateSavePanel()
            {
                createSavePanelView.OnSaveCreated += CreateSave;
                createSavePanelView.UpdateCurrentKey(_saveService.CurrentKey);

                void CreateSave(string key, bool empty)
                {
                    _saveService.CreateSave(key, empty);
                    InitSaveLoadContent();
                }
            }
        }
    }
}