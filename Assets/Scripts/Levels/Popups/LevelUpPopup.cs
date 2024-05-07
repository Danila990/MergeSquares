using System.Linq;
using Advertising;
using Core.Windows;
using Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Levels.Popups
{
    public class LevelUpPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private RewardWindow mainRewardWindow;
        [SerializeField] private RewardWindow adRewardWindow;
        [SerializeField] private GameObject takeWithoutButton;
        [SerializeField] private GameObject takeWithAdButton;
        [SerializeField] private TextMeshProUGUI currentLevel;
        [SerializeField] private RewardAdButton rewardAdButton;
        [SerializeField] private Button noThanksButton;

        private bool _ready;
        
        private LevelService _levelService;
        
        [Inject]
        private void Construct(LevelService levelService)
        {
            _levelService = levelService;
            
            popupBase.Inited += Init;
            popupBase.Disposed += Dispose;
        }

        private void OnDestroy()
        {
            popupBase.Inited -= Init;
            popupBase.Disposed -= Dispose;
        }
        
        public void TakeWithAds()
        {
            rewardAdButton.Rewarded += OnRewarded;
            rewardAdButton.Failed += OnFailed;
            rewardAdButton.ShowAd();
        }
        
        public void TakeWithoutAds()
        {
            _levelService.TakeReward(false);
            _ready = false;
            popupBase.CloseWindow();
        }

        private void Init()
        {
            _ready = false;
            if (!_levelService.HasReward(out var level))
            {
                popupBase.CloseWindow();
                return;
            }
            
            Clear();

            currentLevel.text = (level.id + 1).ToString();
            mainRewardWindow.Init(level.rewards.FindAll(r => !r.isAdditional));
            adRewardWindow.Init(level.rewards.FindAll(r => r.isAdditional));
            
            var availableAdReward = level.rewards.Any(reward => reward.isAdditional);
            
            if (availableAdReward)
            {
                adRewardWindow.gameObject.SetActive(true);
                takeWithAdButton.SetActive(true);
                noThanksButton.gameObject.SetActive(true);
            }
            else
            {
                takeWithoutButton.SetActive(true);
                noThanksButton.gameObject.SetActive(false);
            }

            _ready = true;
        }

        private void Dispose(PopupBaseCloseType closeType)
        {
            if (_ready && closeType == PopupBaseCloseType.Close)
            {
                _ready = false;
                _levelService.TakeReward(false);
            }
        }

        private void Clear()
        {
            mainRewardWindow.Clear();
            adRewardWindow.Clear();
            
            adRewardWindow.gameObject.SetActive(false);
            takeWithoutButton.SetActive(false);
            takeWithAdButton.SetActive(false);
        }
        
        private void OnRewarded()
        {
            rewardAdButton.Rewarded -= OnRewarded;
            rewardAdButton.Failed -= OnFailed;
            _levelService.TakeReward(true);
            _ready = false;
            popupBase.CloseWindow();
        }

        private void OnFailed()
        {
            rewardAdButton.Rewarded -= OnRewarded;
            rewardAdButton.Failed -= OnFailed;
        }
    }
}