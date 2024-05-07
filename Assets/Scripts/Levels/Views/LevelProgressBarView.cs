using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Levels.Views
{
    public class LevelProgressBarView : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        
        private LevelService _levelService;
        
        [Inject]
        private void Construct(LevelService levelService)
        {
            _levelService = levelService;
            _levelService.ExperienceChanged += ChangeValue;
        }

        private void Start()
        {
            if (_levelService.Ready)
            {
                ChangeValue(_levelService.Experience);
            }
        }

        private void OnDestroy()
        {
            _levelService.ExperienceChanged -= ChangeValue;
        }
        
        private void ChangeValue(int value)
        {
            slider.value = (float) value / _levelService.MaxExperience;
        }
    }
}