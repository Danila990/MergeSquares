using Rewards.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rewards
{
    public class RangeRewardView : MonoBehaviour
    {
        [SerializeField] private Sprite unitIcon;
        [SerializeField] private Image image;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private GameObject textParent;
        [SerializeField] private TextMeshProUGUI text;
        
        public void Init(RangeUnitRewardModel model, float size)
        {
            image.sprite = unitIcon;
            rectTransform.sizeDelta = new Vector2(size, size);
            text.text = $"0 - {model.count}";
        }
        
        public void Init(RangeStatRewardModel model, RewardResourceModel resourceModel, float size)
        {
            image.sprite = resourceModel.sprite;
            rectTransform.sizeDelta = new Vector2(size, size);
            text.text = $"{model.baseReward.value} - {model.valueMax}";
        }
    }
}