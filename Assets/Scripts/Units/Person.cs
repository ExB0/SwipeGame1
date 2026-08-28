using System.Threading;

using Cysharp.Threading.Tasks;
using UnityEngine;

using InterFaces;

namespace Units
{
    public class Person : MonoBehaviour, IColorMatchable, IQueueable
    {
        private static readonly int s_IsWalking = Animator.StringToHash("IsWalking");

        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private float _jumpDuration = 2f;
        [SerializeField] private float _minDistance = 0.01f;
        [SerializeField] private float _jumpArcMultiplier = 4f;
        [SerializeField] private float _fullProgress = 1f;
        [SerializeField] private int _hideAfterJumpDelay = 30;
        
        [SerializeField] private Color _color;
        
        [SerializeField] private Animator _animator;
        [SerializeField] private float _animatorSpeed;
        
        [SerializeField] private AudioSource _pickupSound;

        private CancellationTokenSource _cancellationTokenSource;

        public bool IsJumped { get; private set; }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                Debug.LogError($"{name}: Animator не найден!");
            }
            else
            {
                _animator.speed = _animatorSpeed;
            }

            MeshRenderer renderer = GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                _color = renderer.material.color;
            }
            else
            {
                Debug.LogError($"{name}: MeshRenderer не найден!");
            }

            _cancellationTokenSource =
                new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
        
        public Color GetColor() => _color;

        public async UniTask JumpTo(
            Vector3 target,
            Transform parentTransform)
        {
            if (parentTransform == null)
            {
                Debug.LogError(
                    $"{name}: Не задан родительский трансформ для посадки."
                );

                return;
            }

            if (target == transform.position)
            {
                Debug.LogWarning(
                    $"{name}: Целевая позиция совпадает с текущей. " +
                    "Прыжок не требуется."
                );

                return;
            }

            CancellationToken token =
                _cancellationTokenSource.Token;

            if (IsJumped || token.IsCancellationRequested)
            {
                return;
            }

            IsJumped = true;

            PlaySound();

            Vector3 start = transform.position;
            float time = 0f;

            while (time < _jumpDuration)
            {
                token.ThrowIfCancellationRequested();

                float progress = time / _jumpDuration;

                float height =
                    _jumpArcMultiplier *
                    _jumpHeight *
                    progress *
                    (_fullProgress - progress);

                Vector3 position =
                    Vector3.Lerp(
                        start,
                        target,
                        progress
                    ) +
                    Vector3.up * height;

                transform.position = position;

                await UniTask.Yield();

                time += Time.deltaTime;
            }

            transform.position = target;

            await UniTask.Delay(_hideAfterJumpDelay);

            gameObject.SetActive(false);
        }

        public async UniTask MoveToPosition(
            Vector3 target,
            float speed,
            CancellationToken token)
        {
            if (speed <= 0f)
            {
                Debug.LogError(
                    $"{name}: Скорость должна быть положительной. " +
                    $"Текущее значение: {speed}"
                );

                return;
            }

            if (Vector3.Distance(
                    transform.position,
                    target) <= _minDistance)
            {
                return;
            }

            using CancellationTokenSource linkedCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    this.GetCancellationTokenOnDestroy(),
                    token
                );

            CancellationToken linkedToken =
                linkedCancellationTokenSource.Token;

            if (_animator != null)
            {
                _animator.SetBool(s_IsWalking, true);
            }

            try
            {
                while (Vector3.Distance(
                           transform.position,
                           target) > _minDistance)
                {
                    linkedToken.ThrowIfCancellationRequested();

                    Vector3 direction =
                        (target - transform.position).normalized;

                    transform.forward = direction;

                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        target,
                        speed * Time.deltaTime
                    );

                    await UniTask.Yield(
                        PlayerLoopTiming.Update,
                        linkedToken
                    );
                }

                transform.position = target;
            }
            finally
            {
                if (_animator != null)
                {
                    _animator.SetBool(
                        s_IsWalking,
                        false
                    );
                }
            }
        }

        private void PlaySound()
        {
            if (_pickupSound != null)
            {
                _pickupSound.Play();
            }
        }
    }
}