using UnityEngine;
using Cysharp.Threading.Tasks;
using YG;

public class LevelConstructor : MonoBehaviour
{
    public static LevelConstructor Instance { get; private set; }

    [SerializeField] private Spawner[] _spawners;
    [SerializeField] private LevelData[] _levels;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private RoadManager _roadManager;

    [SerializeField] private GameObject _winWindow;
    [SerializeField] private GameObject _menuWindow;
    [SerializeField] private GameObject _levelsWindow;
    [SerializeField] private GameObject _reloadbuttonWindow;
    [SerializeField] private GameObject _menuButtonWindow;
    [SerializeField] private GameObject _mainMenuWindow;
    [SerializeField] private LeaderboardYG _leaderboardYG;
    [SerializeField] private StartTextController _startTextController;
    private bool _isMenuPressed = false;

    private int _currentLevelIndex = 0;
    private bool _isWinTriggered = false;

    private AdsManager _adsManager;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _levelsWindow.SetActive(false);
        _menuButtonWindow.SetActive(false);
        _reloadbuttonWindow.SetActive(false);
    }
    
    private void Start()
    {
        _adsManager = AdsManager.Instance;
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
        _levelsWindow.SetActive(false);
        _menuButtonWindow.SetActive(true);
        _reloadbuttonWindow.SetActive(true);


        ClearLevel();

        ApplyGridSettings(levelData);
        SpawnCars(levelData);
        _gridManager.BuildObstacles();
        _winWindow.SetActive(false);

        for (int i = 0; i < _spawners.Length; i++)
        {
            ApplySpawner(levelData, i);
        }
        _startTextController.ShowIfFirstLevel(levelIndex).Forget();
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
        _menuButtonWindow.SetActive(false);
        _reloadbuttonWindow.SetActive(false);
        _winWindow.SetActive(true);
    }

    public void ShowMenuWindow()
    {
        _isMenuPressed = !_isMenuPressed;
        _startTextController.HideText();
        SetPause(_isMenuPressed);
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
        _adsManager.RegisterAction(2);

        if (_adsManager.TryShowAd(() =>
        {
            LoadLevel(_currentLevelIndex);
        }))
            return;

        LoadLevel(_currentLevelIndex);
    }

    public void LoadCurrentLevel()
    {
        _adsManager.RegisterAction(1);

        if (_adsManager.TryShowAd(() =>
        {
            LoadLevel(_currentLevelIndex);
        }))
            return;

        LoadLevel(_currentLevelIndex);
    }

    public void BackToMainMenu()
    {
        _mainMenuWindow.SetActive(true);
        _levelsWindow.SetActive(false);
        _winWindow.SetActive(false);
        _menuWindow.SetActive(false);

        _menuButtonWindow.SetActive(false);
        _reloadbuttonWindow.SetActive(false);

        Time.timeScale = 1f;
        _isMenuPressed = false;
        _startTextController.HideText();

        ClearLevel();
    }

    private void OnLevelCompleted()
    {
        var data = SaveSystem.Load();

        int reward = _levels[_currentLevelIndex].ScoreReward;

        data.TotalScore += reward;

        if (data.TotalScore > data.BestScore)
        {
            data.BestScore = data.TotalScore;

            YG2.SetLeaderboard("LeaderBoardYG2", data.BestScore);

            if (_leaderboardYG != null)
                _leaderboardYG.UpdateLB();
        }

        if (_currentLevelIndex >= data.UnlockedLevel)
        {
            data.UnlockedLevel = _currentLevelIndex + 1;
        }

        SaveSystem.Save(data);
    }
    private void SetPause(bool isPaused)
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 1f : 0f;
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

        OnLevelCompleted();
        
        ShowWinWindow();
    }
}
