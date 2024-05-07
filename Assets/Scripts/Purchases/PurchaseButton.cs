using System;
using CloudServices;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Purchases
{
    public class PurchaseButton : MonoBehaviour
    {
        [SerializeField] private PurchaseSo purchase;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI amount;

        public Action Inited = () => { };

        private bool _available;
        
        private PurchaseService _purchaseService;
        private SettingsService _settingsService;
        private CloudService _cloudService;

        [Inject]
        public void Construct(PurchaseService purchaseService, SettingsService settingsService, CloudService cloudService)
        {
            _purchaseService = purchaseService;
            _settingsService = settingsService;
            _cloudService = cloudService;
            _purchaseService.PurchaseChanged += OnPurchaseChanged;
        }

        private void OnDestroy()
        {
            _purchaseService.PurchaseChanged -= OnPurchaseChanged;
        }
        
        public void OnClick()
        {
            if(_available)
            {
                _purchaseService.StartPurchase(purchase);
            }
        }

        private void Start()
        {
            Init();
        }

        private void OnPurchaseChanged()
        {
            Init();
        }

        private void Init()
        {
            if (_purchaseService.TryGetCost(purchase, out var value, out var currency) && _purchaseService.IsPurchasesAvailable)
            {
                if (cost != null)
                {
                    cost.text = $"{value} {currency}";
                }

                if (amount != null)
                {
                    amount.text = $"+{purchase.value}";
                }

                if (icon != null)
                {
                    icon.sprite = purchase.icon; 
                }
                _available = true;
                if (!purchase.repeatable)
                {
                    if (amount != null)
                    {
                        amount.gameObject.SetActive(false);
                    }

                    if (_purchaseService.TryCheckAlreadyBought(purchase, out var bought))
                    {
                        if(bought)
                        {
                            _available = false;
                            gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                _available = false;
                gameObject.SetActive(false);
            }
            Inited.Invoke();
        }
    }
}