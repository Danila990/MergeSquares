using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CloudServices
{
    [RequireComponent(typeof(Toggle))]
    public class CloudToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private bool isLogs;

        private CloudService _cloudService;

        [Inject]
        public void Construct(CloudService cloudService)
        {
            _cloudService = cloudService;
            toggle.isOn = isLogs ? _cloudService.ShowLog : _cloudService.NeedWatch;
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool change)
        {
            if (isLogs)
            {
                _cloudService.CloudProvider.SetShowLogs(change);
            }
            else
            {
                _cloudService.CloudProvider.SetNeedWatch(change);
            }
            
        }
    }
}