using System;
using System.Collections;
using System.Collections.Generic;
using GameScripts.Game2248.Shop;
using GameStats;
using LuckyWheel;
using Rewards;
using Rewards.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace LuckyWheel
{
    public class Sector : MonoBehaviour
    {
        [SerializeField] private Image rewardIcon;
        [SerializeField] private SkinView skinView;
        [SerializeField] private TextMeshProUGUI rewardCount;

        public Action /*<WheelSectorData>*/<Sector> OnTriggered;

        public WheelReward WheelReward => _wheelReward;
        public int Weight { get; private set; }

        // private WheelSectorData _sectorData;
        private WheelReward _wheelReward;
        private WheelService _wheelService;
        
        [Inject]
        private void Construct(WheelService wheelService)
        {
            _wheelService = wheelService;
            // popupBase.Disposed += Dispose;

            // variableManager = Engine.GetService<ICustomVariableManager>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            OnTriggered?.Invoke( /*_sectorData*/this);
        }

        public void SetSector( /*WheelSectorData data*/ WheelReward wheelReward)
        {
            _wheelReward = wheelReward;
            Weight = wheelReward.weightForRandom;
            switch (wheelReward.rewardModel.id)
            {
                case "Unit":
                    rewardIcon.gameObject.SetActive(false);
                    skinView.gameObject.SetActive(true);
                    rewardCount.gameObject.SetActive(false);
                    var id = _wheelService.GetSkinId(_wheelReward);
                    var skinChance = _wheelService.GetSummonSkinChance(id);
                    var text = _wheelService.GetRarityText(skinChance.rarity.rarity);
                    skinView.Init(id, skinChance.rarity.color, text, "", true);
                    break;
                default:
                    rewardIcon.gameObject.SetActive(true);
                    skinView.gameObject.SetActive(false);
                    rewardCount.gameObject.SetActive(true);
                    rewardIcon.sprite = wheelReward.rewardResource.sprite;
                    rewardCount.text = $"x{_wheelReward.rewardModel.value}";
                    break;
            }
            // _sectorData = data;
            // reputationCostView.text = $"-{_sectorData.cost}";
            // if (_sectorData.isNegative)
            // {
            //     sectorImage.sprite = negativeSprite;
            // }
        }
    }
}
