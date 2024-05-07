using System;
using System.Collections.Generic;
using Core.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts.PointPanel
{
    public class PointLine : MonoBehaviour
    {    
        [SerializeField] private SoundSource newLine;
        [SerializeField] private PointCell cellPrefab;
        [SerializeField] private Transform pointRoot;
        [SerializeField] private Image background;
        [SerializeField] private Sprite activeBackground;
    
        public void PlayNewLineSound() => newLine.Play();

        public List<PointData> LineFill => _lineFill;
        public RectTransform TargetTransform => _cells[_targetCellIndex].TargetTransform;
        public int CurrentIndex => _targetCellIndex;

        public Action lineEnd;

        private List<PointCell> _cells = new();
        private List<PointData> _lineFill = new();

        private int _targetCellIndex;

        public bool SetNextTarget()
        {
            var point = GetFirstFree();
            if (point != null)
            {
                point.busy = true;
                _targetCellIndex = point.index;
                return true;
            }
            return false;
        }

        public void SetSkin(BallSkin ballSkin)
        {
            foreach (var cell in _cells)
            {
                cell.SetSkin(ballSkin);
            }
        }

        public void SetSize(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var cell = Instantiate(cellPrefab, pointRoot);
                _cells.Add(cell);
                _lineFill.Add(new PointData(EPointId.None, i));
            }
        }

        public void SetEnd(List<EPointState> pointStates, int index = 0)
        {
            _cells[index].animationEnd += SetEndNext;
            _cells[index].SetState(pointStates[index]);

            void SetEndNext()
            {
                _cells[index].animationEnd -= SetEndNext;
                index++;
                if (index >= _cells.Count)
                {
                    lineEnd.Invoke();
                    return;
                }
                SetEnd(pointStates, index);
            }
        }

        public void SetOpened(List<PointData> openedPoints, BallSkin skin)
        {
            foreach (var point in openedPoints)
            {
                _cells[point.index].SetPointColor(point.id);
                _cells[point.index].SetSkin(skin);
                _lineFill[point.index].id = point.id;
            }
        }

        public void SetActive()
        {
            // PlayNewLineSound();
            background.sprite = activeBackground;
            _targetCellIndex = GetAndLockFree();
        }
    
        public void ColorPoint(EPointId pointId, BallSkin skin, int index)
        {
            _cells[index].SetPointColor(pointId);
            _cells[index].SetSkin(skin);
            _lineFill[index].id = pointId;
        }

        public bool IsFull()
        {
            return _lineFill.Find(p => p.id == EPointId.None) == null;
        }

        public void Clear()
        {
            foreach (var cell in _cells)
            {
                cell.Hide();
            }
        }

        private int GetAndLockFree()
        {
            var freePoint = GetFirstFree();
            freePoint.busy = true;
            return freePoint.index;
        }

        private PointData GetFirstFree() => _lineFill.Find(p => p.id == EPointId.None && !p.busy);
    }
}
