using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Splines;

using Grid;
using InterFaces;
using PlatformPuzzle.Levels;
using PlatformPuzzle.Pathfinder;
using Units;

namespace PlatformPuzzle.Managers
{
    public class GridCarSpawner : MonoBehaviour
    {
        private readonly List<Car> _carsToDestroy = new();
        private readonly List<Car> _activeCars = new();
        private readonly PathFinder _pathFinder = new();

        [SerializeField] private MonoBehaviour _factorySource;
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Transform _splineStartPoint;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private RoadManager _roadManager;
        [SerializeField] private LevelFlowController _flowController;

        private IUnitFactory _unitFactory;

        private void Awake()
        {
            ValidateReferences();

            _unitFactory = _factorySource as IUnitFactory;

            if (_unitFactory == null)
            {
                Debug.LogError(
                    $"{nameof(GridCarSpawner)}: FactorySource does not implement IUnitFactory"
                );

                enabled = false;
            }
        }

        public void SpawnCarAt(
            Vector2Int gridPosition,
            UnitType unitType,
            UnitColor unitColor)
        {
            if (_unitFactory == null)
            {
                Debug.LogError($"{nameof(GridCarSpawner)}: UnitFactory is null");
                return;
            }

            if (_gridManager == null)
            {
                Debug.LogError($"{nameof(GridCarSpawner)}: GridManager is null");
                return;
            }

            if (_roadManager == null)
            {
                Debug.LogError($"{nameof(GridCarSpawner)}: RoadManager is null");
                return;
            }

            Cell cell = _gridManager.GetCell(gridPosition);

            if (cell == null)
            {
                Debug.LogError($"No cell at position {gridPosition}");
                return;
            }

            if (cell.HasCar)
            {
                Debug.LogWarning(
                    $"Cell {gridPosition} already has car"
                );

                return;
            }

            if (cell.IsObstacle)
            {
                Debug.LogWarning(
                    $"Cell {gridPosition} is obstacle"
                );

                return;
            }

            GameObject carObject = _unitFactory.Create(
                unitType,
                unitColor,
                cell.transform.position
            );

            if (carObject == null)
            {
                Debug.LogError(
                    $"Failed to create car at {gridPosition}"
                );

                return;
            }

            if (!carObject.TryGetComponent(out Car car))
            {
                Debug.LogError(
                    "Created object does not contain Car component"
                );

                Destroy(carObject);
                return;
            }

            cell.TrySetCar(car);

            car.Initialize(
                _gridManager,
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

        public void ClearCars()
        {
            foreach (Car car in _carsToDestroy)
            {
                if (car != null)
                {
                    Destroy(car.gameObject);
                }
            }

            _carsToDestroy.Clear();
            _activeCars.Clear();
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

        private void ValidateReferences()
        {
            if (_factorySource == null)
            {
                Debug.LogError(
                    $"{nameof(GridCarSpawner)}: factory source is missing"
                );
            }

            if (_splineContainer == null)
            {
                Debug.LogError(
                    $"{nameof(GridCarSpawner)}: spline container is missing"
                );
            }

            if (_splineStartPoint == null)
            {
                Debug.LogError(
                    $"{nameof(GridCarSpawner)}: spline start point is missing"
                );
            }

            if (_gridManager == null)
            {
                Debug.LogError(
                    $"{nameof(GridCarSpawner)}: GridManager is missing"
                );
            }

            if (_roadManager == null)
            {
                Debug.LogError(
                    $"{nameof(GridCarSpawner)}: RoadManager is missing"
                );
            }

            if (_flowController == null)
            {
                Debug.LogError(
                    $"{nameof(GridCarSpawner)}: LevelFlowController is missing"
                );
            }
        }
    }
}