using UnityEngine;

namespace Core.Windows
{
    public enum PopupDisposeActionType
    {
        None = 0,
        Deactivate = 1
    }
    
    public class PopupAction : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private PopupDisposeActionType disposeActionType = PopupDisposeActionType.None;

        private void Start()
        {
            popupBase.Disposed += PopupBaseOnDisposed;
        }

        private void OnDestroy()
        {
            popupBase.Disposed -= PopupBaseOnDisposed;
        }

        private void PopupBaseOnDisposed(PopupBaseCloseType obj)
        {
            switch (disposeActionType)
            {
                case PopupDisposeActionType.Deactivate:
                    popupBase.gameObject.SetActive(false);
                    break;
            }
        }
    }
}