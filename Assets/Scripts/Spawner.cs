using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Threading;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private UnitQueue _unitQueue;
    [SerializeField] private TextMeshProUGUI _remainingPool;
    [SerializeField] private MonoBehaviour _factorySource;
    [SerializeField] private int _spawnDelayMs = 50;

    private readonly SemaphoreSlim _queueLock = new SemaphoreSlim(1, 1);

    private bool _isActive;
    private LevelConstructor _levelConstructor;
    private IUnitFactory _unitFactory;
    private List<PersonSpawnData> _peopleQueueData = new();
    private int _currentPersonIndex;
    private CancellationTokenSource _spawnerCts;
    private ColorMatchStrategy _takeStrategy;

    private void Awake()
    {
        _unitFactory = _factorySource as IUnitFactory;

        if (_unitFactory == null)
            Debug.LogError("Spawner: _factorySource does not implement IUnitFactory");
    }

    private void Start()
    {
        _levelConstructor = LevelConstructor.Instance;
        _takeStrategy = ColorMatchStrategy.Instance;
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
        CancelToken();
        CreateToken();

        _isActive = true;
        FillQueueAsync(_spawnerCts.Token).Forget();
    }

    private async UniTaskVoid FillQueueAsync(CancellationToken token)
    {
        await _queueLock.WaitAsync(token);

        try
        {
            for (int i = 0; i < _unitQueue.Capacity; i++)
            {
                token.ThrowIfCancellationRequested();

                if (_currentPersonIndex >= _peopleQueueData.Count)
                    break;

                await EnqueueNextInternal(token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _queueLock.Release();
        }
    }

    private async void OnTriggerStay(Collider other)
    {
        if (!_isActive) return;

        var car = other.GetComponent<Car>();
        if (car == null) return;

        var token = _spawnerCts?.Token ?? CancellationToken.None;

        try
        {
            await TryProcessCar(car, token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask TryProcessCar(Car car, CancellationToken token)
    {
        if (!await _queueLock.WaitAsync(0, token))
            return;

        try
        {
            token.ThrowIfCancellationRequested();

            var personQueueable = _unitQueue.Peek();
            if (personQueueable == null) return;

            var person = (personQueueable as MonoBehaviour)?.GetComponent<Person>();
            if (person == null) return;

            if (person.IsJumped) return;

            var context = new TakeContext(car, person, person, car);

            if (!_takeStrategy.TryTake(context))
                return;

            await _unitQueue.Dequeue(token);

            if (_currentPersonIndex < _peopleQueueData.Count)
                await EnqueueNextInternal(token);

            _levelConstructor.CheckWinCondition();
        }
        finally
        {
            _queueLock.Release();
        }
    }

    private async UniTask EnqueueNextInternal(CancellationToken token)
    {
        if (_currentPersonIndex >= _peopleQueueData.Count)
            return;

        await UniTask.Delay(_spawnDelayMs, DelayType.DeltaTime, PlayerLoopTiming.Update, token);

        var personData = _peopleQueueData[_currentPersonIndex];

        GameObject obj = _unitFactory.Create(
            personData.UnitType,
            personData.Color,
            _spawnPoint.position
        );

        if (obj == null) return;

        await _unitQueue.Enqueue(obj, token);

        _currentPersonIndex++;
        UpdateRemainingText();
    }

    public async UniTask ClearSpawnerAsync()
    {
        CancelToken();

        await _queueLock.WaitAsync();

        try
        {
            _isActive = false;
            _unitQueue.ClearAndDestroy();
            _currentPersonIndex = 0;
            UpdateRemainingText();
        }
        finally
        {
            _queueLock.Release();
        }
    }

    public void ClearSpawner()
    {
        CancelToken();

        _isActive = false;
        _unitQueue.ClearAndDestroy();

        _currentPersonIndex = 0;

        UpdateRemainingText();
    }

    public bool IsFinished()
    {
        return _currentPersonIndex >= _peopleQueueData.Count
            && _unitQueue.Count == 0;
    }

    private void UpdateRemainingText()
    {
        if (_remainingPool != null)
            _remainingPool.text = Mathf.Max(0, _peopleQueueData.Count - _currentPersonIndex).ToString();
    }

    private void CreateToken()
    {
        _spawnerCts?.Cancel();
        _spawnerCts?.Dispose();

        var levelToken = LevelConstructor.Instance != null
            ? LevelConstructor.Instance.LevelToken
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

    private void OnDestroy()
    {
        CancelToken();
    }
}
