using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.PointPanel
{
    [Serializable]
    public class BottomLinePrefabData
    {
        public int count;
        public BottomLine bottomLine;
    }

    [Serializable]
    public class LineSeparatorData
    {
        public int count;
        public List<int> linesSizes;
    }

    public class BottomPanel : MonoBehaviour
    {
        [SerializeField] private List<BottomLinePrefabData> linePrefabs;
        [SerializeField] private List<LineSeparatorData> separatorDatas;
        [SerializeField] private BottomCell bottomCellPrefab;
        [SerializeField] private int separateLineCount;

        public List<BottomCell> Cells => _cells;

        private List<BottomLine> _lines = new List<BottomLine>();
        private List<BottomCell> _cells = new List<BottomCell>();

        public void SetSkin(BallSkin ballSkin)
        {
            foreach (var cell in _cells)
            {
                cell.SetSkin(ballSkin);
            }
        }

        public void SetFailedCellsOverlaysByIds(List<EPointId> failedIds)
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                if (failedIds.Contains(_cells[i].PointId) && _cells[i].WasClicked)
                {
                    _cells[i].SetClickable(false);
                    _cells[i].SetDisabledColor();
                }
            }
        }

        public void SetPoints(List<EPointId> pointIds, Action<EPointId, RectTransform> onClick)
        {
            foreach (var cell in _cells)
            {
                Destroy(cell.gameObject);
            }
            _cells.Clear();
            foreach (var line in _lines)
            {
                Destroy(line.gameObject);
            }
            _lines.Clear();
            if (pointIds.Count >= separateLineCount)
            {
                int index = 0;
                var lineSizes = separatorDatas.Find(s => s.count == pointIds.Count).linesSizes;
                foreach (var lineSize in lineSizes)
                {
                    var line = SpawnLine(lineSize);
                    var indexBorder = Math.Min(lineSize + index, pointIds.Count);
                    for (; index < indexBorder; index++)
                    {
                        var newCell = Instantiate(bottomCellPrefab, line);
                        newCell.clicked += onClick;
                        newCell.SetColor(pointIds[index]);
                        _cells.Add(newCell);
                    }
                }
            }
            else
            {
                var line = SpawnLine(pointIds.Count);
                foreach (var id in pointIds)
                {
                    var newCell = Instantiate(bottomCellPrefab, line);
                    newCell.clicked += onClick;
                    newCell.SetColor(id);
                    _cells.Add(newCell);
                }
            }
        }

        private Transform SpawnLine(int size)
        {
            var line = Instantiate(linePrefabs.Find(l => l.count == size).bottomLine, transform);
            _lines.Add(line);
            return line.ButtonsRoot;
        }
    }
}