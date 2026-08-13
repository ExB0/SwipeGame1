using System.Threading;

using UnityEngine;

using PlatformPuzzle.Managers;
using Buildings;
using Grid;

namespace PlatformPuzzle.Levels
{
    public class LevelConstructor : MonoBehaviour
    {
        public static LevelConstructor Instance { get; private set; }

        [Header("Level")]
        [SerializeField] private Spawner[] _spawners;
        [SerializeField] private LevelData[] _levels;
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private RoadManager _roadManager;

        [Header("Controllers")]
        [SerializeField] private LevelSession _levelSession;
        [SerializeField] private LevelFlowController _flowController;

        [Header("Services")]
        [SerializeField] private LevelTimer _levelTimer;

        private int _currentLevelIndex;

        public int CurrentLevelIndex => _currentLevelIndex;

        public int LevelsCount =>
            _levels != null ? _levels.Length : 0;

        public LevelData CurrentLevelData
        {
            get
            {
                if (_levels == null ||
                    _currentLevelIndex < 0 ||
                    _currentLevelIndex >= _levels.Length)
                {
                    return null;
                }

                return _levels[_currentLevelIndex];
            }
        }

        public CancellationToken LevelToken =>
            _levelSession != null
                ? _levelSession.Token
                : CancellationToken.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void LoadLevel(int levelIndex)
        {
            if (!TryGetLevelData(levelIndex, out LevelData levelData))
            {
                return;
            }

            Time.timeScale = 1f;

            CancelCurrentLevel();
            ClearLevel();
            CreateLevelSession();

            _currentLevelIndex = levelIndex;

            ApplyGridSettings();
            SpawnCars(levelData);

            if (_gridManager != null)
            {
                _gridManager.BuildObstacles();
            }

            ApplySpawners(levelData);

            _flowController?.OnLevelStarted(levelIndex);
        }

        public void LoadLevelWithTimerReset(int levelIndex)
        {
            if (!TryGetLevelData(levelIndex, out LevelData levelData))
            {
                return;
            }

            LoadLevel(levelIndex);
            StartLevelTimer(levelData);
        }

        public void LoadCurrentLevelWithTimerReset()
        {
            LoadLevelWithTimerReset(_currentLevelIndex);
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

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_levelTimer == null)
            {
                return;
            }

            if (wasUnlimited)
            {
                _levelTimer.DisableLimit();
                _levelTimer.ShowTimer();
            }
            else if (remainingTime > 0f)
            {
                _levelTimer.StartTimer(
                    remainingTime,
                    LevelToken
                );
            }
        }

        public void ClearLevel()
        {
            if (_gridManager != null)
            {
                _gridManager.ClearGrid();
            }

            if (_roadManager != null)
            {
                _roadManager.ClearCars();
            }

            if (_spawners == null)
            {
                return;
            }

            foreach (Spawner spawner in _spawners)
            {
                if (spawner == null)
                {
                    continue;
                }

                spawner.ClearSpawner();
            }
        }

        public void CancelCurrentLevel()
        {
            if (_levelSession != null)
            {
                _levelSession.Cancel();
            }
        }

        private void CreateLevelSession()
        {
            if (_levelSession != null)
            {
                _levelSession.Create();
            }
        }

        private void SpawnCars(LevelData level)
        {
            if (_gridManager == null)
            {
                Debug.LogError("LevelConstructor: GridManager is null");
                return;
            }

            foreach (CarSpawnData carData in level.Cars)
            {
                _gridManager.SpawnCarAt(
                    carData.GridPosition,
                    carData.UnitType,
                    carData.Color
                );
            }
        }

        private void ApplySpawners(LevelData level)
        {
            if (_spawners == null)
            {
                return;
            }

            for (int i = 0; i < _spawners.Length; i++)
            {
                ApplySpawner(level, i);
            }
        }

        private void ApplySpawner(LevelData level, int spawnerIndex)
        {
            if (spawnerIndex < 0 ||
                _spawners == null ||
                spawnerIndex >= _spawners.Length)
            {
                return;
            }

            Spawner spawner = _spawners[spawnerIndex];

            if (spawner == null)
            {
                return;
            }

            if (spawnerIndex >= level.Spawners.Count)
            {
                Debug.LogWarning(
                    $"No data for spawner {spawnerIndex}"
                );

                return;
            }

            spawner.Initialize(this, _flowController);

            spawner.SetPeopleQueue(
                level.Spawners[spawnerIndex].People
            );

            spawner.ResetSpawner();
        }

        private void ApplyGridSettings()
        {
            if (_gridManager != null)
            {
                _gridManager.RebuildGrid();
            }
        }

        private bool TryGetLevelData(int levelIndex, out LevelData levelData)
        {
            levelData = null;

            if (_levels == null || _levels.Length == 0)
            {
                Debug.LogError("Levels array is empty");
                return false;
            }

            if (levelIndex < 0 || levelIndex >= _levels.Length)
            {
                Debug.LogError("Invalid level index!");
                return false;
            }

            levelData = _levels[levelIndex];

            return ValidateLevelData(levelData);
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

            if (_gridManager == null)
            {
                Debug.LogError("GridManager is null");
                return false;
            }

            foreach (CarSpawnData car in level.Cars)
            {
                if (!_gridManager.IsCellExists(car.GridPosition))
                {
                    Debug.LogError(
                        $"Car position {car.GridPosition} is outside grid"
                    );

                    return false;
                }

                Cell cell = _gridManager.GetCell(car.GridPosition);

                if (cell == null)
                {
                    Debug.LogError(
                        $"Cell at {car.GridPosition} is null"
                    );

                    return false;
                }
            }

            if (_spawners == null)
            {
                Debug.LogError("Spawners array is null");
                return false;
            }

            if (level.Spawners.Count > _spawners.Length)
            {
                Debug.LogError(
                    $"Level has {level.Spawners.Count} spawners, " +
                    $"but scene has only {_spawners.Length}"
                );

                return false;
            }

            return true;
        }

        private void StartLevelTimer(LevelData levelData)
        {
            if (_levelTimer == null)
            {
                return;
            }

            _levelTimer.StartTimer(
                levelData.TimeLimitSeconds,
                LevelToken
            );
        }

        private void OnDestroy()
        {
            CancelCurrentLevel();
        }
    }
}
