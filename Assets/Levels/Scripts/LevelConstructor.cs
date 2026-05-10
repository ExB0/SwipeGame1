using UnityEngine;
using Cysharp.Threading.Tasks;
using YG;
using System.Threading;

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
    [SerializeField] private LevelTimer _levelTimer;
    [SerializeField] private GameObject _loseWindow;
    [SerializeField] private AdsManager _adsManager;
    private bool _isMenuPressed = false;
    private bool _isLoseTriggered;

    private int _currentLevelIndex = 0;
    private bool _isWinTriggered = false;
    private CancellationTokenSource _levelCts;


    public CancellationToken LevelToken => _levelCts?.Token ?? CancellationToken.None;



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
        _loseWindow.SetActive(false);
    }

    private void Start()
    {
        _adsManager = AdsManager.Instance;
        if (_levelTimer != null)
            _levelTimer.OnTimeExpired += OnTimeExpired;
    }


    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Length)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        Time.timeScale = 1f;
        _isMenuPressed = false;

        CancelLevelToken();
        ClearLevel();
        CreateLevelToken();

        _currentLevelIndex = levelIndex;
        LevelData levelData = _levels[_currentLevelIndex];

        if (!ValidateLevelData(levelData))
            return;

        if (_loseWindow != null)
            _loseWindow.SetActive(false);

        if (_menuWindow != null)
            _menuWindow.SetActive(false);

        _levelsWindow.SetActive(false);
        _menuButtonWindow.SetActive(true);
        _reloadbuttonWindow.SetActive(true);
        _winWindow.SetActive(false);

        ApplyGridSettings(levelData);
        SpawnCars(levelData);
        _gridManager.BuildObstacles();

        for (int i = 0; i < _spawners.Length; i++)
        {
            ApplySpawner(levelData, i);
        }

        if (_levelTimer != null)
            _levelTimer.ShowTimer();

        _startTextController.ShowIfFirstLevel(levelIndex).Forget();
    }
    public void RestartCurrentLevelKeepTimer()
    {
        float remainingTime = 0f;
        bool wasUnlimited = false;

        if (_levelTimer != null)
        {
            remainingTime = _levelTimer.RemainingTime;
            wasUnlimited = _levelTimer.IsUnlimited;
        }

        LoadLevel(_currentLevelIndex);

        if (_levelTimer == null)
            return;

        if (wasUnlimited)
        {
            _levelTimer.DisableLimit();
            _levelTimer.ShowTimer();
        }
        else if (remainingTime > 0f)
        {
            _levelTimer.StartTimer(remainingTime, LevelToken);
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
        _levelTimer?.HideTimer();

        _menuButtonWindow.SetActive(false);
        _reloadbuttonWindow.SetActive(false);
        _winWindow.SetActive(true);
    }

    public void ShowMenuWindow()
    {
        if (_isLoseTriggered)
            return;

        _isMenuPressed = !_isMenuPressed;
        _startTextController.HideText();
        SetPause(_isMenuPressed);
        _menuWindow.SetActive(_isMenuPressed);

        if (_levelTimer != null)
        {
            if (_isMenuPressed)
                _levelTimer.HideTimer();
            else
                _levelTimer.ShowTimer();
        }
    }

    public void LoadNextLevel()
    {
        int nextLevelIndex = _currentLevelIndex + 1;

        if (nextLevelIndex >= _levels.Length)
        {
            Debug.Log("No more levels!");
            return;
        }

        _adsManager.RegisterAction(2);

        if (_adsManager.TryShowAd(() =>
        {
            LoadLevelWithTimerReset(nextLevelIndex);
        }))
            return;

        LoadLevelWithTimerReset(nextLevelIndex);
    }


    public void LoadCurrentLevel()
    {
        _adsManager.RegisterAction(1);

        if (_adsManager.TryShowAd(() =>
        {
            RestartCurrentLevelKeepTimer();
        }))
            return;

        RestartCurrentLevelKeepTimer();
    }

    public void BackToMainMenu()
    {
        CancelLevelToken();

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
    public void CheckWinCondition()
    {
        if (_isWinTriggered || _isLoseTriggered) return;

        foreach (var spawner in _spawners)
        {
            if (!spawner.IsFinished())
                return;
        }

        if (_gridManager.HasActiveCars()) return;

        if (_roadManager.HasCars()) return;

        _isWinTriggered = true;

        if (_levelTimer != null)
            _levelTimer?.StopAndHide();

        OnLevelCompleted();

        ShowWinWindow();
    }
    public void ContinueAfterRewardAd()
    {
        if (!_isLoseTriggered)
            return;

        YG2.RewardedAdvShow("SecondChance", ApplySecondChance);
    }
    public void RestartAfterLose()
    {
        LoadLevelWithTimerReset(_currentLevelIndex);
    }
    public void LoadLevelWithTimerReset(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Length)
        {
            Debug.LogError("Invalid level index!");
            return;
        }

        LoadLevel(levelIndex);

        LevelData levelData = _levels[levelIndex];
        StartLevelTimer(levelData);
    }

    private void OnLevelCompleted()
    {
        var data = SaveSystem.Load();

        int reward = _levels[_currentLevelIndex].ScoreReward;

        data.TotalScore += reward;

        if (data.TotalScore > data.BestScore)
        {
            data.BestScore = data.TotalScore;

            YG2.SetLeaderboard("LeaderBoardYG", data.BestScore);

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
        Time.timeScale = isPaused ? 0f : 1f;    
    }

    private void ClearLevel()
    {
        _isWinTriggered = false;
        _isLoseTriggered = false;

        _gridManager.ClearGrid();
        _roadManager.ClearCars();

        foreach (var spawner in _spawners)
        {
            spawner.ClearSpawner();
        }
    }
    private void CreateLevelToken()
    {
        _levelCts?.Cancel();
        _levelCts?.Dispose();
        _levelCts = new CancellationTokenSource();
    }
    private void CancelLevelToken()
    {
        _levelCts?.Cancel();
        _levelCts?.Dispose();
        _levelCts = null;
    }
    private bool ValidateLevelData(LevelData level)
    {
        if (level == null)
        {
            Debug.LogError("LevelData is null");
            return false;
        }

        if (level.Cars == null)
        {
            Debug.LogError("LevelData.Cars is null");
            return false;
        }

        if (level.Spawners == null)
        {
            Debug.LogError("LevelData.Spawners is null");
            return false;
        }

        foreach (var car in level.Cars)
        {
            if (!_gridManager.IsCellExists(car.GridPosition))
            {
                Debug.LogError($"Car position {car.GridPosition} is outside grid");
                return false;
            }

            var cell = _gridManager.GetCell(car.GridPosition);
            if (cell == null)
            {
                Debug.LogError($"Cell at {car.GridPosition} is null");
                return false;
            }

            if (_gridManager.GetExitCells().Contains(cell))
            {
                Debug.LogWarning($"Car spawned on exit cell: {car.GridPosition}");
            }
        }

        if (level.Spawners.Count > _spawners.Length)
        {
            Debug.LogError($"Level has {level.Spawners.Count} spawners, but scene has only {_spawners.Length}");
            return false;
        }

        return true;
    }
    private void OnTimeExpired()
    {
        if (_isWinTriggered || _isLoseTriggered)
            return;

        _isLoseTriggered = true;

        _levelTimer?.StopAndHide();

        Time.timeScale = 0f;

        _menuButtonWindow.SetActive(false);
        _reloadbuttonWindow.SetActive(false);

        if (_loseWindow != null)
            _loseWindow.SetActive(true);
    }
    private async void ApplySecondChance()
    {
        _isLoseTriggered = false;

        if (_loseWindow != null)
            _loseWindow.SetActive(false);

        _menuButtonWindow.SetActive(true);
        _reloadbuttonWindow.SetActive(true);

        await UniTask.Yield();

        Time.timeScale = 1f;        

        if (_levelTimer != null)
        {
            _levelTimer.DisableLimit();
            _levelTimer.ShowTimer();
        }
    }
    private void StartLevelTimer(LevelData levelData)
    {
        if (_levelTimer == null) return;

        _levelTimer.StartTimer(levelData.TimeLimitSeconds, LevelToken);
    }

    private void OnDestroy()
    {
        if (_levelTimer != null)
            _levelTimer.OnTimeExpired -= OnTimeExpired;

        CancelLevelToken();
    }
}
