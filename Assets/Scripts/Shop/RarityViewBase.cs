using System.Collections;
using System.Collections.Generic;
using Core.Localization;
using GameScripts.MergeSquares.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    public class RarityViewBase : MonoBehaviour
    {
        [SerializeField] protected ESkinRarity rarity;
        [SerializeField] protected Image icon;
        [SerializeField] protected LocalizeUi rarityName;
        [SerializeField] protected TextMeshProUGUI rarityChance;
    }
}