using System;
using System.Collections;
using System.Collections.Generic;
using Advertising;
using Advertising.AnalyticsSignals;
using CloudServices;
using Core.Windows;
using GameScripts.PointPanel;
using GameStats;
using MergeBoard.UI;
using Shop.AnalyticsSignals;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using Zenject;

public class BallsCustomizeUi : MonoBehaviour
{
    [SerializeField] private Transform skinsRoot;
    [SerializeField] private BallsSkinCell skinCellPrefab;
    [SerializeField] private TextMeshProUGUI coinsCountText;
    [SerializeField] private TextMeshProUGUI advCoinsText;
    [SerializeField] private RewardAdButton rewardAdButton;
    [SerializeField] private Button unlockButton;
    [SerializeField] private int advCoinsCount = 50;
    [SerializeField] private List<ParticleSystem> unlockFxs = new();

    private List<BallsSkinCell> cells = new List<BallsSkinCell>();

    private BallsSkinsManager _skinsManager;
    private GameStatService _gameStatService;
    private PointPanel _pointPanel;
    private WindowManager _windowManager;
    private SignalBus _signalBus;
    private CloudService _cloudService;

    [Inject]
    private void Construct(
        BallsSkinsManager skinsManager,
        GameStatService gameStatService,
        PointPanel pointPanel,
        WindowManager windowManager,
        SignalBus signalBus,
        CloudService cloudService
    )
    {
        _skinsManager = skinsManager;
        _gameStatService = gameStatService;
        _pointPanel = pointPanel;
        _windowManager = windowManager;
        _signalBus = signalBus;
        _cloudService = cloudService;
        ShowSkins();

        _pointPanel.OnSetNewSkin += UpdateUi;
    }

    public void OnClick()
    {
        if ((_pointPanel.OpenedSkinsCount >= _skinsManager.Skins.Count || (!_gameStatService.TryDec(EGameStatType.Soft, _skinsManager.Skins[_pointPanel.OpenedSkinsCount].openCost/*_pointPanel.SkinCost*/))))
        {
            return;
        }

        var skin = _skinsManager.Skins[_pointPanel.OpenedSkinsCount];
        _signalBus.Fire(new ShopPurchaseSignal(skin.openCost, $"Skin: {skin}"));
        _cloudService.CloudProvider.HappyTime();
        foreach (var fx in unlockFxs)
        {
            fx.Play();
        }
        UnlockSkin();
        UpdateUi();
    }

    private void UpdateUi()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (_pointPanel.OpenedSkinsCount > i)
            {
                cells[i].Unlock();
                cells[i].ClearFrame();
            }
            else
            {
                if (_pointPanel.OpenedSkinsCount == i)
                {
                    cells[i].SetFrameNext();
                }
                cells[i].Lock();
            }
            
            if (cells[i].Skin.Equals(_pointPanel.CurrentBallSkin))
            {
                cells[i].SetFrameUsing();
            }
        }

        SetUnlockButton();
    }

    private void SetUnlockButton()
    {
        if (_pointPanel.OpenedSkinsCount >= _skinsManager.Skins.Count)
        {
            unlockButton.gameObject.SetActive(false);
        }
        else
        {
            unlockButton.gameObject.SetActive(true);
            var cost = _skinsManager.Skins[_pointPanel.OpenedSkinsCount].openCost;
            coinsCountText.text = cost.ToString();
            if (cost > _gameStatService.GetStatValue(EGameStatType.Soft))
            {
                unlockButton.interactable = false;
            }
            else
            {
                unlockButton.interactable = true;
            }
        }
    }

    private void UnlockSkin()
    {
        cells[_pointPanel.OpenedSkinsCount].ClearFrame();
        _pointPanel.OpenNextSkin();
        if (_pointPanel.OpenedSkinsCount < _skinsManager.Skins.Count)
        {
            cells[_pointPanel.OpenedSkinsCount].SetFrameNext();
        }
    }

    private void ShowSkins()
    {
        for (int i = 0; i < _skinsManager.Skins.Count; i++)
        {
            var skinCell = Instantiate(skinCellPrefab, skinsRoot);
            skinCell.Show(_skinsManager.Skins[i], i);
            if (_skinsManager.Skins[i].Equals(_pointPanel.CurrentBallSkin))
            {
                skinCell.SetFrameUsing();
            }
            var openedSkinsCount = _pointPanel.OpenedSkinsCount;
            if (openedSkinsCount > i)
            {
                skinCell.Unlock();
            }
            else
            {
                if (openedSkinsCount == i)
                {
                    skinCell.SetFrameNext();
                }
                skinCell.Lock();
            }
            cells.Add(skinCell);
        }

        SetUnlockButton();
        // coinsCountText.text = _pointPanel.SkinCost.ToString();
        advCoinsText.text = advCoinsCount.ToString();
    }

    public void BuySoftCurrencyWithAd()
    {
        if(rewardAdButton.CanShow())
        {
            rewardAdButton.GetComponent<Button>().interactable = false;
            rewardAdButton.Rewarded += OnRewarded;
            rewardAdButton.Failed += OnFailed;
            rewardAdButton.ShowAd();
        }
    }
    
    private void OnRewarded()
    {
        rewardAdButton.GetComponent<Button>().interactable = true;
        _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Completed, "Buy Soft currency"));

        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;

        CoinsAddPopup coinsAddPopup;
        if (_windowManager.TryShowAndGetWindow(EPopupType.CoinsAdd.ToString(), out coinsAddPopup))
        {
            _gameStatService.TryIncWithAnim(EGameStatType.Soft, advCoinsCount);
            coinsAddPopup.SetArgs(advCoinsCount, () => {});
        }
        SetUnlockButton();
    }

    private void OnFailed()
    {
        rewardAdButton.GetComponent<Button>().interactable = true;
        _signalBus.Fire(new AdSignal(AdType.Rewarded, AdStatus.Failed, "Buy Soft currency"));

        rewardAdButton.Rewarded -= OnRewarded;
        rewardAdButton.Failed -= OnFailed;
        SetUnlockButton();
    }

    private void OnDestroy()
    {
        _pointPanel.OnSetNewSkin -= UpdateUi;
    }
}
