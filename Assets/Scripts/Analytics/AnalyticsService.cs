using Core.Signals;
using Core.Windows.AnalyticsSignals;
using Installers;
using Levels.AnalyticsSignals;
using Shop.AnalyticsSignals;
using System;
using System.Collections.Generic;
using System.Reflection;
using Tutorial.AnalyticsSignals;
using Unity.Services.Analytics;
using UnityEngine;
using Zenject;
using Advertising.AnalyticsSignals;
using CloudServices;
using Core.SaveLoad;
using GameScripts.MergeSquares.AnalyticsSignals;
using GameScripts.PointPanel.AnalyticsSignals;
using GameScripts.AnalyticsSignals;
using Popups.AnalyticsSignals;
using Purchases.AnalyticsSignals;

namespace Analytics
{
    public enum GameType
    {
        Balls,
        Squares
    }

    public class AnalyticsService : MonoBehaviour
    {
        [SerializeField] private GameType gameType;

        private Dictionary<Action<object>, Type> _signalActions = new();

        private SignalBus _signalBus;
        private DeclaredSignalsContainer _declaredSignalsContainer;
        private CloudService _cloudService;
        private SaveService _saveService;

        [Inject]
        public void Construct(SignalBus signalBus, DeclaredSignalsContainer declaredSignalsContainer, CloudService cloudService, SaveService saveService)
        {
            _signalBus = signalBus;
            _declaredSignalsContainer = declaredSignalsContainer;
            _cloudService = cloudService;
            _saveService = saveService;
        }

        private void Start()
        {
            SubscribeAnalyticsSignals(_declaredSignalsContainer);
        }

        private void OnDestroy()
        {
            UnsubscribeAnalyticsSignals();
        }

        private void SubscribeAnalyticsSignals(DeclaredSignalsContainer declaredSignalsContainer)
        {
            //Get type of AnalyticsSignals
            var analyticsSignalType = typeof(AnalyticsSignal);

            //Enumerating all declared signals
            foreach (var declaredSignalType in declaredSignalsContainer.Types)
            {
                //Skip a signal if it does not inherit from AnalyticsSignal
                if (!analyticsSignalType.IsAssignableFrom(declaredSignalType))
                {
                    continue;
                }

                //Get type of IUnityAnalyticsSignal
                var signalType = typeof(IYandexAnalyticsSignal);
                var gameSignalType = gameType switch
                {
                    GameType.Balls => typeof(IBallsSignal),
                    GameType.Squares => typeof(ISquaresSignal),
                    _ => throw new ArgumentException("Can't find game signal type for:", gameType.ToString())
                };

                //Checking if the signal is inherited from the IUnityAnalyticsSignal interface
                if (signalType.IsAssignableFrom(declaredSignalType))
                {
                    //Search function by name and signal type
                    if (TryGetAnalyticsSignalAction("OnYandexAnalyticsSignal", declaredSignalType, out var unityAnalyticsAction))
                    {
                        //If the function is found then subscribe to it and add to the list
                        _signalBus.Subscribe(declaredSignalType, unityAnalyticsAction);
                        _signalActions.Add(unityAnalyticsAction, declaredSignalType);
                    }
                }
            }
        }

        private bool TryGetAnalyticsSignalAction(string name, Type signalType, out Action<object> signalAction)
        {
            signalAction = null;

            //Get the class type in which the function will be searched
            var analyticsServiceType = GetType();

            //The function must be private and not static
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            //Getting a method by name and type with the specified parameters in the analyticsServiceType class
            var method = analyticsServiceType.GetMethod(name, bindingFlags, null, new Type[] { signalType }, null);
            if (method == null)
            {
                //If the function is not found then create an error in the log
                Debug.LogError($"[AnalyticsService][TryGetAnalyticsSignalDelegate] Function ({name}) to handle analytics signal {signalType.Name} not found");
                return false;
            }

            //Creating an Action<object> to further sign the SignalBus
            //THIS - the instance where the function will be executed.
            //new [] { signal } - array with method parameters
            signalAction = signal => { method.Invoke(this, new[] { signal }); };
            return true;
        }

        private void UnsubscribeAnalyticsSignals()
        {
            foreach (var signalAction in _signalActions)
            {
                _signalBus.Unsubscribe(signalAction.Value, signalAction.Key);
            }
        }

        #region UnityAnalyticsSignals

        private Dictionary<string, object> CreateEventData()
        {
            return new Dictionary<string, object>
            {
                { "SaveId", _saveService.SaveId }
            };
        }
        private void OnYandexAnalyticsSignal(BallsAnalyticsSignalBase signal)
        {
            
        }

        private void OnYandexAnalyticsSignal(SquaresAnalyticsSignalBase signal)
        {
            
        }

        private void OnYandexAnalyticsSignal(TutorialStepSignal signal)
        {
            var eventName = "TutorialStep";
            var eventParams = CreateEventData();
            eventParams.Add("TutorialStep", $"{signal.Id}_{signal.Index}");
            // eventParams.Add("TutorialName", $"{signal.Id}");

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(PopupOpenedSignal signal)
        {
            var eventName = "PopupOpened";
            var eventParams = CreateEventData();
            eventParams.Add("PopupId", $"{signal.PopupName}");

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(PopupClosedSignal signal)
        {
            var eventName = "PopupClosed";
            var eventParams = CreateEventData();
            eventParams.Add("PopupId", $"{signal.PopupName}");

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(PlayerLevelUpSignal signal)
        {
            // var eventName = "PlayerLevelUp";
            var eventParams = new Dictionary<string, object>
            {
                { "NewLevelIndex", signal.Level }
            };

            // _yandexService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(PlayerLevelUpOfferSignal signal)
        {
            // var eventName = "PlayerLevelUpOffer";
            var eventParams = new Dictionary<string, object>
            {
                { "LevelIndex", signal.Level }
            };

            // _yandexService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(ShopPurchaseSignal signal)
        {
            var eventName = "ShopSoftCurrencySpend";
            var eventParams = CreateEventData();
            eventParams.Add("ProductName", signal.ProductName);
            // eventParams.Add("ProductAmountSpent", signal.AmountSpent);

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(AdSignal signal)
        {
            var status = signal.Status switch
            {
                AdStatus.Completed => AdCompletionStatus.Completed,
                AdStatus.Failed => AdCompletionStatus.Incomplete,
                _ => throw new ArgumentException($"Adv status not exists: {nameof(signal.Status)}.")
            };

            var type = signal.Type switch
            {
                AdType.Rewarded => AdPlacementType.REWARDED,
                AdType.Interstitial => AdPlacementType.INTERSTITIAL,
                AdType.Banner => AdPlacementType.BANNER,
                _ => throw new ArgumentException($"Adv type not exists: {nameof(signal.Type)}.")
            };
            var eventName = "AdSignal";
            var eventParams = CreateEventData();
            eventParams.Add("AdInfo", $"{signal.PlacementName} {type.ToString()} {status.ToString()}");

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(BoughtNoAdsSignal signal)
        {
            var eventName = "BoughtNoAds";
            var eventParams = CreateEventData();
            eventParams.Add("NoAdsCost", signal.Cost);

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }
        
        private void OnYandexAnalyticsSignal(CurrencyBoughtSignal signal)
        {
            var eventName = "CurrencyBought";
            var eventParams = CreateEventData();
            eventParams.Add("CurrencyBought", $"{signal.Amount} {signal.StatType.ToString()} for {signal.Cost}");

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        private void OnYandexAnalyticsSignal(LevelStatusSignal signal)
        {
            var eventName = "LevelStatus";
            var eventParams = CreateEventData();
            eventParams.Add("LevelStatus", $"{signal.Id}_{signal.Status}");

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }
        
        private void OnYandexAnalyticsSignal(RateSignal signal)
        {
            var eventName = "Rate";
            var eventParams = CreateEventData();
            eventParams.Add("WasRated", $"{signal.WasRated}");

            _cloudService.CloudProvider.SendAnalyticEvent(eventName, eventParams);
        }

        #endregion
    }
}
