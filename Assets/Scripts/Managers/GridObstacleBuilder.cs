using System.Collections.Generic;

using UnityEngine;

using Grid;

namespace PlatformPuzzle.Managers
{
    public class GridObstacleBuilder : MonoBehaviour
    {
        private readonly List<GameObject> _spawnedObstacles = new();

        [SerializeField] private GameObject _obstaclePrefab;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private RoadManager _roadManager;

        private void Awake()
        {
            ValidateReferences();
        }

        public void BuildObstacles()
        {
            ClearObstacles();

            if (_gridManager == null)
            {
                Debug.LogError(
                    $"{nameof(GridObstacleBuilder)}: GridManager is null"
                );

                return;
            }

            foreach (Cell cell in _gridManager.GetAllCells())
            {
                if (cell == null)
                {
                    continue;
                }

                if (!cell.HasCar &&
                    !_gridManager.GetExitCells().Contains(cell))
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

        public void ClearObstacles()
        {
            foreach (GameObject obstacle in _spawnedObstacles)
            {
                if (obstacle != null)
                {
                    Destroy(obstacle);
                }
            }

            _spawnedObstacles.Clear();

            if (_gridManager == null)
            {
                return;
            }

            foreach (Cell cell in _gridManager.GetAllCells())
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
                    $"{nameof(GridObstacleBuilder)}: obstacle prefab is missing"
                );
            }

            if (_gridManager == null)
            {
                Debug.LogError(
                    $"{nameof(GridObstacleBuilder)}: GridManager is missing"
                );
            }

            if (_roadManager == null)
            {
                Debug.LogError(
                    $"{nameof(GridObstacleBuilder)}: RoadManager is missing"
                );
            }
        }
    }
}