using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UnitQueue : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 20f;
    [SerializeField] private Transform[] _queuePositions;

    private readonly Queue<IQueueable> _queue = new();

    public int Count => _queue.Count;
    public int Capacity => _queuePositions.Length;

    public async UniTask Enqueue(GameObject obj)
    {
        obj.SetActive(true);

        var queueable = obj.GetComponent<IQueueable>();
        if (queueable == null)
        {
            Debug.LogError("Объект не реализует IQueueable");
            obj.SetActive(false);
            return;
        }

        _queue.Enqueue(queueable);
        int targetIndex = _queue.Count - 1;
        await queueable.MoveToPosition(_queuePositions[targetIndex].position, _moveSpeed);
    }

    public async UniTask Dequeue()
    {
        if (_queue.Count == 0) return;

        var unit = _queue.Dequeue();
        var go = (unit as MonoBehaviour)?.gameObject;

        int i = 0;
        var tasks = new List<UniTask>();
        foreach (var q in _queue)
        {
            if (q != null && (q as MonoBehaviour) != null)
                tasks.Add(q.MoveToPosition(_queuePositions[i].position, _moveSpeed));
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

}
