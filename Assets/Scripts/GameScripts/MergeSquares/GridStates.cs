using System.Collections.Generic;
using System.Linq;
using GameStats;
using UnityEngine;

namespace GameScripts.MergeSquares
{
    public class GridState
    {
        public readonly List<CellData> Cells;
        public readonly int CollectedPoins;

        public GridState(List<CellData> cells, int collectedPoins)
        {
            Cells = cells;
            CollectedPoins = collectedPoins;
        }
    }

    public class GridStates
    {
        private Stack<GridState> _states;
        private GameStatService _statService;

        public int Count => _states.Count;

        public GridStates(GameStatService statService)
        {
            _states = new Stack<GridState>();
            _statService = statService;
        }

        public bool CanTake()
        {
            return _states.Count > 1;
        }

        public GridState TakePreviuosState()
        {
            _states.Pop();  // remove current state
            _statService.TryDec(EGameStatType.StepBacks, 1);
            return _states.Pop();
        }

        public void PutCurrentState(GridState state)
        {
            _states.Push(state);
        }

        public void Clear()
        {
            _states.Clear();
        }

        public void RemoveCellFromStates(Vector2Int position)
        {
            foreach (var state in _states)
            {
                var cell = state.Cells.Where(c => c.position == position).FirstOrDefault();
                
                if (cell != null)
                    state.Cells.Remove(cell);
            }
        }

    }
}
