using UnityEngine;
using Utils;

namespace CloudServices
{
    public class CloudSwitch : MonoBehaviour
    {
        [SerializeField] private bool applyOnStart;
        [SerializeField] private CloudActivator activator;
        [SerializeField] private UnityEventBool enabled;
        [SerializeField] private UnityEventBool disabled;

        private void Start()
        {
            if (applyOnStart)
            {
                Apply();
            }
        }

        public void Apply()
        {
            var active = activator.CheckActive();
            enabled.Invoke(active);
            disabled.Invoke(!active);
        }
    }
}