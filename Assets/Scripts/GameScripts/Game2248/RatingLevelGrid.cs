using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Windows;
using DG.Tweening;
using GameScripts.Game2248;
using GameStats;
using LargeNumbers;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Instructions;

namespace GameScripts.Game2248
{
    public class RatingLevelGrid : GridView
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private TextMeshProUGUI movesLeftText;
        [SerializeField] private int movesLeft = 10;
        [SerializeField] TextMeshProUGUI pointsText;
        
        private LargeNumber points = LargeNumber.zero;
        private List<SpawnPow> spawnChancePowList = new List<SpawnPow>();


        public void InitCells(GridModel gridModel)
        {
            pointsText.text = points.ToString();
            movesLeftText.text = movesLeft.ToString();
            _gridModel = gridModel;
            gridLayout.SetSize(gridModel.size.y, gridModel.size.x);
            var columnCount = 0;
            var rowCount = 0;
            ClearCells();
            
            for (int i = 0; i < gridModel.size.x * gridModel.size.y; i++)
            {
                var cell = Instantiate(cellPrefab, gridLayout.transform);
                var gridPosition = new Vector2Int(columnCount, rowCount);
                var rectTransform = (RectTransform)cell.transform;
                cell.localPosition = rectTransform.localPosition;
                cell.worldPosition = rectTransform.position;
                cell.gridPosition = gridPosition;
                cell.SetFull(Instantiate(viewPrefab));
                
                var value = _squaresSpawnController.GetRandomValue();

                cell.view.Init(value);
                cell.view.Animator.AnimateScalePingPong();
                // cell.anchor.SetSorting(sortingLayerName, sortingOrder);
                    
                cell.PointerDowned += OnPointerDown;
                cell.PointerUp += OnPointerUp;
                cell.PointerEnter += OnPointerEnter;

                _cells.Add(gridPosition, cell);

                columnCount++;
                columnCount %= gridLayout.GridSize.x;
                if (columnCount == 0)
                {
                    rowCount++;
                    rowCount %= gridLayout.GridSize.y;
                }
                
                cell.view.SetMaxFrame(false);
            }

            foreach (var pow in gridModel.StartPows)
            {
                var chance = 50f;
                spawnChancePowList.Add(new SpawnPow(pow, chance));
                chance /= 2;
            }

        }

        protected override void OnPointerUp(Cell cell)
        {
            if (!IsDowned)
                return;

            IsDowned = false;
            foreach (var line in _lines)
            {
                Destroy(line.gameObject);
            }

            _lines.Clear();

            if (_downedCells.Count > 1)
            {
                var pointsAnim = DOTween.To(() => points, newPoints =>
                {
                    pointsText.text = Math.Round(newPoints).ToString();
                }, points + _activeSum, 1f);

                pointsAnim.OnKill(() =>
                {
                    points += _activeSum;
                    pointsText.text = points.ToString();
                });
                
                _gridManager.GetNearest2PowValue(_finalValue, out var pow);
                StartCoroutine(FlyParticles(false, pow));
                _gridManager.PlayEndLine();
            }

            else
            {
                _downedCells[0].view.SetSelectLight(false);
                cell.PlayClick();
                _downedCells.Clear();
                _finalValue = LargeNumber.zero;
                _activeSum = LargeNumber.zero;
            }
        }
        
        protected override IEnumerator FixGrid()
        {
            movesLeft--;
            movesLeftText.text = movesLeft.ToString();
            
            for (int i = 0; i < _downedCells.Count - 1; i++)
            {
                _downedCells[i].Clear();
            }

            yield return MoveCellsDown();
            yield return FullFreeCells();

            var maxPow = spawnChancePowList.Max(p => p.value);
            spawnChancePowList.Add(new SpawnPow(maxPow + 1, 0));
            var minPow = spawnChancePowList.Min(p => p.value);
            spawnChancePowList.Remove(spawnChancePowList.Find(p => p.value == minPow));
            SetChances();
            yield return ClearRemovedCells(minPow);

            _lastActivityTime = Time.time;
            ClearLine();

            if (movesLeft <= 0 || !TryFindMergePossibilities())
            {
                _gameStatService.TryAddLocalDelta(EGameStatType.Soft, 100);
                popupBase.CloseWindow();
            }
            
            SetActive();
        }
        
        protected override void FullDownedCell(Cell cell, Cell nextCell)
        {
            cell.SetFull(Instantiate(viewPrefab));
            cell.view.InitInvisible(nextCell.view.Value);
            var nextCellView = nextCell.view;
            nextCell.SetFree();
            nextCellView.Animator.AnimateFlying(cell, () =>
            {
                cell.view.SetVisible();
                cell.view.SetMaxFrame(false);
                Destroy(nextCellView.gameObject);
            });
        }
        
        protected override IEnumerator FullFreeCells()
        {
            var cells = _cells.Values.ToList().Where(c => c.IsFree).ToList();
            foreach(var cell in cells)
            {
                cell.SetFull(Instantiate(viewPrefab));
                var val = GetRandomValue();
                cell.view.Init(val);

                var localPosition = cell.view.transform.localPosition;
                localPosition = new Vector3(
                    localPosition.x,
                    localPosition.y + ((cell.transform as RectTransform).sizeDelta.y * _gridModel.size.y),
                    localPosition.z);
                cell.view.transform.localPosition = localPosition;

                if(cell == cells.Last())
                {
                    yield return new WaitForCallback(callback => { cell.view.Animator.AnimateFlying(cell, callback); });
                }
                else
                {
                    cell.view.Animator.AnimateFlying(cell);
                }
            }
        }
        
        private void SetChances()
        {
            var chance = 50f;
            foreach (var pow in spawnChancePowList)
            {
                pow.chance = chance;
                chance /= 2;
            }
        }
        
        private LargeNumber GetRandomValue()
        {
            var res = LargeNumber.zero;
            if (spawnChancePowList.TryWeightRandom(value => value.chance, out SpawnPow pow))
            {
                res = new LargeNumber(Math.Pow(2, pow.value));
            }
            else
            {
                res = new LargeNumber(Math.Pow(2, spawnChancePowList.GetRandom().value));
            }
            
            return res;
        }
    }
}
