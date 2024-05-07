using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public enum FitType
    {
        Uniform,
        Width,
        Height
    }

    public class FlexibleGridLayout : LayoutGroup
    {
        [SerializeField] private FitType fitType;
        [SerializeField] private int fixedRows;
        [SerializeField] private int fixedColumns;
        [SerializeField] private Vector2 maxCellSize;
        [SerializeField] private Vector2 cellSize;
        [SerializeField] private Vector2 spacing;
        [SerializeField] private bool squareAspectRatio;
        [SerializeField] private RectTransform background;

        private int _fixedRows;
        private int _fixedColumns;

        public Vector2 CellSize => cellSize;
        public Vector2Int GridSize => new(_fixedColumns, _fixedRows);

        protected override void Start()
        {
            base.Start();
            if (_fixedColumns == 0 || _fixedRows == 0)
            {
                SetSize();
            }
        }
        
        public void SetSize()
        {
            SetSize(fixedRows, fixedColumns);
        }
        
        public void SetSize(int rows, int columns)
        {
            _fixedRows = rows;
            _fixedColumns = columns;
            CalculateLayoutInputHorizontal();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            var pad = padding;
            var padX = 0f;
            var padY = 0f;
            var rows = _fixedRows;
            var columns = _fixedColumns;

            switch (fitType)
            {
                case FitType.Width:
                    columns = _fixedColumns;
                    rows = Mathf.CeilToInt(rectChildren.Count / (float) columns);
                    break;
                case FitType.Height:
                    rows = _fixedRows;
                    rows = Mathf.CeilToInt(rectChildren.Count / (float) rows);
                    break;
                case FitType.Uniform:
                    rows = _fixedRows;
                    columns = _fixedColumns;
                    break;
            }

            var rect = ((RectTransform) rectTransform).rect;
            float parentWidth = rect.width;
            float parentHeight = rect.height;

            float cellWidth = (parentWidth - (pad.left + pad.right) - spacing.x * (columns - 1)) / (float) columns;
            float cellHeight = (parentHeight - (pad.top + pad.bottom) - spacing.y * (rows - 1)) / (float) rows;

            if (fitType == FitType.Width && squareAspectRatio)
            {
                cellHeight = cellWidth;
            }

            if (fitType == FitType.Height && squareAspectRatio)
            {
                cellWidth = cellHeight;
            }

            if (squareAspectRatio)
            {
                if (cellHeight < cellWidth)
                {
                    padX = cellWidth * columns - cellHeight * columns;
                    cellWidth = cellHeight;
                }
                else
                {
                    padY = cellHeight * rows - cellWidth * rows;
                    cellHeight = cellWidth;
                }
            }

            cellSize.x = Mathf.Min(cellWidth, maxCellSize.x);
            cellSize.y = Mathf.Min(cellHeight, maxCellSize.y);

            int columnCount = 0;
            int rowCount = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                rowCount = i / columns;
                columnCount = i % columns;

                var item = rectChildren[i];

                // TextAnchor.UpperLeft
                var xPos = (cellSize.x + spacing.x) * columnCount + pad.left;// + padX;
                var yPos = (cellSize.y + spacing.y) * rowCount + pad.top;// + padY;
                switch (m_ChildAlignment)
                {
                    case TextAnchor.MiddleCenter:
                        xPos += (rect.width - cellSize.x * columns - spacing.x * (columns - 1)  - (pad.left + pad.right)) / 2;
                        yPos += (rect.height - cellSize.y * rows - spacing.y * (rows - 1) - (pad.top + pad.bottom)) / 2;
                        break;
                }

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }

            var xSize = (cellSize.x * columns) + (spacing.x * columns) + pad.left + pad.right;
            var ySize = (cellSize.y * rows) + (spacing.y * rows) + pad.top + pad.bottom;

            // rectTransform.anchoredPosition = new Vector2(padX / 4, padY / 4);

            SetLayoutInputForAxis(xSize, xSize, xSize, 0);
            SetLayoutInputForAxis(ySize, ySize, ySize, 1);
            if(background != null)
            {
                background.sizeDelta = new Vector2(xSize, ySize);
            }
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }
    }
}