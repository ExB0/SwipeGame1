using UnityEngine;
using Cysharp.Threading.Tasks;

public class LevelConstructor : MonoBehaviour
{
    public static LevelConstructor Instance { get; private set; }

    [SerializeField] private Spawner[] _spawners;
    [SerializeField] private LevelData[] _levels;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private RoadManager _roadManager;

    [SerializeField] private GameObject _winWindow;
    [SerializeField] private GameObject _menuWindow;
    private bool _isMenuPressed = false;

    private int _currentLevelIndex = 0;
    private bool _isWinTriggered = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadLevel(_currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Length)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        _currentLevelIndex = levelIndex;
        LevelData levelData = _levels[_currentLevelIndex];

        ClearLevel();

        ApplyGridSettings(levelData);
        SpawnCars(levelData);
        _gridManager.BuildObstacles();
        _winWindow.SetActive(false);

        for (int i = 0; i < _spawners.Length; i++)
        {
            ApplySpawner(levelData, i);
        }
    }

    private void SpawnCars(LevelData level)
    {
        foreach (var carData in level.Cars)
        {
            _gridManager.SpawnCarAt(carData.GridPosition, carData.UnitType, carData.Color);
        }
    }

    private void ApplySpawner(LevelData level, int spawnerIndex)
    {
        if (spawnerIndex < 0 || spawnerIndex >= _spawners.Length)
            return;

        if (spawnerIndex >= level.Spawners.Count)
        {
            Debug.LogWarning($"No data for spawner {spawnerIndex}");
            return;
        }

        _spawners[spawnerIndex].SetPeopleQueue(
            level.Spawners[spawnerIndex].People
        );

        _spawners[spawnerIndex].ResetSpawner();
    }


    private void ApplyGridSettings(LevelData level)
    {
        _gridManager.RebuildGrid();
    }

    public void ShowWinWindow()
    {
        _winWindow.SetActive(true);
    }

    public void ShowMenuWindow()
    {
        _isMenuPressed = !_isMenuPressed;
        _menuWindow.SetActive(_isMenuPressed);
    }

    public void LoadNextLevel()
    {
        _currentLevelIndex++;

        if (_currentLevelIndex >= _levels.Length)
        {
            Debug.Log("No more levels!");
            return;
        }

        LoadLevel(_currentLevelIndex); ;
    }

    public void LoadCurrentLevel()
    {
        LoadLevel(_currentLevelIndex);;
    }

    private void ClearLevel()
    {
        _isWinTriggered = false;

        _gridManager.ClearGrid();
        _roadManager.ClearCars();

        foreach (var spawner in _spawners)
        {
            spawner.ClearSpawner();
        }
    }
        public void CheckWinCondition()
    {
        if (_isWinTriggered) return;
        
        foreach (var spawner in _spawners)
        {
            if (!spawner.IsFinished())
                return;
        }
        
        if (_gridManager.HasActiveCars()) return;

        if (_roadManager.HasCars()) return;

        _isWinTriggered = true;
        
        ShowWinWindow();
    }
}
