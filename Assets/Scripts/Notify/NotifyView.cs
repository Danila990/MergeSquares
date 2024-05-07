using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Notify
{
    public class NotifyView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private List<NotifyRef> refs = new();

        private NotifyService _notifyService;
        
        [Inject]
        public void Construct(NotifyService notifyService)
        {
            _notifyService = notifyService;
            _notifyService.Activated += OnActivated;
            _notifyService.Deactivated += OnDeactivated;
        }

        private void OnDestroy()
        {
            _notifyService.Activated -= OnActivated;
            _notifyService.Deactivated -= OnDeactivated;
        }

        private void Start()
        {
            root.SetActive(IsAnyActive());
        }

        private void OnActivated(NotifyRef notifyRef)
        {
            root.SetActive(IsAnyActive());
        }
        
        private void OnDeactivated(NotifyRef notifyRef)
        {
            root.SetActive(IsAnyActive());
        }

        private bool IsAnyActive()
        {
            foreach (var notifyRef in refs)
            {
                if (_notifyService.IsNotifyActive(notifyRef))
                {
                    return true;
                }
            }
            return false;
        }
    }
}