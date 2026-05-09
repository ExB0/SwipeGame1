using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

public class UnitQueue : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 20f;
    [SerializeField] private Transform[] _queuePositions;

    private readonly Queue<IQueueable> _queue = new();

    public int Count => _queue.Count;
    public int Capacity => _queuePositions.Length;
    private void Awake()
    {
        if (_queuePositions == null || _queuePositions.Length == 0)
            Debug.LogError($"{name}: queue positions are empty");
    }


    public async UniTask Enqueue(GameObject obj, CancellationToken token)
    {
        if (_queue.Count >= _queuePositions.Length)
        {
            Debug.LogWarning($"{name}: Queue is full");
            Destroy(obj);
            return;
        }

        obj.SetActive(true);

        var queueable = obj.GetComponent<IQueueable>();
        if (queueable == null)
        {
            Debug.LogError("Объект не реализует IQueueable");
            Destroy(obj);
            return;
        }

        _queue.Enqueue(queueable);

        int targetIndex = _queue.Count - 1;

        await queueable.MoveToPosition(
            _queuePositions[targetIndex].position,
            _moveSpeed,
            token
        );
    }

    public async UniTask Dequeue(CancellationToken token)
    {
        if (_queue.Count == 0) return;

        _queue.Dequeue();

        int i = 0;
        var tasks = new List<UniTask>();

        foreach (var q in _queue)
        {
            token.ThrowIfCancellationRequested();

            if (i >= _queuePositions.Length)
                break;

            var mb = q as MonoBehaviour;

            if (q != null && mb != null && mb.gameObject.activeInHierarchy)
                tasks.Add(q.MoveToPosition(_queuePositions[i].position, _moveSpeed, token));

            i++;
        }

        await UniTask.WhenAll(tasks);
    }


    public IQueueable Peek()
    {
        return _queue.Count > 0 ? _queue.Peek() : null;
    }
    public void ClearImmediate()
    {
        foreach (var unit in _queue)
        {
            var go = (unit as MonoBehaviour)?.gameObject;
            if (go != null)
                go.SetActive(false);
        }

        _queue.Clear();
    }
    public void ClearAndDestroy()
    {
        foreach (var unit in _queue)
        {
            var go = (unit as MonoBehaviour)?.gameObject;
            if (go != null)
                Destroy(go);
        }

        _queue.Clear();
    }
}
