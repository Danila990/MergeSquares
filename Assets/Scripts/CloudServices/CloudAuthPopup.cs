using System.Linq;
using Core.Localization;
using Core.Windows;
using UnityEngine;
using Zenject;

namespace CloudServices
{
    public class CloudAuthPopupParams
    {
        public bool firstAuth;
    }
    
    public class CloudAuthPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private string firstAuthQuestKey;
        [SerializeField] private string firstAuthKey;
        [SerializeField] private LocalizeUi questText;
        [SerializeField] private LocalizeUi text;

        private CloudAuthPopupParams _popupParams;
        
        private CloudService _cloudService;
    
        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
            popupBase.ShowArgsGot += OnShowArgsGot;
        }

        private void OnDestroy()
        {
            popupBase.ShowArgsGot -= OnShowArgsGot;
        }

        public void Auth()
        {
            _cloudService.CloudProvider.Auth();
        }
        
        private void OnShowArgsGot(object[] args)
        {
            if(args.Length > 0)
            {
                _popupParams = args.First() as CloudAuthPopupParams;
                if (_popupParams.firstAuth)
                {
                    text.SetLocalizationKey(firstAuthKey);
                    questText.SetLocalizationKey(firstAuthQuestKey);
                }
            }
        }
    }
}
