using System.Collections.Generic;
using Core.Anchors;
using Core.Windows;
using GameStats;
using Tutorial;
using Tutorial.View;
using UnityEngine;
using Utils;
using Zenject;

namespace GameScripts.MergeSquares.Tutorial
{
    public class TutorialProvider : TutorialProviderBase<TutorialDesc, TutorialStep>
    {
        [SortingLayer]
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;
        [SerializeField] private Anchor settingsButton;
        [SerializeField] private List<TutorialDesc> tutorials = new();
        
        public override IReadOnlyList<TutorialDesc> TutorialDescriptions => tutorials;
        
        public int SortingOrder => sortingOrder;
        public string SortingLayerName => sortingLayerName;
        public Anchor SettingsButton => settingsButton;
        
        public GridManager GridManager => _gridManager;
        public GameStatService GameStatService => _gameStatService;
        public FingerController FingerController => _fingerController;
        public AnchorService AnchorService => _anchorService;
        public WindowManager WindowManager => _windowManager;

        private GridManager _gridManager;
        private FingerController _fingerController;
        private GameStatService _gameStatService;
        private AnchorService _anchorService;
        private WindowManager _windowManager;
        
        [Inject]
        public void Construct(GridManager gridManager, FingerController fingerController, GameStatService gameStatService, AnchorService anchorService, WindowManager windowManager)
        {
            _gridManager = gridManager;
            _fingerController = fingerController;
            _gameStatService = gameStatService;
            _anchorService = anchorService;
            _windowManager = windowManager;
        }

        protected override TutorialBase<TutorialDesc, TutorialStep> Create() => new SquaresTutorial(this);
    }
}