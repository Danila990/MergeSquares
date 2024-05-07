using System;
using System.Collections;
using System.Collections.Generic;
using GameScripts.MergeSquares.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class RewardLine : MonoBehaviour
    {
        // [SerializeField] private List<RewardView> rewardViews;
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private SquaresSkinsManager squaresSkinsManager;
        [SerializeField] private RectTransform rect;
        [SerializeField] private RewardView rewardStatViewPrefab;
        [SerializeField] private RewardView rewardSkinViewPrefab;
        [SerializeField] private Transform root;
        // [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject activePos;
        [SerializeField] private TextMeshProUGUI activePositionText;

        public RectTransform Rect => rect;
        
        public void Init(List<RewardData> rewardsMin, List<RewardData> rewardsMax, string position)
        {
            for (int i = 0; i < rewardsMin.Count; i++)
            {
                switch (rewardsMin[i].type)
                {
                    case ERewardViewType.Stat:
                        // var viewStat = rewardViews.Find(e => e.Type == ERewardViewType.Stat && e.GameStatType == rewardsMin[i].statType);
                        var viewStat = Instantiate(rewardStatViewPrefab, root);
                        var maxCount = rewardsMax.Find(r => r.type == ERewardViewType.Stat && r.statType == rewardsMin[i].statType).baseAmount;
                        var textCount = $"{rewardsMin[i].baseAmount}-{maxCount}";
                        viewStat.Init(rewardsMin[i].statType, textCount);
                        break;
                    case ERewardViewType.UnitSkin:
                        var viewSkin = Instantiate(rewardSkinViewPrefab, root);
                        // var viewSkin = rewardViews.Find(e => e.Type == ERewardViewType.UnitSkin);
                        var rarity = squaresSkinsManager.GetRarity(rewardsMin[i].rarity);
                        var maxSkin = rewardsMax.Find(r => r.type == ERewardViewType.UnitSkin).baseAmount;
                        var textSkin = $"{rewardsMin[i].baseAmount}-{maxSkin}";
                        viewSkin.Init(textSkin, rarity.color, squaresSkinsManager.GetRarityText(rewardsMin[i].rarity));
                        break;
                }
            }

            positionText.text = position;
        }

        public void SetThisPos(bool isActive, int pos = 0)
        {
            activePos.SetActive(isActive);
            activePositionText.text = pos.ToString();
        }
        //
        // private string GetRarityText(ESkinRarity rarity)
        // {
        //     return localizationRepository.GetTextInCurrentLocale($"{rarity.ToString()}Name").Substring(0, 1);
        // }

    }
}