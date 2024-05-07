using System;
using System.Collections.Generic;
using System.Linq;
using Core.Localization;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameStats;
using LeadboardScores;
using Mono.CSharp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class LeadboardStatLine : MonoBehaviour
    {
        // [SerializeField] private TextMeshProUGUI tryCountText;
        // [SerializeField] private TextMeshProUGUI sizeText;
        // [SerializeField] private TextMeshProUGUI maxPointsText;
        // [SerializeField] private Transform unitRoot;
        // [SerializeField] private UnitView unitPrefab;
        // [SerializeField] private bool isRating;
        // [SerializeField] private LocalizeUi ratingTitle;
        // [SerializeField] private LocalizeUi ratingPlace;
        // [SerializeField] private Transform rewardRoot;
        // [SerializeField] private List<RewardModel> rewardPrefabs = new();
        // [SerializeField] private List<RewardView> rewardViews;
        // [SerializeField] private SquaresSkinsManager squaresSkinsManager;
        // [SerializeField] private TextMeshProUGUI positionText;
        // [SerializeField] private LocalizationRepository localizationRepository;
        [SerializeField] private Button claimRewardButton;
        [SerializeField] private GameObject showAllPos;
        // [SerializeField] private Button startLevel;
        [SerializeField] private AllRewardsPopup allRewardsPopupPrefab;
        [SerializeField] private RewardView rewardStatViewPrefab;
        [SerializeField] private RewardView rewardSkinViewPrefab;
        [SerializeField] private Transform root;
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI scoresText;
        [SerializeField] private SquaresSkinsManager squaresSkinsManager;

        // private Action<InfinityGridModel> Click = (m) => {};
        // private Action<InfinityGridModel> Delete = (m) => {};
        public Action<LeadboardStatLine> Delete = (LeadboardStatLine) => {};

        // private InfinityGridModel _gridModel;
        // private int _ratingSeed;
        // private int _position;
        private List<RewardData> rewards = new List<RewardData>();
        private LeadboardRewardData data;

        // private RatingService _ratingService;
        private GameStatService _gameStatService;
        private GridManager _gridManager;
        private WindowManager _windowManager;
        private LeadboardScoresService _leadboardScoresService;

        [Inject]
        public void Construct(WindowManager windowManager, GridManager gridManager, GameStatService gameStatService,
            LeadboardScoresService leadboardScoresService)
        {
            _windowManager = windowManager;
            _gridManager = gridManager;
            _gameStatService = gameStatService;
            _leadboardScoresService = leadboardScoresService;
        }

        public void Init(List<RewardData> rewards, LeadboardRewardData data, bool isToClaim = false)
        {
            this.data = data;
            this.rewards = rewards;
            claimRewardButton.gameObject.SetActive(isToClaim);
            showAllPos.SetActive(!isToClaim);
            positionText.text = data.placeInRating.ToString();
            scoresText.text = data.scores.ToString();
            foreach (var reward in rewards)
            {
                switch (reward.type)
                {
                    case ERewardViewType.Stat:
                        var viewStat = Instantiate(rewardStatViewPrefab, root);
                        var textCount = $"{reward.baseAmount}";
                        viewStat.Init(reward.statType, textCount);
                        break;
                    case ERewardViewType.UnitSkin:
                        var viewSkin = Instantiate(rewardSkinViewPrefab, root);
                        var rarity = squaresSkinsManager.GetRarity(reward.rarity);
                        var textSkin = $"{reward.baseAmount}";
                        viewSkin.Init(textSkin, rarity.color, squaresSkinsManager.GetRarityText(reward.rarity));
                        break;
                }
            }
        }

        private void ShowAllRewards(bool isMonth)
        {
            var pos = _leadboardScoresService.GetCurrentPosition(isMonth);
            var allRewardParams = new AllRewardsParams()
            {
                position = pos,
                GetRewards = _leadboardScoresService.CalcRewards,
                isMonth = isMonth,
            };
            var popupParams = new GenericPopupParams
            {
                prefabToCreate = allRewardsPopupPrefab,
                dataToInitIt = allRewardParams,
            };
            var window = _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
            window.Canvas.sortingOrder = 320;
        }

        public void OnDelete()
        {
            Delete.Invoke(this);
            Destroy(gameObject);
        }

        public void ShowAllRewards()
        {
            var allRewardParams = new AllRewardsParams()
            {
                position = data.placeInRating,
                GetRewards = _leadboardScoresService.CalcRewards,
                isMonth = data.isMonth,
            };
            var popupParams = new GenericPopupParams
            {
                prefabToCreate = allRewardsPopupPrefab,
                dataToInitIt = allRewardParams,
            };
            var window = _windowManager.ShowWindow(EPopupType.GenericPopup.ToString(), new[] { popupParams });
            window.Canvas.sortingOrder = 320;
        }

        public void ClaimReward()
        {
            // var rewards = _leadboardScoresService.Data.rewardsInProgress.First().rewardsToClaim;
            foreach (var reward in rewards)
            {
                switch (reward.type)
                {
                    case ERewardViewType.Stat:
                        _gameStatService.TryIncWithAnim(reward.statType, reward.baseAmount);
                        break;
                    case ERewardViewType.UnitSkin:
                        _gridManager.OpenSkinByRarity(reward.rarity);
                        break;
                }
            }

            var rewardData = _leadboardScoresService.Data.rewardsToClaim.Find(r => r.id == data.id);
            _leadboardScoresService.Data.rewardsToClaim.Remove(rewardData);
            OnDelete();
        }
    }
}
