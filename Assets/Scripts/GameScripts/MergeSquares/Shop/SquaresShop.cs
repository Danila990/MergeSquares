using System;
using System.Collections.Generic;
using Core.Windows;
using GameStats;
using GameTime;
using Notify;
using Offers;
using Offers.Model;
using Purchases;
using TMPro;
using Tutorial;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares.Shop
{
    public class SquaresShop : MonoBehaviour
    {
        [SerializeField] private GameObject offersCategory;
        [SerializeField] private List<OfferModel> modelsToCheck = new();
        [SerializeField] private GameObject inAppsCategory;
        [SerializeField] private List<PurchaseButton> buttons = new();
        [SerializeField] private ShopScrollRectMarkers scrollRectMarkers;
        [SerializeField] private TextMeshProUGUI timeLeftForFree;
        [SerializeField] private Button freeRewardButton;
        [SerializeField] private GameObject freeText;
        [SerializeField] private string targetTimeId = "shopAdsTime";
        [SerializeField] private int reward = 30;

        private EShopMarkers _target = EShopMarkers.InApps;
        private bool _firstMoveToTarget;
        private SquaresSkinsCategoryBase skinsCategory;

        private TutorialService _tutorialService;
        private TimeService _timeService;
        private WindowManager _windowManager;
        private GameStatService _gameStatService;
        private OfferService _offerService;
        private NotifyService _notifyService;

        [Inject]
        public void Construct(
            TutorialService tutorialService,
            TimeService timeService,
            WindowManager windowManager,
            GameStatService gameStatService,
            OfferService offerService,
            NotifyService notifyService
        )
        {
            _tutorialService = tutorialService;
            _timeService = timeService;
            _windowManager = windowManager;
            _gameStatService = gameStatService;
            _offerService = offerService;
            _offerService.OfferDestroyed += OnOfferDestroyed;
            _notifyService = notifyService;
            foreach (var purchaseButton in buttons)
            {
                purchaseButton.Inited += OnButtonInited;
            }
        }

        private void Start()
        {
            if (_tutorialService != null && _tutorialService.HasActiveTutorial)
            {
                _target = EShopMarkers.Units;
            }

            skinsCategory = GetComponentInChildren<SquaresSkinsCategoryBase>();

            CheckOfferCategory();
        }

        private void OnDestroy()
        {
            _offerService.OfferDestroyed -= OnOfferDestroyed;
            foreach (var purchaseButton in buttons)
            {
                purchaseButton.Inited -= OnButtonInited;
            }
        }

        private void Update()
        {
            if (!_firstMoveToTarget)
            {
                _firstMoveToTarget = true;
                scrollRectMarkers.ScrollToMarker(_target);
            }

            SetTimer();
        }

        private void SetTimer()
        {
            DateTime targetTime = DateTime.Now;
            
            if (!_timeService.TryGetTimeTarget(targetTimeId, ref targetTime))
            {
                freeRewardButton.interactable = true;
                timeLeftForFree.GameObject().SetActive(false);
                freeText.SetActive(true);
                _notifyService.SetNotify(new NotifyRef{id = targetTimeId}, true);
                return;
            };
                
            TimeSpan countdown = targetTime - DateTime.Now;
            if (countdown.Ticks > 0)
            {
                countdown = targetTime - DateTime.Now;
                freeRewardButton.interactable = false;
                timeLeftForFree.GameObject().SetActive(true);
                freeText.SetActive(false);
                timeLeftForFree.text = $"{countdown.Minutes:D2} : {countdown.Seconds:D2}";
                _notifyService.SetNotify(new NotifyRef{id = targetTimeId}, false);
                return;
            }
            freeRewardButton.interactable = true;
            timeLeftForFree.GameObject().SetActive(false);
            freeText.SetActive(true);
            _notifyService.SetNotify(new NotifyRef{id = targetTimeId}, true);
        }

        public void UpdateSkinsCells() => skinsCategory.UpdateUi();
        public void GetFreeReward()
        {
            if (_windowManager.TryShowAndGetWindow<CoinsAddPopup>(EPopupType.CoinsAdd.ToString(), out var coinsAddPopup))
            {
                coinsAddPopup.SetArgs(reward, () =>{});
                _gameStatService.TryIncWithAnim(EGameStatType.Soft, reward);
                SetNextFreeReward();
            }
        }
        
        public void SetNextFreeReward()
        {
            _timeService.SetTimeTarget(targetTimeId, DateTime.Now.AddMinutes(30));
        }

        public static void OpenSection(WindowManager windowManager, EShopMarkers marker)
        {
            var shop = ((PopupBase) windowManager.EnsureOpen(EPopupType.Shop.ToString())).GetComponent<SquaresShop>();
            if(shop != null)
            {
                shop.OpenSection(marker);
            }
        }

        public void OpenSection(EShopMarkers marker)
        {
            _firstMoveToTarget = false;
            if (marker == EShopMarkers.InApps)
            {
                marker = EShopMarkers.Offers;
            }
            _target = marker;
        }
        
        private void OnButtonInited()
        {
            foreach(var b in buttons){
                if (b.gameObject.activeInHierarchy)
                {
                    return;
                }
            }
            inAppsCategory.SetActive(false);
        }
        
        private void OnOfferDestroyed(OfferData offerData)
        {
            CheckOfferCategory();
            UpdateSkinsCells();
        }

        private void CheckOfferCategory()
        {
            if (offersCategory != null && modelsToCheck.Count > 0)
            {
                offersCategory.SetActive(false);
                foreach (var offerModel in modelsToCheck)
                {
                    var offers = _offerService.GetActiveOffersByModel(offerModel);
                    if (offers.Count > 0)
                    {
                        offersCategory.SetActive(true);
                        break;
                    }
                }
            }
        }
    }
}
