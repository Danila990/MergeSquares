using System.Collections;
using System.Collections.Generic;
using Core.Localization;
using GameStats;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Popups
{
    public class SkinLevelsPopupBase : MonoBehaviour
    {
        [SerializeField] protected EGameStatType levelType;
        [SerializeField] protected LocalizeUi levelText;

        protected int _levelIndex = 0;
        protected int _levelIndexMax = 0;
        
        protected GameStatLeveled _gameStatLeveled;

        [Inject]
        public void Construct(GameStatLeveled gameStatLeveled)
        {
            _gameStatLeveled = gameStatLeveled;
        }
        
        [UsedImplicitly]
        public void OnRightClick()
        {
            _levelIndex++;
            if (_levelIndex > _levelIndexMax)
            {
                _levelIndex = _levelIndexMax;
            }
            UpdateViews();
        }
        
        [UsedImplicitly]
        public void OnLeftClick()
        {
            _levelIndex--;
            if (_levelIndex < 0)
            {
                _levelIndex = 0;
            }
            UpdateViews();
        }

        protected virtual void UpdateViews(){}
    }
}
