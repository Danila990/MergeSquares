using Core.Windows;
using UnityEngine;
using Zenject;

namespace Levels.Popups
{
    public class LevelUpButton : MonoBehaviour
    {
        private WindowManager _windowManager;
        
        [Inject]
        private void Construct(WindowManager windowManager, LevelService levelService)
        {
            _windowManager = windowManager;
        }
        
        public void OnClick()
        {
            _windowManager.ShowWindow(EPopupType.LevelUp.ToString());
        }
    }
}