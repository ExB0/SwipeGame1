using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;
using UnityEngine;

using InterFaces;

namespace Units
{
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
            {
                Debug.LogError($"{name}: queue positions are empty");
            }
        }

        public async UniTask Enqueue(GameObject unitObject, CancellationToken token)
        {
            if (_queue.Count >= _queuePositions.Length)
            {
                Debug.LogWarning($"{name}: Queue is full");
                Destroy(unitObject);
                return;
            }

            unitObject.SetActive(true);

            IQueueable queueable = unitObject.GetComponent<IQueueable>();

            if (queueable == null)
            {
                Debug.LogError("Объект не реализует IQueueable");
                Destroy(unitObject);
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
            if (_queue.Count == 0)
            {
                return;
            }

            _queue.Dequeue();

            int i = 0;
            List<UniTask> tasks = new();

            foreach (IQueueable queueable in _queue)
            {
                token.ThrowIfCancellationRequested();

                if (i >= _queuePositions.Length)
                {
                    break;
                }

                MonoBehaviour monoBehaviour = queueable as MonoBehaviour;

                if (queueable != null &&
                    monoBehaviour != null &&
                    monoBehaviour.gameObject.activeInHierarchy)
                {
                    tasks.Add(
                        queueable.MoveToPosition(
                            _queuePositions[i].position,
                            _moveSpeed,
                            token
                        )
                    );
                }

                i++;
            }

            await UniTask.WhenAll(tasks);
        }

        public IQueueable Peek()
        {
            return _queue.Count > 0
                ? _queue.Peek()
                : null;
        }

        public void ClearAndDestroy()
        {
            foreach (IQueueable unit in _queue)
            {
                GameObject go = (unit as MonoBehaviour)?.gameObject;

                if (go != null)
                {
                    Destroy(go);
                }
            }

            _queue.Clear();
        }
    }
}