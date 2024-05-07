using Core.Localization;
using GameScripts.MergeSquares.Shop;
using Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts.Game2248.Shop
{
    public class RarityView : RarityViewBase
    {
        [SerializeField] private SquaresSkinsManager skinsRepo;

        public void UpdateView(int level = 0)
        {
            var rarityData = skinsRepo.GetRarity(rarity);
            rarityName.SetLocalizationKey($"{rarity.ToString()}Name");
            icon.color = rarityData.color;
            var chance = rarityData.chanceForLevel[level];
            rarityChance.text = $"{chance:F3}";
        }
    }
}