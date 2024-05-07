using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Windows;
using LargeNumbers;
using UnityEngine;

namespace GameScripts.Game2248
{ 
    public class TutorialGrid : GridView
    {
        [SerializeField] private PopupBase popupBase;
        
        public List<Cell> filledCells = new List<Cell>();

        public Action OnLineEnd = () => { };
        
        public override bool UseHint() => false;
        
        public void InitCells(GridModel gridModel)
        {
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


                var unitModel = gridModel.Units.ToList().Find(u => u.position == gridPosition);
                if (unitModel != null)
                {
                    var value = unitModel.largeValue;
                    cell.view.Init(value);
                    cell.view.Animator.AnimateScalePingPong();

                    cell.PointerDowned += OnPointerDown;
                    cell.PointerUp += OnPointerUpTutor;
                    cell.PointerEnter += OnPointerEnter;
                    
                    filledCells.Add(cell);
                }
                else
                {
                    cell.view.SetInvisible();
                }
                
                _cells.Add(gridPosition, cell);

                columnCount++;
                columnCount %= gridLayout.GridSize.x;
                if (columnCount == 0)
                {
                    rowCount++;
                    rowCount %= gridLayout.GridSize.y;
                }
            }
        }

        public void Close()
        {
            popupBase.CloseWindow();
        }

        protected override bool CheckBonuses()
        {
            return false;
        }

        protected override IEnumerator EndLine(bool needsX2Offer)
        {
            for (int i = 0; i < _downedCells.Count - 1; i++)
            {
                _downedCells[i].Clear();
            }
            OnLineEnd.Invoke();
            _downedCells.Clear();
            return null;
        }
        
        protected override void OnPointerUp(Cell cell)
        {
            if(!IsDowned)
                return;
            
            IsDowned = false;
            foreach (var line in _lines)
            {
                Destroy(line.gameObject);
            }
            _lines.Clear();

            _gridManager.HideNextSquare();
            if (_downedCells.Count > 1)
            {
                _gridManager.GetNearest2PowValue(_finalValue, out var pow);
                SetNotActive();
                StartCoroutine(FlyParticles(false, pow));
            }

            else
            {
                _downedCells.Clear();
                _finalValue = LargeNumber.zero;
                _activeSum = LargeNumber.zero;
            }
        }
        
        protected override IEnumerator FlyParticles(bool needsX2Offer, int pow)
        {
            var lastCell = _downedCells.Last();
            SetNotActive();
            var endColor = _gridManager.ResourceRepository.GetSquare2248ImageByPow(pow).color;
            var flyTime = _gridManager.AnimParams.time * 2f;

            for (int i = 0; i < _downedCells.Count - 1; i++)
            {
                StartCoroutine(_downedCells[i].view.ParticleController.FlyToTarget(flyTime,
                    lastCell.transform, _downedCells[i].view.ImageData.color, endColor, () => { }));
                StartCoroutine(_downedCells[i].view.AnimateByTime(_gridManager.AnimParams));
            }

            StartCoroutine(lastCell.view.ParticleController.FlyToTarget(flyTime,
                lastCell.transform, lastCell.view.ImageData.color, endColor, () => { }));
            yield return lastCell.view.AnimateByTime(_gridManager.AnimParams);
            var lastView = lastCell.view;
            lastCell.SetFull(Instantiate(viewPrefab));
            lastCell.view.Init(_finalValue, false);
            yield return lastCell.view.AnimateByTime(new UnitViewAnimParams()
            {
                time = _gridManager.AnimParams.time,
                aColor = 1,
                startRotation = _gridManager.AnimParams.endRotation,
                endRotation = 0,
                startScale = _gridManager.AnimParams.endScale,
                endScale = 1
            });
            Destroy(lastView.gameObject);
            yield return EndLine(needsX2Offer);
        }
        
        private void OnPointerUpTutor(Cell cell)
        {
            if (_gridModel.Units.Count != _downedCells.Count)
            {
                ClearLine();
                return;
            }
            else
            {
                for (int i = 0; i < _downedCells.Count; i++)
                {
                    if (_downedCells[i].gridPosition != _gridModel.Units[i].position)
                    {
                        ClearLine();
                        return;
                    }
                }
            }
            
            OnPointerUp(cell);
        }

        private void ClearCells()
        {
            foreach (var cell in _cells)
            {
                Destroy(cell.Value.gameObject);
            }
            _cells.Clear();
            filledCells.Clear();
        }

        private void ClearLine()
        {
            if (!IsDowned)
                return;

            IsDowned = false;
            foreach (var line in _lines)
            {
                Destroy(line.gameObject);
            }

            _lines.Clear();

            foreach (var cell in _downedCells)
            {
                cell.view.SetSelectLight(false);
            }
            _downedCells.Clear();
            _finalValue = LargeNumber.zero;
            _activeSum = LargeNumber.zero;
        }
    }
}
