using System;
using System.Collections;
using System.Collections.Generic;
using Advertising;
using Advertising.AnalyticsSignals;
using Core.Windows;
using GameScripts.Game2248;
using GameScripts.MergeSquares.Shop;
using GameStats;
using LargeNumbers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class X2Offer : MonoBehaviour
{
    [SerializeField] private PopupBase popupBase;
    [SerializeField] private UnitView unitStart;
    [SerializeField] private UnitView unitGet;
    [SerializeField] private AdsBar adsBar;

    private Action OnClose = () => { };

    private Cell cell;

    private WindowManager _windowManager;
    private GameStatService _gameStatService;
    private GridManager _gridManager;
    private Button x2AdsButton;

    [Inject]
    private void Construct(GameStatService gameStatService, SignalBus signalBus, WindowManager windowManager, GridManager gridManager)
    {
        _gameStatService = gameStatService;
        _windowManager = windowManager;
        _gridManager = gridManager;

        popupBase.Inited += OnInited;
    }
    
    private void OnInited()
    {
        // popupBase.Canvas.sortingLayerName = sortingLayerName;
    }

    private void OnDestroy()
    {
        OnClose.Invoke();
        popupBase.Inited -= OnInited;
    }
    
    public void Init(Cell cell, Action callback)
    {
        adsBar.Init(2, GetX2Ads);
        this.cell = cell;
        unitStart.Init(cell.view.Value);
        unitGet.Init(cell.view.Value * 2);
        OnClose = callback;
    }

    private void GetX2Ads()
    {
        _gridManager.CurrentGridView.SetX2Square(cell);
        popupBase.CloseWindow();
    }
}
