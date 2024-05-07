using UnityEngine;

namespace Core.Windows
{
    public abstract class GenericPopupContent : MonoBehaviour
    {
        protected WindowManager _windowManager;
        public abstract string GetWindowId();
        public abstract void Init(object dataToInit, PopupBase popupBase);
        public abstract void Dispose(PopupBaseCloseType closeType);
    }
}