using System;
using System.Collections;
using System.Collections.Generic;
using Core.Windows;
using LeadboardScores;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScripts.MergeSquares.InfinityLevel
{
    public class LeaderboardStatsPopup : GenericPopupContent
    {
        [SerializeField] private LeadboardStatLine leadboardLinePrefab;
        [SerializeField] private Transform rootCurrent;
        [SerializeField] private Transform rootPrev;
        // [SerializeField] private GameObject previousRewardsButton;
        // [SerializeField] private GameObject currentRewardsButton;
        // [SerializeField] private GameObject noPrevRewardsText;
        private List<LeadboardStatLine> lines = new List<LeadboardStatLine>();

        private LeadboardScoresService _leadboardScoresService;

        [Inject]
        private void Construct(WindowManager windowManager, LeadboardScoresService leadboardScoresService)
        {
            _windowManager = windowManager;
            _leadboardScoresService = leadboardScoresService;
        }
        
        public override string GetWindowId() => "LeaderboardStatsPopup";

        public override void Init(object dataToInit, PopupBase popupBase)
        {
            
        }

        private void Start()
        {
            DrawPanels();
            _leadboardScoresService.WeekReset += DrawPanels;
            _leadboardScoresService.MonthReset += DrawPanels;
        }

        public void ShowPrevious()
        {
            // previousRewardsButton.SetActive(false);
            // noPrevRewardsText.SetActive(false);
            // currentRewardsButton.SetActive(true);
            // foreach (var line in lines)
            // {
                // Destroy(line.gameObject);
            // }
            // lines.Clear();
            if (_leadboardScoresService.Data.rewardsToClaim.Count <= 0)
            {
                rootPrev.gameObject.SetActive(false);
                return;
            }
            foreach (var rewards in _leadboardScoresService.Data.rewardsToClaim)
            {
                var line = Instantiate(leadboardLinePrefab, rootPrev);
                line.Init(_leadboardScoresService.CalcRewards(rewards.isMonth, rewards.placeInRating), rewards, true);
                lines.Add(line);
                // line.Delete += OnLineDelete;
            }
        }

        public void ShowCurrent()
        {
            // if (_leadboardScoresService.Data.rewardsToClaim.Count <= 0)
            // {
            //     noPrevRewardsText.SetActive(true);
            //     previousRewardsButton.SetActive(false);
            // }
            // else
            // {
            //     noPrevRewardsText.SetActive(false);
            //     previousRewardsButton.SetActive(true);
            // }
            // currentRewardsButton.SetActive(false);
            // foreach (var line in lines)
            // {
                // Destroy(line.gameObject);
            // }
            // lines.Clear();
            if (_leadboardScoresService.Data.rewardsInProgress.Count <= 0)
            {
                rootCurrent.gameObject.SetActive(false);
                return;
            }
            foreach (var rewards in _leadboardScoresService.Data.rewardsInProgress)
            {
                var line = Instantiate(leadboardLinePrefab, rootCurrent);
                line.Init(_leadboardScoresService.CalcRewards(rewards.isMonth, rewards.placeInRating), rewards);
                lines.Add(line);
            }
        }

        public void OnClickClose()
        {
            _windowManager.CloseAll(GetWindowId());
        }

        public override void Dispose(PopupBaseCloseType closeType)
        {
            _leadboardScoresService.WeekReset += DrawPanels;
            _leadboardScoresService.MonthReset += DrawPanels;
        }

        private void DrawPanels()
        {
            foreach (var line in lines)
            {
                Destroy(line.gameObject);
            }
            lines.Clear();
            ShowCurrent();
            ShowPrevious();
        }
        
        

        // private void OnLineDelete(LeadboardStatLine line)
        // {
            // line.Delete -= OnLineDelete;
            // lines.Remove(line);
        // }
    }
}