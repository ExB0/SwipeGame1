using System;
using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

using GameStrategies;
using InterFaces;
using PlatformPuzzle.Levels;
using Units;

namespace Buildings
{
    public class Spawner : MonoBehaviour
    {
        private readonly SemaphoreSlim _queueLock = new(1, 1);

        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private UnitQueue _unitQueue;
        [SerializeField] private TextMeshProUGUI _remainingPool;
        [SerializeField] private MonoBehaviour _factorySource;
        [SerializeField] private int _spawnDelayMs = 50;
        [SerializeField] private LevelConstructor _levelConstructor;
        [SerializeField] private LevelFlowController _flowController;

        private bool _isActive;
        private IUnitFactory _unitFactory;

        private List<PersonSpawnData> _peopleQueueData = new();
        private int _currentPersonIndex;
        private CancellationTokenSource _spawnerCts;
        private TakeValidator _takeValidator;

        private void Awake()
        {
            _unitFactory = _factorySource as IUnitFactory;

            if (_unitFactory == null)
            {
                Debug.LogError($"{nameof(Spawner)}: FactorySource does not implement IUnitFactory");
            }

            _takeValidator = new TakeValidator();
        }

        private void Start()
        {
            if (_levelConstructor == null)
            {
                Debug.LogWarning(
                    $"{nameof(Spawner)}: LevelConstructor was not initialized yet"
                );
            }

            if (_flowController == null)
            {
                Debug.LogWarning(
                    $"{nameof(Spawner)}: LevelFlowController was not initialized yet"
                );
            }
        }

        private async void OnTriggerStay(Collider other)
        {
            if (!_isActive)
            {
                return;
            }

            Car car = other.GetComponent<Car>();

            if (car == null)
            {
                return;
            }

            CancellationToken token =
                _spawnerCts?.Token ?? CancellationToken.None;

            try
            {
                await TryProcessCar(car, token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnDestroy()
        {
            CancelToken();
            _queueLock?.Dispose();
        }

        public void Initialize(
            LevelConstructor levelConstructor,
            LevelFlowController flowController)
        {
            _levelConstructor = levelConstructor;
            _flowController = flowController;
        }

        public void SetPeopleQueue(List<PersonSpawnData> people)
        {
            _peopleQueueData = people != null
                ? new List<PersonSpawnData>(people)
                : new List<PersonSpawnData>();

            _currentPersonIndex = 0;

            UpdateRemainingText();
        }

        public void ResetSpawner()
        {
            CancelToken();;
            CreateToken();

            ReleaseSemaphoreSafely();

            _isActive = true;

            FillQueueAsync(_spawnerCts.Token).Forget();
        }

        public void ClearSpawner()
        {
            CancelToken();
            ReleaseSemaphoreSafely();

            _isActive = false;

            _unitQueue?.ClearAndDestroy();

            _currentPersonIndex = 0;

            UpdateRemainingText();
        }

        public bool IsFinished()
        {
            return _currentPersonIndex >= _peopleQueueData.Count &&
                   _unitQueue != null &&
                   _unitQueue.Count == 0;
        }

        private async UniTaskVoid FillQueueAsync(CancellationToken token)
        {
            bool lockAcquired = false;

            try
            {
                await _queueLock.WaitAsync(token);

                lockAcquired = true;
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                if (_unitQueue == null)
                {
                    return;
                }

                for (int i = 0; i < _unitQueue.Capacity; i++)
                {
                    token.ThrowIfCancellationRequested();

                    if (_currentPersonIndex >= _peopleQueueData.Count)
                    {
                        break;
                    }

                    await EnqueueNextInternal(token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (lockAcquired)
                {
                    ReleaseSemaphoreSafely();
                }
            }
        }

        private async UniTask TryProcessCar(
            Car car,
            CancellationToken token)
        {
            if (!await _queueLock.WaitAsync(0, token))
            {
                return;
            }

            try
            {
                token.ThrowIfCancellationRequested();

                if (_unitQueue == null)
                {
                    return;
                }

                IQueueable personQueueable = _unitQueue.Peek();

                if (personQueueable == null)
                {
                    return;
                }

                Person person = (personQueueable as MonoBehaviour)
                    ?.GetComponent<Person>();

                if (person == null)
                {
                    return;
                }

                if (person.IsJumped)
                {
                    return;
                }

                if (!_takeValidator.TryTake(car, person))
                {
                    return;
                }

                await _unitQueue.Dequeue(token);

                if (_currentPersonIndex < _peopleQueueData.Count)
                {
                    await EnqueueNextInternal(token);
                }

                _flowController?.CheckWinCondition();
            }
            finally
            {
                ReleaseSemaphoreSafely();
            }
        }

        private async UniTask EnqueueNextInternal(CancellationToken token)
        {
            if (_currentPersonIndex >= _peopleQueueData.Count)
            {
                return;
            }

            await UniTask.Delay(
                _spawnDelayMs,
                DelayType.DeltaTime,
                PlayerLoopTiming.Update,
                token
            );

            PersonSpawnData personData =
                _peopleQueueData[_currentPersonIndex];

            GameObject obj = _unitFactory.Create(
                personData.UnitType,
                personData.Color,
                _spawnPoint.position
            );

            if (obj == null)
            {
                return;
            }

            await _unitQueue.Enqueue(obj, token);

            _currentPersonIndex++;

            UpdateRemainingText();
        }

        private void UpdateRemainingText()
        {
            if (_remainingPool == null)
            {
                return;
            }

            int remainingCount = GetRemainingCount();

            _remainingPool.text = remainingCount.ToString();
        }

        private int GetRemainingCount()
        {
            return Mathf.Max(
                0,
                _peopleQueueData.Count - _currentPersonIndex
            );
        }

        private void CreateToken()
        {
            _spawnerCts?.Cancel();
            _spawnerCts?.Dispose();

            CancellationToken levelToken = _levelConstructor != null
                ? _levelConstructor.LevelToken
                : CancellationToken.None;

            _spawnerCts = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy(),
                levelToken
            );
        }

        private void CancelToken()
        {
            _spawnerCts?.Cancel();
            _spawnerCts?.Dispose();

            _spawnerCts = null;
        }

        private void ReleaseSemaphoreSafely()
        {
            try
            {
                if (_queueLock.CurrentCount == 0)
                {
                    _queueLock.Release();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SemaphoreFullException)
            {
            }
        }
    }
}