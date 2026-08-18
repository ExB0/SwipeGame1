using System.Collections.Generic;

using UnityEngine;

using Grid;

namespace PlatformPuzzle.Managers
{
    public class GridManager : MonoBehaviour
    {
        private readonly Dictionary<Vector2Int, Cell> _grid = new();

        [SerializeField] private List<Cell> _cells = new();
        [SerializeField] private List<Cell> _exitCells = new();
        [SerializeField] private int _width = 5;
        [SerializeField] private int _height = 5;

        public int Width => _width;
        public int Height => _height;

        private void Awake()
        {
            ValidateReferences();
            BuildGrid();
        }

        private void OnDrawGizmos()
        {
            if (_cells == null)
            {
                return;
            }

            foreach (Cell cell in _cells)
            {
                if (cell == null)
                {
                    continue;
                }

                Gizmos.color = _exitCells != null &&
                               _exitCells.Contains(cell)
                    ? Color.red
                    : Color.green;

                Gizmos.DrawWireCube(
                    cell.transform.position,
                    Vector3.one * 2f
                );
            }
        }

        public List<Cell> GetExitCells()
        {
            return _exitCells;
        }

        public List<Cell> GetAllCells()
        {
            return _cells;
        }

        public Cell GetCell(Vector2Int gridPosition)
        {
            return _grid.TryGetValue(
                gridPosition,
                out Cell cell
            )
                ? cell
                : null;
        }

        public bool IsCellExists(Vector2Int gridPosition)
        {
            return _grid.ContainsKey(gridPosition);
        }

        public void RebuildGrid()
        {
            BuildGrid();
        }

        public void ClearCells()
        {
            foreach (Cell cell in _cells)
            {
                if (cell == null)
                {
                    continue;
                }

                if (cell.HasCar)
                {
                    cell.TryClearCar();
                }

                cell.SetObstacle(false);
                cell.SetAvailable(false);
            }
        }

        public void SetCellsAvailable(bool available)
        {
            foreach (Cell cell in _cells)
            {
                if (cell != null &&
                    cell.HasCar)
                {
                    cell.SetAvailable(available);
                }
            }
        }

        private void BuildGrid()
        {
            _grid.Clear();

            if (_width <= 0 ||
                _height <= 0)
            {
                Debug.LogError(
                    $"{nameof(GridManager)}: width and height must be greater than zero"
                );

                return;
            }

            if (_cells == null)
            {
                Debug.LogError($"{nameof(GridManager)}: cells list is null");
                return;
            }

            for (int i = 0; i < _cells.Count; i++)
            {
                if (_cells[i] == null)
                {
                    Debug.LogError(
                        $"{nameof(GridManager)}: Cell at index {i} is null"
                    );

                    continue;
                }

                int x = i % _width;
                int y = i / _width;

                if (y >= _height)
                {
                    Debug.LogError(
                        $"Превышена высота сетки! Ячейка {i} " +
                        $"(x:{x}, y:{y}) выходит за пределы Height:{_height}"
                    );

                    continue;
                }

                Vector2Int gridPosition = new(x, y);

                if (_grid.ContainsKey(gridPosition))
                {
                    Debug.LogError(
                        $"{nameof(GridManager)}: duplicate grid position {gridPosition}"
                    );

                    continue;
                }

                _cells[i].Initialize(gridPosition);
                _grid.Add(gridPosition, _cells[i]);
            }
        }

        private void ValidateReferences()
        {
            if (_cells == null ||
                _cells.Count == 0)
            {
                Debug.LogError(
                    $"{nameof(GridManager)}: cells list is empty"
                );
            }

            if (_exitCells == null ||
                _exitCells.Count == 0)
            {
                Debug.LogWarning(
                    $"{nameof(GridManager)}: exit cells list is empty"
                );
            }
        }
    }
}