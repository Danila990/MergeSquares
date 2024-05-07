using System;
using GameStats;
using UnityEngine;
using Utils.Attributes;
using System.Collections.Generic;
using System.Collections;

namespace Purchases
{
    [Serializable]
    public enum EPurchaseType
    {
        DisableAds = 0,
        AddCurrency = 1,
        Pack = 2,
        GameType = 3
    }
    [Serializable]
    public class PurchaseSoPackedItems : IEnumerable<PurchaseSo>
    {
        public List<PurchaseSo> purchaseItems;

        public IEnumerator<PurchaseSo> GetEnumerator()
        {
            return purchaseItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return purchaseItems.GetEnumerator();
        }
    }
    [CreateAssetMenu(fileName = "Purchase", menuName = "Purchases/Purchase")]
    public class PurchaseSo : ScriptableObject
    {
        public string id;
        public bool repeatable;
        public Sprite icon;
        public EPurchaseType type;
        [Space(height: 20f)]
        [EnumConditionalHide(nameof(type), EPurchaseType.AddCurrency, true)]
        public EGameStatType statType;
        [EnumConditionalHide(nameof(type), EPurchaseType.AddCurrency, true)]
        public int value;
        [EnumConditionalHide(nameof(type), EPurchaseType.Pack, true)]
        public PurchaseSoPackedItems packedItems;

        [EnumConditionalHide(nameof(type), EPurchaseType.GameType, true)]
        public PurchaseExtSoBase purchaseExt;
    }
}