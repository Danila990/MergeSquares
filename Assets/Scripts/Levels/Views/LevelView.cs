using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Levels.Views
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string needUpKey;
        
        private LevelService _levelService;

        [Inject]
        private void Construct(LevelService levelService)
        {
            _levelService = levelService;
            _levelService.LevelChanged += ChangeValue;
            _levelService.LevelRewardedChanged += ChangeValue;
        }

        private void OnDestroy()
        {
            _levelService.LevelChanged -= ChangeValue;
            _levelService.LevelRewardedChanged -= ChangeValue;
        }

        private void Start()
        {
            UpdateState();
        }

        private void ChangeValue(int value)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            animator.SetBool(needUpKey, _levelService.HasReward(out _));
        }
    }
}