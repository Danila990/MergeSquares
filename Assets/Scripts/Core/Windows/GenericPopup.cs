using System.Linq;
using UnityEngine;
using Zenject;

namespace Core.Windows
{
    public class GenericPopupParams
    {
        public GenericPopupContent prefabToCreate;
        public object dataToInitIt;
        public bool isDimmingActive = true;
    }
    public class GenericPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private Transform rootForContent;
        [SerializeField] private GameObject screenDimming;
        
        private GenericPopupParams _popupParams;
        private GenericPopupContent _content;
        private bool _closeWithError;
        
        [Inject]
        private void Construct()
        {
            popupBase.ShowArgsGot += OnShowArgsGot;
            popupBase.Inited += OnInited;
            popupBase.Disposed += Dispose;
        }

        private void OnDestroy()
        {
            popupBase.ShowArgsGot -= OnShowArgsGot;
            popupBase.Inited -= OnInited;
            popupBase.Disposed -= Dispose;
        }
        
        private void OnShowArgsGot(object[] args)
        {
            if(args.Length > 0 && args.First() is GenericPopupParams popupParams)
            {
                _popupParams = popupParams;
                _content = Instantiate(_popupParams.prefabToCreate, rootForContent);
                screenDimming.SetActive(popupParams.isDimmingActive);
                _content.Init(_popupParams.dataToInitIt, popupBase);
                popupBase.IsGeneric = true;
                popupBase.GenericId = _content.GetWindowId();
            }
            else
            {
                _closeWithError = true;
            }
        }
        
        private void OnInited()
        {
            if (_closeWithError)
            {
                popupBase.CloseWindow();
            }
        }
        
        private void Dispose(PopupBaseCloseType closeType)
        {
            if (_content != null)
            {
                _content.Dispose(closeType);
            }
        }
    }
}