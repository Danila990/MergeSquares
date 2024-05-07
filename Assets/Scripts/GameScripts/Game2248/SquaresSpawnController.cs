using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.SaveLoad;
using Core.Windows;
using GameStats;
using LargeNumbers;
using UnityEngine;
using Utils;
using Utils.Instructions;
using Zenject;

namespace GameScripts.Game2248
{
    [Serializable]
    public class SpawnPow
    {
        public int value;
        public float chance;

        public SpawnPow(int value, float chance)
        {
            this.value = value;
            this.chance = chance;
        }
    }
    
    [Serializable]
    public class SpawnServiceData
    {
        public List<SpawnPow> spawns = new List<SpawnPow>();
    }

    
    [Serializable]
    public class SquaresSpawnController : MonoBehaviour
    {
        [SerializeField] private List<int> baseSpawnPowList;
        [SerializeField] private Saver saver;

        private List<int> spawnPowList = new List<int>();
        private List<SpawnPow> spawnChancePowList = new List<SpawnPow>();
        private SpawnServiceData _data = new SpawnServiceData();
        
        private GridManager _gridManager;
        
        [Inject]
        public void Construct(GridManager gridManager)
        {
            _gridManager = gridManager;
            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
        }

        private void Awake()
        {
            spawnPowList.AddRange(baseSpawnPowList);
            SetChances();
        }
        
        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }

        public LargeNumber GetRandomValue()
        {
            var res = LargeNumber.zero;
            if (spawnChancePowList.TryWeightRandom(value => value.chance, out SpawnPow pow))
            {
                res = new LargeNumber(Math.Pow(2, pow.value));
            }
            else
            {
                res = new LargeNumber(Math.Pow(2, spawnPowList.GetRandom()));
            }
            
            return res;
        }

        public IEnumerator TryAddSpawn(int spawn)
        {
            if (!spawnPowList.Contains(spawn))
            {
                var maxSpawn = spawnPowList.Max();
                yield return AddSpawn(maxSpawn + 1, spawn);
            }
        }

        public IEnumerator AddNext()
        {
            var nextSpawn = spawnPowList.Max() + 1;
            yield return AddSpawn(nextSpawn, nextSpawn);
        }

        public IEnumerator RemoveMin()
        {
            var minValue = spawnPowList.Min();
            yield return TryRemoveSpawn(minValue);
            yield return _gridManager.CurrentGridView.ClearRemovedCells(minValue);
        }

        private void SetChances()
        {
            spawnChancePowList.Clear();
            var chance = 50f;
            foreach (var pow in spawnPowList)
            {
                spawnChancePowList.Add(new SpawnPow(pow, chance));
                chance /= 2;
            }

            _data.spawns = spawnChancePowList;
            saver.SaveNeeded.Invoke(true);
        }

        private IEnumerator AddSpawn(int add, int maxAdd, float nextTime = 0.0f)
        {
            if (add <= maxAdd)
            {
                yield return new WaitForSeconds(nextTime);
                spawnPowList.Add(add);
                SetChances();
                yield return new WaitForCallback(callback =>
                {
                    _gridManager.ShowSquareSliderAdd(new SquareSliderParams()
                    {
                        targetPowNum = add,
                        winBonus = 25,
                        giftBonus = 10,
                        delay = 1f,
                        ClosePopup = () => { callback?.Invoke();}
                    });
                });
                yield return AddSpawn(add + 1, maxAdd, 0.4f);
            }
        }

        public IEnumerator TryRemoveSpawn(int spawn)
        {
            if (spawnPowList.Contains(spawn))
            {
                spawnPowList.Remove(spawn);
                SetChances();
                yield return new WaitForCallback(callback =>
                {
                    _gridManager.ShowSquareSliderRemove(new SquareSliderParams()
                    {
                        targetPowNum = spawn + 1,
                        winBonus = 25,
                        giftBonus = 10,
                        delay = 1f,
                        ClosePopup = () => { callback.Invoke(); },
                        delete = true,
                    });
                });
            }
        }

        public void SetSpawns(IReadOnlyList<int> spawnPows)
        {
            if (spawnPows == null || spawnPows.Count == 0)
                spawnPowList = new List<int>() { 1, 2, 3, 4 };
            else
                spawnPowList = new List<int>(spawnPows);
            SetChances();
        }
        
        private void Init(SpawnServiceData data, LoadContext context)
        {
            _data = data;
            if (_data.spawns != null && _data.spawns.Count != 0)
            {
                spawnChancePowList = _data.spawns;
                spawnPowList.Clear();
                foreach (var spawnPow in spawnChancePowList)
                {
                    spawnPowList.Add(spawnPow.value);
                }
            }
        }
        
        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new SpawnServiceData()), context);
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}

