using UnityEngine;

namespace CloudServices
{
    public class CloudService : MonoBehaviour
    {
        [SerializeField] private CloudProviderBase cloudProvider;

        public CloudProviderBase CloudProvider => cloudProvider;
        public bool NeedWatch => CloudProvider.NeedWatch;
        public bool ShowLog => CloudProvider.ShowLog;
    }
}