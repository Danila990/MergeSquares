using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Windows;
using GameScripts.MergeSquares;
using GameScripts.MergeSquares.Shop;
using Shop;
using UnityEngine;
using Zenject;

namespace Shop
{
    public class RarityCount : RarityViewBase
    {
        [SerializeField] private SquaresSkinsManager skinsRepo;
        [SerializeField] private bool initOnStart = true;

        public ESkinRarity Rarity => rarity;
        
        private GridManager _gridManager;

        [Inject]
        private void Construct(GridManager gridManager)
        {
            _gridManager = gridManager;
        }
        
        private void Start()
        {

            var rarityData = skinsRepo.GetRarity(rarity);
            rarityName.SetLocalizationKey($"{rarity.ToString()}Name");
            icon.color = rarityData.color;
            if(!initOnStart)
                return;
            var count = 0;
            foreach (var skin in _gridManager.OpenedSkins)
            {
                var skinData = skinsRepo.Skins.ToList().Find(s => s.Skin == skin.skinType);
                if (skinData.Rarity == rarity)
                {
                    count += skin.count;
                }
            }
            
            rarityChance.text = $"{count}";
        }

        public void SetCount(string text) => rarityChance.text = text;
    }
}