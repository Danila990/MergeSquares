using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Windows;
using DG.Tweening;
using GameStats;
using Levels;
using Mono.CSharp;
using Rewards;
using Rewards.Models;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace LuckyWheel
{
// [Serializable]
// public class WheelSectorData
// {
//     public bool isNegative;
//     public bool isAdv;
//     public int cost;
// }
    [Serializable]
    public class WheelParams
    {
        public string id;
        public int fixedSector = -1;
        public List<WheelReward> rewards = new List<WheelReward>();
    }

    [Serializable]
    public class WheelReward
    {
        [NonSerialized] public RewardResourceModel rewardResource;
        public RewardModel rewardModel;
        public int weightForRandom;
    }

    public class LuckyWheelPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private Selector selector;
        [SerializeField] private List<Sector> sectors;

        [SerializeField] private List<Button> exitButtons;

        // [SerializeField] private TextMeshProUGUI repDiaplay;
        [SerializeField] private Button rollButton;

        // [SerializeField] private Animator lightAnimator;
        [SerializeField] private RewardRepository _rewardRepository;

        public Action endRoll;
        public Action closed;
        
        // private List<RewardModel> rewards;
        private WheelParams _wheelParams;
        private int _maxCost = 0;
        // private WheelSectorData _savedData;
        // private int _sectorsCount = 10;
        // private string _rollButtonText;
        private int _fixedSector = -1;
        
        private WheelService _wheelService;

        [Inject]
        private void Construct(WheelService wheelService)
        {
            _wheelService = wheelService;
            popupBase.ShowArgsGot += OnShowArgsGot;
            // popupBase.Disposed += Dispose;

            // variableManager = Engine.GetService<ICustomVariableManager>();
        }

        private void OnDestroy()
        {
            popupBase.ShowArgsGot -= OnShowArgsGot;
            // popupBase.Disposed -= Dispose;

            // variableManager.SetVariableValue("wheelTrigger", "");
            closed?.Invoke();
        }

        // public void SetWheel(WheelRollData data)
        // {
        //     rewards = data.rewards;
        //     fixedSector = data.fixedSector;
        //     sectorsCount = sectors.Count;
        //
        //     var datas = data.sectors;
        //     for (int i = 0; i < math.min(sectorsCount, datas.Count); i++)
        //     {
        //         var newData = new WheelSectorData();
        //         sectors[i].SetSector(datas[i]);
        //         sectors[i].OnTriggered += OnSectorTriggered;
        //         
        //         if (_maxCost < datas[i].cost)
        //         {
        //             _maxCost = datas[i].cost;
        //         }
        //     }
        //     UpdateState();
        // }

        public void OnClickRoll()
        {
            if (_fixedSector >= 0 && _fixedSector < sectors.Count)
            {
                selector.Roll(_fixedSector);
            }
            else
            {
                if (_fixedSector >= sectors.Count)
                {
                    Debug.LogError(
                        $"[LuckyWheelPopup][OnClickRoll] Error: fixed sector is too big: {_fixedSector}, sectors count: {sectors.Count}. Used random value");
                }

                sectors.TryWeightRandom(s => s.Weight, out var sector);
                selector.Roll(sectors.IndexOf(sector));
                // result = UnityEngine.Random.Range(0, sectors.Count);
            }

            rollButton.interactable = false;
            // lightAnimator.SetBool("isLight", true);
            SetExitButtons(false);
        }

        // private void UpdateState()
        // {
        //     // var rep = _gameStatService.Get(EGameStatType.Soft);
        //     
        //     // repDiaplay.text = $"{rep}";
        //     rollButton.interactable = rep >= _maxCost;
        // }

        private void OnShowArgsGot(object[] args)
        {
            if (args.Length > 0)
            {
                _wheelParams = args.First() as WheelParams;
                _fixedSector = _wheelParams.fixedSector;
                for (int i = 0; i < Math.Min(sectors.Count, _wheelParams.rewards.Count); i++)
                {
                    var param = _wheelParams.rewards[i];
                    param.rewardResource = _rewardRepository.GetById(param.rewardModel.id);
                    if (param.rewardModel.id == "Unit" && !_wheelService.CheckSkin(param))
                    {
                        param.rewardModel.id = "Soft";
                        param.rewardResource = _rewardRepository.GetById(param.rewardModel.id);
                    }
                    sectors[i].SetSector(param);
                    sectors[i].OnTriggered += OnSectorTriggered;
                }
                // levelNum.text = winPopupParams.showedLevel.ToString();
                // ShowAddedCoins();
                // if(giftBonus != null)
                // {
                // giftBonus.text = "+" + winPopupParams.giftBonus;
                // }

                // DOTween.To(() => winPopupParams.startGiftProgress, newAmount =>
                // {
                // giftIcon.fillAmount = newAmount;
                // }, winPopupParams.giftProgress, 1f);

                // if (winPopupParams.giftProgress >= 1f)
                // {
                //     giftAnimator.SetTrigger("start");
                //     // giftButton.gameObject.SetActive(true);
                // }
                // else
                // {
                //     giftAnimator.StopPlayback();
                //     // giftButton.gameObject.SetActive(false);
                // }
            }
        }

        // private void OnCloseOffer(bool isAdsWatched, OfferWheelRespinPopup offer)
        // {
        //     offer.OnClose -= OnCloseOffer;
        //     if (isAdsWatched)
        //     {
        //         OnClickRoll();
        //     }
        //     else
        //     {
        //         EndRoll(/*_savedData*/);
        //         UpdateState();
        //     }
        // }

        private void OnSectorTriggered( /*WheelSectorData data*/ Sector sector)
        {
            _wheelService.GetReward(sector);
            // 
            EndRoll();
            // lightAnimator.SetBool("isLight", false);
            // if (data.isAdv)
            // {
            //     var id = EPopupType.OfferLuckyWheel.ToString();
            //     var window = _windowManager.GetWindow(id);
            //     if (window == null)
            //     {
            //         var newOffer = ((PopupBase)_windowManager.ShowWindow(id)).GetComponent<OfferWheelRespinPopup>();
            //         newOffer.OnClose += OnCloseOffer;
            //     }
            //     _savedData = data;
            // }
            // else
            // {
            // EndRoll(/*data*/);
            // }
        }

        private void EndRoll( /*WheelSectorData data*/)
        {
            // _gameStatService.TryDec(EGameStatType.Soft, data.cost);

            // UpdateState();
            SetExitButtons(true);
            // if (!data.isNegative)
            // {
            // foreach (var reward in rewards)
            // {
            //     _rewardService.Award(reward);
            // }
            // }
            endRoll?.Invoke( /*data.isNegative*/);
            // popupBase.CloseWindow();
        }

        private void SetExitButtons(bool isInteractable)
        {
            foreach (var btn in exitButtons)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = isInteractable;
            }
        }
    }
}
