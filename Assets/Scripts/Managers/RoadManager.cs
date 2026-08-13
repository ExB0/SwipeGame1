using Cysharp.Threading.Tasks;
using UnityEngine;

using Units;
using Grid;

namespace PlatformPuzzle.Managers
{
    public class RoadManager : MonoBehaviour
    {
        [SerializeField] private int _maxCarsNumber = 3;
        [SerializeField] private int _currentCarsNumber;
        [SerializeField] private GridManager _gridManager;

        private async void Start()
        {
            await UniTask.Yield();

            if (_gridManager == null)
            {
                _gridManager = GridManager.Instance;
            }

            UpdateCells();
        }

        public void Initialize(GridManager gridManager)
        {
            _gridManager = gridManager;
        }

        public bool IsRoadFull()
        {
            return _currentCarsNumber >= _maxCarsNumber;
        }

        public void AddCar()
        {
            _currentCarsNumber++;
            UpdateCells();
        }

        public void RemoveCar()
        {
            _currentCarsNumber--;

            if (_currentCarsNumber < 0)
            {
                _currentCarsNumber = 0;
            }

            UpdateCells();
        }

        public void ClearCars()
        {
            _currentCarsNumber = 0;
            UpdateCells();
        }

        public bool HasCars()
        {
            return _currentCarsNumber > 0;
        }

        public void UpdateCells()
        {
            if (_gridManager == null)
            {
                return;
            }

            foreach (Cell cell in _gridManager.GetAllCells())
            {
                if (cell == null)
                {
                    continue;
                }

                if (!cell.HasCar)
                {
                    cell.SetAvailable(false);
                    continue;
                }

                if (cell.IsObstacle || cell.IsReserved)
                {
                    cell.SetAvailable(false);
                    continue;
                }

                Car car = cell.CurrentCar;

                if (car == null || car.IsMoving)
                {
                    cell.SetAvailable(false);
                    continue;
                }

                bool canMove = !IsRoadFull() && car.CanMoveFrom(cell);

                cell.SetAvailable(canMove);
            }
        }
    }
}