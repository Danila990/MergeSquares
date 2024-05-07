using System;
using System.Collections.Generic;
using System.Linq;
using Core.Localization;
using UnityEngine;

namespace GameScripts.MergeSquares.Shop
{
    [Serializable]
    public enum ESkinRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5,
    }

    [Serializable]
    public class SkinRarity
    {
        public ESkinRarity rarity;
        public Color color;
        public List<float> chanceForLevel;
        public int cashBack;
        public float multiplier = 1.5f;
    }
    
    [Serializable]
    public class SquaresSkin<T> where T : Enum
    {
        [SerializeField] private T _skin;
        [SerializeField] private int _openCost;
        [SerializeField] private ESkinRarity rarity;

        public T Skin => _skin;
        public int OpenCost => _openCost;

        public ESkinRarity Rarity => rarity;
    }

    [Serializable]
    public class SquaresSkin : SquaresSkin<ESquareSkin>{};
    
    public class SquaresSkinsManagerBase<T1, T2> : ScriptableObject where T1 : SquaresSkin<T2> where T2 : Enum
    {
        [SerializeField] private List<T1> skins = new();
        [SerializeField] private List<SkinRarity> rarityData = new();
        [SerializeField] private LocalizationRepository localizationRepository;

        public IReadOnlyList<T1> Skins => skins;
        public IReadOnlyList<SkinRarity> RarityData => rarityData;

        public T1 GetElementByEnum(T2 skin)
        {
            return skins.FirstOrDefault(t => t.Skin.Equals(skin));
        }

        public SkinRarity GetRarity(T1 skin)
        {
            foreach (var rarityColor in rarityData)
            {
                if (rarityColor.rarity.Equals(skin.Rarity))
                {
                    return rarityColor;
                }
            }

            return new SkinRarity
            {
                color = Color.white,
                rarity = skin.Rarity
            };
        }
        
        public string GetRarityText(ESkinRarity rarity)
        {
            return localizationRepository.GetTextInCurrentLocale($"{rarity.ToString()}Name").Substring(0, 1);
        }
        
        public SkinRarity GetRarity(ESkinRarity rarity)
        {
            foreach (var skinRarity in rarityData)
            {
                if (skinRarity.rarity.Equals(rarity))
                {
                    return skinRarity;
                }
            }

            return new SkinRarity
            {
                color = Color.white,
                rarity = rarity
            };
        }
    }

    [CreateAssetMenu(fileName = "SquaresSkinsManager", menuName = "Squares/SquaresSkinsManager")]
    public class SquaresSkinsManager : SquaresSkinsManagerBase<SquaresSkin, ESquareSkin> {}
}