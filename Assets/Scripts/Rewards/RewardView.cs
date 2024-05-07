using System;
using Mono.CSharp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Rewards
{
    public class RewardViewParams
    {
        public Sprite sprite;
        public string text;
        public float size;
    }

    public class RewardView : MonoBehaviour, IHideable
    {
        [SerializeField] private Image image;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private GameObject textParent;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Animator animator;
        [SerializeField] private string hideKey;

        public void Init(RewardViewParams rewardViewParams)
        {
            image.sprite = rewardViewParams.sprite;
            textParent.SetActive(!String.IsNullOrEmpty(rewardViewParams.text));
            rectTransform.sizeDelta = new Vector2(rewardViewParams.size, rewardViewParams.size);
            text.text = rewardViewParams.text;
        }

        public void Hide()
        {
            if (animator != null)
            {
                animator.SetTrigger(hideKey);
            }
        }
    }
}