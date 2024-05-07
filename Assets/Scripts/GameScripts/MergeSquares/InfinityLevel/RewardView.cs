using System;
using Core.Repositories;
using GameStats;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class RewardView : MonoBehaviour
    {
        [SerializeField] private ERewardViewType type;
        [SerializeField] private EGameStatType gameStatType;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image skinBackground;
        [SerializeField] private Image skinForeground;
        [SerializeField] private Image statIcon;
        [SerializeField] private ResourceRepository resourceRepository;

        public ERewardViewType Type => type;
        public EGameStatType GameStatType => gameStatType;

        public void Init(EGameStatType type, int amount)
        {
            // amountText.text = $"{amount}";
            Init(type, $"{amount}");
        }
        
        public void Init(EGameStatType type, string amount)
        {
            var sprite = resourceRepository.GetImageById(type.ToString());
            statIcon.sprite = sprite;
            amountText.text = amount;
        }

        public void Init(int amount, Color rarityColor, string rarityName)
        {
            amountText.text = $"{amount}";
            nameText.text = rarityName;
            skinBackground.color = rarityColor;
            skinForeground.color = rarityColor;
        }
        
        public void Init(string amount, Color rarityColor, string rarityName)
        {
            amountText.text = amount;
            nameText.text = rarityName;
            skinBackground.color = rarityColor;
            skinForeground.color = rarityColor;
        }
    }
}