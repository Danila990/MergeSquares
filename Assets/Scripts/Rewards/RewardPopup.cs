using System.Collections.Generic;
using Core.Audio;
using Core.Localization;
using Core.Windows;
using Rewards.Models;
using UnityEngine;
using Zenject;

public enum EAwardWindowPhrases {AGift, Reward} 
namespace Rewards
{
    public class RewardPopupParams
    {
        public List<RewardModel> rewards = new();
    }

    public class RewardPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private Transform rewardRowRoot;
        [SerializeField] private RewardWindow rewardRowPrefab;
        [SerializeField] private float startTime;
        [SerializeField] private float pauseTime;
        [SerializeField] private LocalizeUi rewardText;
        [SerializeField] private SoundSource rewardSound;
        
        private bool _ready;
        private RewardPopupParams _args;

        private RewardService _rewardService;
        private RewardRepository _rewardRepository;
        private Dictionary<EAwardWindowPhrases, string> windowPhrases;

        [Inject]
        private void Construct(RewardService rewardService, RewardRepository rewardRepository)
        {
            _rewardService = rewardService;
            _rewardRepository = rewardRepository;

            popupBase.Inited += Init;
            popupBase.ShowArgsGot += OnShowArgsGot;
            popupBase.Disposed += Dispose;
        }

        private void Awake()
        {
            windowPhrases = new Dictionary<EAwardWindowPhrases, string>
            {
               [EAwardWindowPhrases.AGift] = "GiftTitle",
               [EAwardWindowPhrases.Reward] = "RewardTitle"
            };
        }

        private void OnDestroy()
        {
            popupBase.Inited -= Init;
            popupBase.ShowArgsGot -= OnShowArgsGot;
            popupBase.Disposed -= Dispose;
        }

        public void Take()
        {
            _rewardService.AwardAll(_args.rewards);
            _ready = false;
            popupBase.CloseWindow();
        }

        private void OnShowArgsGot(object[] args)
        {
            if (args.Length > 0)
            {
                _args = args[0] as RewardPopupParams;
            }

            if (args.Length > 1)
            {
                windowPhrases.TryGetValue((EAwardWindowPhrases)args[1], out var value);
                rewardText.SetLocalizationKey(value);
            }
            else
            {
                windowPhrases.TryGetValue(EAwardWindowPhrases.Reward, out var value);
                rewardText.SetLocalizationKey(value);
            }
        }
        
        private void Init()
        {
            _ready = false;
            if (_args == null)
            {
                return;
            }

            var rewardViews = new List<RewardViewData>();

            foreach (var rewardModel in _args.rewards)
            {
                var resourceModel = _rewardRepository.GetById(rewardModel.id);
                var reward = rewardViews.Find(r => r.Mergeable(rewardModel));
                var count = resourceModel.rewardType == ERewardType.Unit ? 1 : rewardModel.value;
                if (reward != null)
                {
                    reward.count += count;
                }
                else
                {
                    var viewData = new RewardViewData
                    {
                        baseReward = rewardModel,
                        count = count
                    };
                    rewardViews.Add(viewData);
                }
            }

            
            var index = 0f;
            while (rewardViews.Count > 0)
            {
                var row = Instantiate(rewardRowPrefab, rewardRowRoot);
                var max = Mathf.Min(3, rewardViews.Count);
                row.Init(rewardViews.GetRange(0, max), true, startTime + pauseTime * index++);
                rewardViews.RemoveRange(0, max);
            }

            _ready = true;
            rewardSound.Play();
        }

        private void Dispose(PopupBaseCloseType closeType)
        {
            if (_ready)
            {
                _ready = false;
                _rewardService.AwardAll(_args.rewards);
            }
        }
    }
}