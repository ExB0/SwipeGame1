using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Splines;

using Units;
using Grid;
using InterFaces;
using PlatformPuzzle.Pathfinder;
using PlatformPuzzle.Levels;

namespace PlatformPuzzle.Managers
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private GameObject _obstaclePrefab;
        [SerializeField] private List<Cell> _cells = new();
        [SerializeField] private List<Cell> _exitCells = new();
        [SerializeField] private MonoBehaviour _factorySource;
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Transform _splineStartPoint;
        [SerializeField] private List<Car> _activeCars = new();
        [SerializeField] private RoadManager _roadManager;
        [SerializeField] private LevelFlowController _flowController;

        private readonly List<Car> _carsToDestroy = new();
        private readonly List<GameObject> _spawnedObstacles = new();
        private readonly Dictionary<Vector2Int, Cell> _grid = new();

        private readonly PathFinder _pathFinder = new();

        private IUnitFactory _unitFactory;

        public int Width { get; set; } = 5;
        public int Height { get; set; } = 5;

        public List<Cell> GetExitCells() => _exitCells;

        public List<Cell> GetAllCells() => _cells;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            ValidateReferences();

            _unitFactory = _factorySource as IUnitFactory;

            if (_unitFactory == null)
            {
                Debug.LogError("Нет Фабрики");
                enabled = false;
                return;
            }

            if (_roadManager == null)
            {
                _roadManager = FindAnyObjectByType<RoadManager>();
            }

            if (_flowController == null)
            {
                _flowController = FindAnyObjectByType<LevelFlowController>();
            }

            if (_roadManager == null)
            {
                Debug.LogError("RoadManager is null");
            }
            else
            {
                _roadManager.Initialize(this);
            }

            BuildGrid();
        }

        public void SpawnCarAt(
            Vector2Int gridPosition,
            UnitType unitType,
            UnitColor unitColor)
        {
            if (_unitFactory == null)
            {
                Debug.LogError("UnitFactory is null");
                return;
            }

            if (_roadManager == null)
            {
                Debug.LogError("RoadManager is null");
                return;
            }

            Cell cell = GetCell(gridPosition);

            if (cell == null)
            {
                Debug.LogError($"No cell at position {gridPosition}");
                return;
            }

            if (cell.HasCar)
            {
                Debug.LogWarning($"Cell {gridPosition} already has car");
                return;
            }

            if (cell.IsObstacle)
            {
                Debug.LogWarning($"Cell {gridPosition} is obstacle");
                return;
            }

            GameObject carObj = _unitFactory.Create(
                unitType,
                unitColor,
                cell.transform.position
            );

            if (carObj == null)
            {
                Debug.LogError($"Failed to create car at {gridPosition}");
                return;
            }

            if (!carObj.TryGetComponent(out Car car))
            {
                Debug.LogError("Created object does not contain Car component");
                Destroy(carObj);
                return;
            }

            cell.TrySetCar(car);

            car.Initialize(
                this,
                _roadManager,
                _flowController,
                _pathFinder
            );

            car.SetSpline(_splineContainer);
            car.SetRoad(_splineStartPoint);

            _activeCars.Add(car);
            _carsToDestroy.Add(car);

            _roadManager.UpdateCells();
        }

        public void BuildObstacles()
        {
            ClearObstacles();

            foreach (Cell cell in GetAllCells())
            {
                if (!cell.HasCar && !_exitCells.Contains(cell))
                {
                    cell.SetObstacle(true);

                    GameObject obstacle = Instantiate(
                        _obstaclePrefab,
                        cell.transform.position,
                        Quaternion.identity
                    );

                    _spawnedObstacles.Add(obstacle);
                }
                else
                {
                    cell.SetObstacle(false);
                }
            }

            _roadManager?.UpdateCells();
        }

        public Cell GetCell(Vector2Int gridPosition)
        {
            return _grid.TryGetValue(gridPosition, out Cell cell)
                ? cell
                : null;
        }

        public bool IsCellExists(Vector2Int gridPos)
        {
            return _grid.ContainsKey(gridPos);
        }

        public void ClearGrid()
        {
            foreach (Cell cell in _cells)
            {
                if (cell != null && cell.HasCar)
                {
                    cell.TryClearCar();
                }
            }

            foreach (Car car in _carsToDestroy)
            {
                if (car != null)
                {
                    Destroy(car.gameObject);
                }
            }

            _carsToDestroy.Clear();
            _activeCars.Clear();

            ClearObstacles();
        }

        public void RebuildGrid()
        {
            BuildGrid();
        }

        public void RemoveCar(Car car)
        {
            if (car == null)
            {
                Debug.LogWarning("Нет машины");
                return;
            }

            _activeCars.Remove(car);
        }

        public bool HasActiveCars()
        {
            _activeCars.RemoveAll(car => car == null);

            return _activeCars.Count > 0;
        }

        public void SetCellsAvailable(bool available)
        {
            foreach (Cell cell in _cells)
            {
                if (cell != null && cell.HasCar)
                {
                    cell.SetAvailable(available);
                }
            }
        }

        private void BuildGrid()
        {
            _grid.Clear();

            if (Width <= 0 || Height <= 0)
            {
                Debug.LogError("Grid width and height must be greater than zero");
                return;
            }

            for (int i = 0; i < _cells.Count; i++)
            {
                if (_cells[i] == null)
                {
                    Debug.LogError($"Cell at index {i} is null");
                    continue;
                }

                int x = i % Width;
                int y = i / Width;

                if (y >= Height)
                {
                    Debug.LogError(
                        $"Превышена высота сетки! Ячейка {i} " +
                        $"(x:{x}, y:{y}) выходит за пределы Height:{Height}"
                    );

                    continue;
                }

                Vector2Int gridPos = new(x, y);

                if (_grid.ContainsKey(gridPos))
                {
                    Debug.LogError($"Duplicate grid position: {gridPos}");
                    continue;
                }

                _cells[i].Initialize(gridPos);
                _grid.Add(gridPos, _cells[i]);
            }
        }

        private void ClearObstacles()
        {
            foreach (GameObject obstacle in _spawnedObstacles)
            {
                if (obstacle != null)
                {
                    Destroy(obstacle);
                }
            }

            _spawnedObstacles.Clear();

            foreach (Cell cell in _cells)
            {
                if (cell != null)
                {
                    cell.SetObstacle(false);
                }
            }
        }

        private void ValidateReferences()
        {
            if (_obstaclePrefab == null)
            {
                Debug.LogError(
                    $"{nameof(GridManager)}: obstacle prefab is missing"
                );
            }

            if (_factorySource == null)
            {
                Debug.LogError(
                    $"{nameof(GridManager)}: factory source is missing"
                );
            }

            if (_splineContainer == null)
            {
                Debug.LogError(
                    $"{nameof(GridManager)}: spline container is missing"
                );
            }

            if (_splineStartPoint == null)
            {
                Debug.LogError(
                    $"{nameof(GridManager)}: spline start point is missing"
                );
            }

            if (_cells == null || _cells.Count == 0)
            {
                Debug.LogError(
                    $"{nameof(GridManager)}: cells list is empty"
                );
            }

            if (_exitCells == null || _exitCells.Count == 0)
            {
                Debug.LogWarning(
                    $"{nameof(GridManager)}: exit cells list is empty"
                );
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Cell cell in _cells)
            {
                if (cell == null)
                {
                    continue;
                }

                Gizmos.color = _exitCells.Contains(cell)
                    ? Color.red
                    : Color.green;

                Gizmos.DrawWireCube(
                    cell.transform.position,
                    Vector3.one * 2f
                );
            }
        }
    }
}