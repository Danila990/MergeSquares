using System.Collections.Generic;
using Core.Localization;
using GameScripts.MergeSquares.Shop;
using GameStats;
using JetBrains.Annotations;
using Popups;
using UnityEngine;
using Zenject;

namespace GameScripts.Game2248.Shop
{
    public class SkinLevelsPopup : SkinLevelsPopupBase
    {
        [SerializeField] private SquaresSkinsManager skinsRepo;
        [SerializeField] protected List<RarityView> views = new();

        private void Start()
        {
            var rarityData = skinsRepo.GetRarity(ESkinRarity.Common);
            _levelIndex = _gameStatLeveled.GetLevel(levelType);
            _levelIndexMax = rarityData.chanceForLevel.Count - 1;
            UpdateViews();
        }

        protected override void UpdateViews()
        {
            {
                levelText.UpdateArgs(new []{$"{_levelIndex + 1}"});
                foreach (var rarityView in views)
                {
                    rarityView.UpdateView(_levelIndex);
                }
            }
        }
    }
}