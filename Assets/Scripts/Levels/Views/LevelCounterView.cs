using System;
using TMPro;
using UnityEngine;
using Zenject;

namespace Levels.Views
{
    public class LevelCounterView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private LevelService _levelService;

        [Inject]
        private void Construct(LevelService levelService)
        {
            _levelService = levelService;
            _levelService.LevelChanged += ChangeLevel;
        }

        private void OnDestroy()
        {
            _levelService.LevelChanged -= ChangeLevel;
        }

        private void Start()
        {
            if (_levelService.Ready)
            {
                ChangeLevel(_levelService.Level);
            }
        }

        private void ChangeLevel(int level)
        {
            text.text = level.ToString();
        }
    }
}