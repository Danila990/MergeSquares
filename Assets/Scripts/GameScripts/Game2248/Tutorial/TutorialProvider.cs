using System.Collections.Generic;
using Core.Anchors;
using Core.Windows;
using GameStats;
using Tutorial;
using Tutorial.View;
using UnityEngine;
using Utils;
using Zenject;

namespace GameScripts.Game2248.Tutorial
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
        
        public GameStatService GameStatService => _gameStatService;
        public AnchorService AnchorService => _anchorService;
        public GridManager GridManager => _gridManager;
        public WindowManager WindowManager => _windowManager;
        public FingerController FingerController => _fingerController;

        public TutorialGrid TutorialGrid { get; set; }

        private GameStatService _gameStatService;
        private AnchorService _anchorService;
        private GridManager _gridManager;
        private WindowManager _windowManager;
        private FingerController _fingerController;

        [Inject]
        public void Construct(GridManager gridManager, GameStatService gameStatService, AnchorService anchorService, WindowManager windowManager, FingerController fingerController)
        {
            _gridManager = gridManager;
            _gameStatService = gameStatService;
            _anchorService = anchorService;
            _windowManager = windowManager;
            _fingerController = fingerController;
        }

        protected override TutorialBase<TutorialDesc, TutorialStep> Create() => new Tutorial(this);
    }
}