using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private GameObject _obstaclePrefab;

    [SerializeField] private List<Cell> _cells = new List<Cell>();
    [SerializeField] private List<Cell> _exitCells = new List<Cell>();

    [SerializeField] private MonoBehaviour _factorySource;

    [SerializeField] private SplineContainer _splineContainer;

    [SerializeField] private Transform _splineStartPoint;
    [SerializeField] private List<Car> _activeCars = new();
    private List<Car> _carsToDestroy = new();
    private List<GameObject> _spawnedObstacles = new List<GameObject>();

    public int Width { get; set; } = 5;
    public int Height { get; set; } = 5;

    private Dictionary<Vector2Int, Cell> _grid = new Dictionary<Vector2Int, Cell>();
    private IUnitFactory _unitFactory;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        _unitFactory = _factorySource as IUnitFactory;

        if (_unitFactory == null)
            Debug.LogError("Нет Фабрики");

        BuildGrid();
    }

    private void BuildGrid()
    {
        _grid.Clear();
        for (int i = 0; i < _cells.Count; i++)
        {
            int x = i % Width;
            int y = i / Width;

            if (y >= Height)
            {
                Debug.LogError($"Превышена высота сетки! Ячейка {i} (x:{x}, y:{y}) выходит за пределы Height:{Height}");
                continue;
            }

            Vector2Int gridPos = new Vector2Int(x, y);
            _cells[i].Initialize(gridPos);
            _grid[gridPos] = _cells[i];
        }
    }


    public void SpawnCarAt(Vector2Int gridPosition, UnitType unitType, UnitColor unitColor)
    {
        Cell cell = GetCell(gridPosition);
        if (cell == null || cell.HasCar) return;

        GameObject carObj = _unitFactory.Create(unitType, unitColor, cell.transform.position);
        if (carObj != null && carObj.TryGetComponent(out Car car))
        {
            cell.TrySetCar(car);
            car.SetSpline(_splineContainer);
            car.SetRoad(_splineStartPoint);

            _activeCars.Add(car);
            _carsToDestroy.Add(car);
        }
    }
    public void BuildObstacles()
    {

        foreach (var obj in _spawnedObstacles)
        {
            if (obj != null)
                Destroy(obj);
        }
        _spawnedObstacles.Clear();

        foreach (var cell in GetAllCells())
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
    }

    public Cell GetCell(Vector2Int gridPosition)
    {
        return _grid.TryGetValue(gridPosition, out var cell) ? cell : null;
    }

    public bool IsCellExists(Vector2Int gridPos)
    {
        return _grid.ContainsKey(gridPos);
    }

    public List<Cell> GetExitCells() => _exitCells;
    public List<Cell> GetAllCells() => _cells;

    private void OnDrawGizmos()
    {
        foreach (var cell in _cells)
        {
            Gizmos.color = _exitCells.Contains(cell) ? Color.red : Color.green;
            Gizmos.DrawWireCube(cell.transform.position, Vector3.one * 2f);
        }
    }
    public void ClearGrid()
    {
        foreach (var car in _carsToDestroy)
        {
            if (car != null)
                Destroy(car.gameObject);
        }
        
        _carsToDestroy.Clear();
        _activeCars.Clear();
        

        foreach (var cell in _cells)
        {
            if (cell != null && cell.HasCar)
                cell.TryClearCar();
        }
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
        }
        _activeCars.Remove(car);
    }
    
    public bool HasActiveCars()
    {
        _activeCars.RemoveAll(c => c == null);

        return _activeCars.Count > 0;
    }
}