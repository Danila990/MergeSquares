using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Utils
{
    [RequireComponent(typeof(Toggle))]
    public class PrefsToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;

        [Inject]
        public void Construct()
        {
            // Dirty hack to exec method in disabled mono, don't forget that obj graph is not completed yet
            toggle.isOn = PlayerPrefsExt.GetBool(gameObject.name, toggle.isOn);
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool change)
        {
            PlayerPrefsExt.SetBool(gameObject.name, toggle.isOn);
            PlayerPrefsExt.Save();
        }
    }
}