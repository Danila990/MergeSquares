using System;
using System.Collections.Generic;
using System.Linq;
using CloudServices;
using Core.Anchors;
using Core.Localization;
using Core.Windows;
using GameScripts.MergeSquares.Shop;
using GameStats;
using Mono.CSharp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    [Serializable]
    public class RewardModel
    {
        public RewardView viewPrefab;
        public ERewardViewType type;
    }
    
    public class SeedPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tryCountText;
        [SerializeField] private TextMeshProUGUI sizeText;
        [SerializeField] private TextMeshProUGUI maxPointsText;
        [SerializeField] private TextMeshProUGUI notPlayedText;
        [SerializeField] private GameObject rewardsBackground;
        [SerializeField] private Transform unitRoot;
        [SerializeField] private UnitView unitPrefab;
        [SerializeField] private bool isRating;
        [SerializeField] private SquaresSkinsManager squaresSkinsManager;
        [SerializeField] private TextMeshProUGUI positionText;
        // [SerializeField] private LocalizationRepository localizationRepository;
        [SerializeField] private Button claimRewardButton;
        [SerializeField] private Button startLevel;
        [SerializeField] private AllRewardsPopup allRewardsPopupPrefab;
        [SerializeField] private RewardView rewardStatViewPrefab;
        [SerializeField] private RewardView rewardSkinViewPrefab;
        [SerializeField] private Transform root;
        [SerializeField] private Anchor anchor;
        [SerializeField] private TextMeshProUGUI tileText;
        [SerializeField] private LocalizationRepository localizationRepository;

        public Anchor Anchor => anchor;
        
        private Action<InfinityGridModel> Click = (m) => {};
        private Action<InfinityGridModel> Delete = (m) => {};
        
        private InfinityGridModel _gridModel;
        private int _ratingSeed;
        private int _position;
        private bool _current;
        private List<RewardData> rewards = new List<RewardData>();
        private ClaimRewardData data;
        
        private RatingService _ratingService;
        private GameStatService _gameStatService;
        private GridManager _gridManager;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(WindowManager windowManager, GridManager gridManager, GameStatService gameStatService)
        {
            _windowManager = windowManager;
            _gridManager = gridManager;
            _gameStatService = gameStatService;
        }
        
        public void Init(InfinityGridModel model, Action<InfinityGridModel> OnClick, Action<InfinityGridModel> OnDelete)
        {
            _gridModel = model;
            SetTexts();
            Click += OnClick;
            Delete += OnDelete;
            _ratingService = ZenjectBinding.FindObjectOfType<RatingService>();
            // _gameStatService = ZenjectBinding.FindObjectOfType<GameStatService>();
            // _gridManager = ZenjectBinding.FindObjectOfType<GridManager>();
        }
        
        public void Init(ClaimRewardData rewardData, Action<InfinityGridModel> OnClick, Action<InfinityGridModel> OnDelete)
        {
            if (rewardData.isMonth)
                tileText.text = localizationRepository.GetTextInCurrentLocale("MonthlyText");
            else
                tileText.text = localizationRepository.GetTextInCurrentLocale("WeeklyText");
            // tileText.text = $"{rewardData.id}";
            data = rewardData;
            _gridManager = ZenjectBinding.FindObjectOfType<GridManager>();
            _gridModel = _gridManager.GetExternalGrid(Convert.ToInt32(rewardData.id));
            SetTexts();
            Click += OnClick;
            Delete += OnDelete;
            _ratingService = ZenjectBinding.FindObjectOfType<RatingService>();
            rewards = rewardData.rewardsToClaim;
            _position = rewardData.placeInRating;
            if (isRating)
            {
                if (data.placeInRating <= 0)
                {
                    SetNotPlayed();
                }
                else
                {
                    SetRewards();
                }
                claimRewardButton.gameObject.SetActive(isRating);
                positionText.text = rewardData.placeInRating.ToString();
            }
            // _gameStatService = ZenjectBinding.FindObjectOfType<GameStatService>();
            // _gridManager = ZenjectBinding.FindObjectOfType<GridManager>();
        }

        public void SetIsToClaim(bool isToClaim)
        {
            claimRewardButton.gameObject.SetActive(isToClaim);
            startLevel.gameObject.SetActive(!isToClaim);
            // startLevel.enabled = !isToClaim;
            _current = !isToClaim;
        }

        public void OnClick()
        {
            Click.Invoke(_gridModel);
        }

        public void OnDelete()
        {
            Delete.Invoke(_gridModel);
            Destroy(gameObject);
        }

        public void OnOpenTable()
        {
            _ratingService.OpenTable(data.isMonth, _current);
        }

        public void ShowAllRewards()
        {
            var allRewardParams = new AllRewardsParams()
            {
                position = _position,
                GetRewards = _ratingService.CalcRewards,
                isMonth = data.isMonth
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
            // var rewards = _ratingService.Data.rewardsInProgress.First().rewardsToClaim;
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

            _ratingService.RemoveReward(data.id);
            OnDelete();
        }
        
        private void SetTexts()
        {
            tryCountText.text = _gridModel.retryCount.ToString();
            sizeText.text = $"{_gridModel.model.size.x}x{_gridModel.model.size.y}";
            maxPointsText.text = _gridModel.bestScore.ToString();
            var totalChance = 0f;
            foreach (var model in _gridModel.model.nextValues)
            {
                totalChance += model.chance;
            }

            // foreach (var view in unitRoot.GetComponentsInChildren<UnitView>())
            // {
            //     Destroy(view.gameObject);
            // }
            //
            // foreach (var view in unitRoot.GetComponentsInChildren<RewardView>())
            // {
            //     Destroy(view.gameObject);
            // }
            
            if(!isRating)
            {
                foreach (var model in _gridModel.model.nextValues)
                {
                    var unit = Instantiate(unitPrefab, unitRoot);
                    unit.Init(model.value);
                    var node = unit.transform.Find("Chance");
                    if (node != null)
                    {
                        var text = node.GetComponent<TextMeshProUGUI>();
                        if (text != null)
                        {
                            var chance = 100f * model.chance / totalChance;
                            text.text = chance < 10f
                                ? (chance < 5f ? $"{chance:F2}%" : $"{chance:F1}%")
                                : $"{chance:F0}%";
                        }
                    }
                }
            }
            else
            {
                
            }
        }

        private void SetNotPlayed()
        {
            positionText.gameObject.SetActive(false);
            tryCountText.gameObject.SetActive(false);
            sizeText.gameObject.SetActive(false);
            maxPointsText.gameObject.SetActive(false);
            notPlayedText.gameObject.SetActive(true);
            rewardsBackground.gameObject.SetActive(false);
        }

        private void SetRewards()
        {
            // var rewards = _ratingService.Data.rewardsInProgress.First().rewardsToClaim;
            // Debug.Log($"123  MAIN COUNT:  {rewards.Count}");
            // foreach (var reward in rewards)
            // {

            foreach (var reward in rewards)
            {
                // Debug.Log($"123 {rr.type}  {rr.statType}  {rr.baseAmount}  {rr.rarity}");
                switch (reward.type)
                {
                    case ERewardViewType.Stat:
                        // var viewStat = rewardViews.Find(e => e.Type == ERewardViewType.Stat && e.GameStatType == reward.statType);
                        var viewStat = Instantiate(rewardStatViewPrefab, root);
                        viewStat.Init(reward.statType, reward.baseAmount);
                        break;
                    case ERewardViewType.UnitSkin:
                        // var viewSkin = rewardViews.Find(e => e.Type == ERewardViewType.UnitSkin);
                        var viewSkin = Instantiate(rewardSkinViewPrefab, root);
                        var rarity = squaresSkinsManager.GetRarity(reward.rarity);
                        viewSkin.Init(reward.baseAmount, rarity.color,
                            squaresSkinsManager.GetRarityText(reward.rarity));
                        break;
                }
            }
        }
    }
}
