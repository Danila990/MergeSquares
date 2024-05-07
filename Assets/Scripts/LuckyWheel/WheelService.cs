using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Advertising;
using CloudServices;
using Core.Localization;
using Core.Windows;
using GameScripts.Game2248;
using GameScripts.MergeSquares.Shop;
using GameScripts.PointPanel;
using GameStats;
using Rewards;
using UnityEngine;
using Zenject;
using ESquareSkin = GameScripts.Game2248.ESquareSkin;
using SquaresSkin = GameScripts.Game2248.Shop.SquaresSkin;
using SquaresSkinsManager = GameScripts.Game2248.Shop.SquaresSkinsManager;
using SummonSkinChance = GameScripts.Game2248.Shop.SummonSkinChance;

namespace LuckyWheel
{
    public class WheelService : MonoBehaviour
    {
        [SerializeField] private List<WheelParams> wheelParams;
        [SerializeField] private SquaresSkinsManager skinsManager;

        public Action<float> SetGiftProgress = f => { };

        private Dictionary<String, ESquareSkin> skinsIds = new Dictionary<string, ESquareSkin>();
        
        private WindowManager _windowManager;
        private RewardService _rewardService;
        private GridManager _gridManager;
        private LocalizationRepository _localizationRepository;
        private GameStatService _gameStatService;

        [Inject]
        private void Construct(WindowManager windowManager, RewardService rewardService, GridManager gridManager,
            LocalizationRepository localizationRepository, GameStatService gameStatService)
        {
            _windowManager = windowManager;
            _rewardService = rewardService;
            _gridManager = gridManager;
            _localizationRepository = localizationRepository;
            _gameStatService = gameStatService;
        }

        private void Start()
        {
            foreach (ESquareSkin eSkin in Enum.GetValues(typeof(ESquareSkin)))
            {
                skinsIds.Add(eSkin.ToString().ToLower(), eSkin);
            }
        }

        public void ShowWheel(string id)
        {
            var param = wheelParams.Find(p => p.id == id);
            if (param == null)
            {
                Debug.LogError($"[WheelService][ShowWheel] Not found param with id: {id}. Used first param");
                param = wheelParams.First();
            }
            _windowManager.ShowWindow(EPopupType.LuckyWheelPopup.ToString(), new[] { param });
        }

        public bool CheckSkin(WheelReward reward)
        {
            var id = reward.rewardModel.unit.ToLower();
            return (skinsIds.TryGetValue(id, out var skinType) && !_gridManager.OpenedSkins.Contains(skinType));
        }

        public ESquareSkin GetSkinId(WheelReward reward)
        {
            var id = reward.rewardModel.unit.ToLower();
            skinsIds.TryGetValue(id, out var skinType);
            return skinType;
        }
        
        public SummonSkinChance GetSummonSkinChance(ESquareSkin skinType)
        {
            SquaresSkin skin = null;
            foreach (var skinElem in skinsManager.Skins)
            {
                if (skinElem.Skin == skinType)
                {
                    skin = skinElem;
                    break;
                }
            }
            var rarity = skinsManager.GetRarity(skin.Rarity);
            var skinChance = new SummonSkinChance
            {
                type = skin.Skin,
                rarity = rarity
            };
            return skinChance;
        }

        public string GetRarityText(ESkinRarity rarity)
        {
            return _localizationRepository.GetTextInCurrentLocale($"{rarity.ToString()}Name").Substring(0, 1);
        }

        public void GetReward(Sector sector)
        {
            var reward = sector.WheelReward;
            switch (reward.rewardResource.rewardType)
            {
                case ERewardType.GameStat:
                    // _rewardService.Award(reward.rewardModel);
                    if (_windowManager.TryShowAndGetWindow<CoinsAddPopup>(EPopupType.CoinsAdd.ToString(), out var coinsAddPopup))
                    {
                        coinsAddPopup.SetArgs(reward.rewardModel.value, () =>{});
                        _gameStatService.TryIncWithAnim(EGameStatType.Soft, reward.rewardModel.value);
                    }
                    break;
                case ERewardType.Unit:
                    SummonSkinChance skinChance;
                    var id = reward.rewardModel.unit.ToLower();
                    skinsIds.TryGetValue(id, out var skinType);
                    // SquaresSkin skin = null;
                    // foreach (var skinElem in skinsManager.Skins)
                    // {
                    //     if (skinElem.Skin == skinType)
                    //     {
                    //         skin = skinElem;
                    //     }
                    // }
                    // var rarity = skinsManager.GetRarity(skin.Rarity);
                    // skinChance = new SummonSkinChance
                    // {
                    //     type = skin.Skin,
                    //     rarity = rarity
                    // };
                    var skin = GetSummonSkinChance(skinType);
                            
                    SkinAddPopup skinAddPopup;
                            
                    _gridManager.OpenSkin(skinType);
                    _gridManager.SaveGrid();
                            
                    if (_windowManager.TryShowAndGetWindow(EPopupType.SkinAdd.ToString(), out skinAddPopup))
                    {
                        var text = GetRarityText(skin.rarity.rarity);
                        skinAddPopup.SetArgs(skinType, skin.rarity.color, text,() => {});
                    }
                    break;
                case ERewardType.Gift:
                    _rewardService.Award(reward.rewardModel);
                    break;
            }
        }
    }
}
