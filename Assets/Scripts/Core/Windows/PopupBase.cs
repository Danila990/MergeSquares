using System;
using UnityEngine;
using Plugins.WindowsManager;
using Zenject;

namespace Core.Windows
{
    public enum PopupBaseCloseType
    {
        Opened = -1,
        None = 0,
        Close = 1,
        Screen = 2,
    }
    
    public class PopupBase : Window<PopupBase>
    {
        [SerializeField] private EPopupType windowId;
        [SerializeField] private string hideKey;
        [SerializeField] private PopupAnimator popupAnimator;

        public override string WindowId => IsGeneric ? GenericId : windowId.ToString();
        public event Action Inited = () => { };
        
        public Action BeforeCloseWindow = () => {};
        public event Action<object[]> ShowArgsGot = args => {};
        public event Action<PopupBaseCloseType> Disposed = closeType => { };

        public bool IsGeneric { get; set; } = false;
        public string GenericId { get; set; } = "UnknownIdSetIt";

        private PopupBaseCloseType _closeType = PopupBaseCloseType.Opened;
        
        private WindowManager _windowManager;

        [Inject]
        public void Construct
        (
            WindowManager windowManager,
            UnityEngine.Camera worldCamera
        )
        {
            _windowManager = windowManager;
            Canvas.worldCamera = worldCamera;
        }

        public void CloseWindow()
        {
            CloseWindow(PopupBaseCloseType.None);
        }
        
        public void CloseWindowWithScreenTap()
        {
            CloseWindow(PopupBaseCloseType.Screen);
        }
        
        public void CloseWindowWithCloseButton()
        {
            CloseWindow(PopupBaseCloseType.Close);
        }

        public void CloseWindow(PopupBaseCloseType closeType, bool immediately = false)
        {
            if (_closeType == closeType)
            {
                return;
            }
            
            _closeType = closeType;
            BeforeCloseWindow.Invoke();

            if (immediately)
                Close();
            else
                popupAnimator.AnimateHide(() => Close());
        }

        public override void Activate(bool immediately = false)
        {
            ActivatableState = ActivatableState.Active;
            _closeType = PopupBaseCloseType.Opened;
            Canvas.ForceUpdateCanvases();
            gameObject.SetActive(true);
            
            if (!immediately)
                popupAnimator.AnimateShow();
            
            Inited.Invoke();
        }

        public override void Deactivate(bool immediately = false)
        {
            ActivatableState = ActivatableState.Inactive;
            Disposed.Invoke(_closeType);
        }

        public override void SetArgs(object[] args)
        {
            if (args != null)
            {
                ShowArgsGot.Invoke(args);
            }
        }

        public void SetCanvasSorting(string layerName, int order)
        {
            Canvas.overrideSorting = true;
            Canvas.sortingLayerName = layerName;
            Canvas.sortingOrder = order;
        }
        
        private void Close()
        {
            if (ActivatableState != ActivatableState.Inactive)
            {
                _windowManager.CloseAll(WindowId);
            }
        }
    }
}