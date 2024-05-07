using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameStats
{
    public class DebugStatChanger : MonoBehaviour
    {
        [SerializeField] private EGameStatType statType;
        [SerializeField] private TMP_InputField valueText;
        [SerializeField] private bool setTo;
        private GameStatService _gameStatService;

        [Inject]
        public void Construct(GameStatService gameStatService) => _gameStatService = gameStatService;
        
        public void MakeMagic()
        {
            var value = Convert.ToInt32(valueText.text);
            if(setTo)
            {
                _gameStatService.TrySet(statType, value);
            }
            else
            {
                _gameStatService.TryIncWithAnim(statType, value);
            }
        }
    }
}


