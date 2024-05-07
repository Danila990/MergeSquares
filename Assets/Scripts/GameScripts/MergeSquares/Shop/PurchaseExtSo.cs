using System;
using Purchases;
using UnityEngine;

namespace GameScripts.MergeSquares.Shop
{
    [Serializable]
    public enum EPurchaseType
    {
        Skin = 0
    }
    
    [CreateAssetMenu(fileName = "PurchaseExtSo", menuName = "Squares/PurchaseExt")]
    public class PurchaseExtSo : PurchaseExtSoBase
    {
        [SerializeField] private EPurchaseType type;
        [SerializeField] private ESquareSkin skinType;

        public EPurchaseType Type => type;
        public ESquareSkin SkinType => skinType;
    }
}