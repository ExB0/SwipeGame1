using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private UnitQueue _unitQueue;
    [SerializeField] private TextMeshProUGUI _remainingPool;
    [SerializeField] private MonoBehaviour _factorySource;
    [SerializeField] private int _spawnDelayMs = 50;

    private bool _isProcessing = false;
    private LevelConstructor _levelConstructor;
    private IUnitFactory _unitFactory;
    private List<PersonSpawnData> _peopleQueueData = new();
    private int _currentPersonIndex;

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
        _peopleQueueData = new List<PersonSpawnData>(people);
        _currentPersonIndex = 0;
        UpdateRemainingText();
    }

    public void ResetSpawner()
    {
        _currentPersonIndex = 0;
        FillQueueAsync().Forget();
    }

    private async UniTaskVoid FillQueueAsync()
    {
        _isProcessing = true;
        for (int i = 0; i < _unitQueue.Capacity; i++)
        {
            await EnqueueNext();
        }
        _isProcessing = false;
    }

    public async UniTask EnqueueNext()
    {
        if (_currentPersonIndex >= _peopleQueueData.Count)
            return;

        await UniTask.Delay(_spawnDelayMs, DelayType.DeltaTime);

        var personData = _peopleQueueData[_currentPersonIndex];

        GameObject obj = _unitFactory.Create(
            personData.UnitType,
            personData.Color,
            _spawnPoint.position
        );

        if (obj == null) return;

        await _unitQueue.Enqueue(obj);

        _currentPersonIndex++;
        UpdateRemainingText();
    }

    private async void OnTriggerStay(Collider other)
    {
        if (_isProcessing) return;

        var car = other.GetComponent<Car>();
        if (car == null) return;

        var personQueueable = _unitQueue.Peek();
        if (personQueueable == null) return;

        var person = (personQueueable as MonoBehaviour)?.GetComponent<Person>();
        if (person == null) return;


        var context = new TakeContext(car, person, person, car);

        if (_takeStrategy.TryTake(context))
        {
            _isProcessing = true;
            await _unitQueue.Dequeue();

            if (_currentPersonIndex < _peopleQueueData.Count)
            {
                await EnqueueNext();
            }

            _levelConstructor.CheckWinCondition();

            _isProcessing = false;
        }
    }

    public void ClearSpawner()
    {
        while (_unitQueue.Peek() != null)
        {
            var queueable = _unitQueue.Peek();
            _unitQueue.Dequeue().Forget();;

            var mono = queueable as MonoBehaviour;
            if (mono != null)
            {
                Destroy(mono.gameObject);
            }
        }

        _currentPersonIndex = 0;
    }
    public bool IsFinished()
    {
        return _currentPersonIndex >= _peopleQueueData.Count
            && _unitQueue.Count == 0;
    }
    private void UpdateRemainingText()
    {
        if (_remainingPool != null)
            _remainingPool.text = (_peopleQueueData.Count - _currentPersonIndex).ToString();
    }
    

}