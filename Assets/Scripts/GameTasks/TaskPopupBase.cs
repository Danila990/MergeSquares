using Core.Windows;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace GameTasks
{
    public class TaskPopupBase : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] protected GameObject dailyTab;
        [SerializeField] protected GameObject repeatedTab;
        [SerializeField] private Button dailyTabButton;
        [SerializeField] private Button dailyActiveTabButton;
        [SerializeField] private Button repeatedTabButton;
        [SerializeField] private Button repeatedActiveTabButton;

        private bool _isDaily = true;

        private void Start()
        {
            Switch();
        }
        
        protected virtual void UpdateTabs(){}

        [UsedImplicitly]
        public void SwitchTabTo(bool isDaily)
        {
            if(isDaily != _isDaily)
            {
                _isDaily = isDaily;
                Switch();
            }
        }

        private void Switch()
        {
            if (_isDaily)
            {
                dailyTab.gameObject.SetActive(true);
                repeatedTab.gameObject.SetActive(false);
                dailyActiveTabButton.gameObject.SetActive(false);
                dailyTabButton.gameObject.SetActive(true);
                repeatedActiveTabButton.gameObject.SetActive(true);
                repeatedTabButton.gameObject.SetActive(false);
            }
            else
            {
                dailyTab.gameObject.SetActive(false);
                repeatedTab.gameObject.SetActive(true);
                dailyActiveTabButton.gameObject.SetActive(true);
                dailyTabButton.gameObject.SetActive(false);
                repeatedActiveTabButton.gameObject.SetActive(false);
                repeatedTabButton.gameObject.SetActive(true);
            }

            UpdateTabs();
        }
    }
}