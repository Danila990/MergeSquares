using System.Collections.Generic;
using Core.Anchors;
using Core.Windows;
using GameStats;
using Tutorial;
using UnityEngine;
using Utils;
using Zenject;

namespace GameScripts.PointPanel.Tutorial
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
        public PointPanel PointPanel => _pointPanel;
        public WindowManager WindowManager => _windowManager;

        private GameStatService _gameStatService;
        private AnchorService _anchorService;
        private PointPanel _pointPanel;
        private WindowManager _windowManager;
        
        [Inject]
        public void Construct(GameStatService gameStatService, AnchorService anchorService, PointPanel pointPanel, WindowManager windowManager)
        {
            _gameStatService = gameStatService;
            _anchorService = anchorService;
            _pointPanel = pointPanel;
            _windowManager = windowManager;
        }

        protected override TutorialBase<TutorialDesc, TutorialStep> Create() => new Tutorial(this);
    }
}