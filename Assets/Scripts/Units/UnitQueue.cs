using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;
using UnityEngine;

using InterFaces;

namespace Units
{
    public class UnitQueue : MonoBehaviour
    {
        private readonly Queue<IQueueable> _queue = new();

        [SerializeField] private float _moveSpeed = 20f;
        [SerializeField] private Transform[] _queuePositions;

        public int Count => _queue.Count;

        public int Capacity
        {
            get
            {
                if (_queuePositions == null) return 0;
                return _queuePositions.Length;
            }
        }

        private void Awake()
        {
            if (_queuePositions == null || _queuePositions.Length == 0)
            {
                Debug.LogError($"{name}: queue positions are empty or null");
            }
        }

        public async UniTask Enqueue(GameObject unitObject, CancellationToken token)
        {
            if (_queuePositions == null || _queuePositions.Length == 0)
            {
                Debug.LogError($"{name}: queue positions not set");
                Destroy(unitObject);
                return;
            }

            if (_queue.Count >= _queuePositions.Length)
            {
                Debug.LogWarning($"{name}: Queue is full");
                Destroy(unitObject);
                return;
            }

            if (unitObject == null)
            {
                Debug.LogError($"{name}: unitObject is null");
                return;
            }

            int targetIndex = _queue.Count;
            if (targetIndex < 0 || targetIndex >= _queuePositions.Length || _queuePositions[targetIndex] == null)
            {
                Debug.LogError($"{name}: invalid position at index {targetIndex}");
                Destroy(unitObject);
                return;
            }

            unitObject.SetActive(true);

            IQueueable queueable = unitObject.GetComponent<IQueueable>();
            if (queueable == null)
            {
                Debug.LogError("UnitQueue: object does not implement IQueueable");
                Destroy(unitObject);
                return;
            }

            _queue.Enqueue(queueable);
            await queueable.MoveToPosition(_queuePositions[targetIndex].position, _moveSpeed, token);
        }

        public async UniTask Dequeue(CancellationToken token)
        {
            if (_queue.Count == 0)
                return;

            if (_queuePositions == null || _queuePositions.Length == 0)
            {
                Debug.LogError($"{name}: queue positions not set, cannot dequeue");
                return;
            }

            _queue.Dequeue();

            int i = 0;
            List<UniTask> tasks = new();

            foreach (IQueueable queueable in _queue)
            {
                token.ThrowIfCancellationRequested();

                if (i >= _queuePositions.Length)
                    break;

                if (_queuePositions[i] == null)
                {
                    Debug.LogWarning($"{name}: position at index {i} is null, skipping");
                    i++;
                    continue;
                }

                MonoBehaviour monoBehaviour = queueable as MonoBehaviour;
                if (queueable != null && monoBehaviour != null && monoBehaviour.gameObject.activeInHierarchy)
                {
                    tasks.Add(queueable.MoveToPosition(_queuePositions[i].position, _moveSpeed, token));
                }

                i++;
            }

            await UniTask.WhenAll(tasks);
        }

        public IQueueable Peek()
        {
            return _queue.Count > 0 ? _queue.Peek() : null;
        }

        public void ClearAndDestroy()
        {
            foreach (IQueueable unit in _queue)
            {
                GameObject go = (unit as MonoBehaviour)?.gameObject;
                if (go != null)
                    Destroy(go);
            }
            _queue.Clear();
        }
    }
}