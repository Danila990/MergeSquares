using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameScripts.MergeSquares
{
    public class LockerView : MonoBehaviour
    {
        [SerializeField] private int lockLevel;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private List<GameObject> objToLock = new();

        private GridManager _gridManager;

        [Inject]
        public void Construct(GridManager gridManager)
        {
            _gridManager = gridManager;
            _gridManager.OnInited += CheckLock;
            _gridManager.OnStartLevel += CheckLock;
            text.text = lockLevel.ToString();
            foreach (var o in objToLock)
            {
                o.SetActive(false);
            }

            CheckLock();
        }

        private void OnDisable()
        {
            _gridManager.OnInited -= CheckLock;
            _gridManager.OnStartLevel -= CheckLock;
        }

        private void OnDestroy()
        {
            _gridManager.OnInited -= CheckLock;
            _gridManager.OnStartLevel -= CheckLock;
        }

        private void CheckLock()
        {
            if (lockLevel <= _gridManager.CurrentLevel)
            {
                foreach (var o in objToLock)
                {
                    o.SetActive(true);
                }
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}