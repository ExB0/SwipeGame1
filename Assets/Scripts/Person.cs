using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Person : MonoBehaviour, IColorMatchable, IJumpable, IQueueable
{
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _jumpDuration =2f;
    
    [SerializeField] private Color _color;

    [SerializeField] private Animator _animator;
    [SerializeField] private float _animatorSpeed;

    public bool IsJumped { get; private set; }
    public Color GetColor() => _color;

    private CancellationTokenSource _cancellationTokenSource;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
             Debug.LogError($"{name}: Animator не найден!");

        _animator.speed = _animatorSpeed;


        _color = GetComponent<MeshRenderer>().material.color;

        _cancellationTokenSource = new CancellationTokenSource();

    }

    public async UniTask JumpTo(Vector3 target, Transform parentTransform)
    {
        if (parentTransform == null)
        {
            Debug.LogError($"{name}: Не задан родительский трансформ для посадки.");
            return;
        }

        if (target == transform.position)
        {
            Debug.LogWarning($"{name}: Целевая позиция совпадает с текущей. Прыжок не требуется.");
            return;
        }

        var token = _cancellationTokenSource.Token;

        if (IsJumped || token.IsCancellationRequested) return;

        IsJumped = true;


        Vector3 start = transform.position;
        float time = 0f;

        while (time < _jumpDuration)
        {
            float t = time / _jumpDuration;

            float height = 4 * _jumpHeight * t * (1 - t);
            Vector3 pos = Vector3.Lerp(start, target, t) + Vector3.up * height;
            transform.position = pos;

            await UniTask.Yield();
            time += Time.deltaTime;
        }

        transform.position = target;
        gameObject.SetActive(false);
    }

    public async UniTask MoveToPosition(Vector3 target, float speed)
    {
        if (speed <= 0f)
        {
            Debug.LogError($"{name}: Скорость должна быть положительной. Текущее значение: {speed}");
            return;
        }

        if (target == transform.position) return;

        var token = _cancellationTokenSource.Token;

        _animator.SetBool(IsWalking, true);

        try
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                if (this == null || gameObject == null || token.IsCancellationRequested)
                    return;

                Vector3 direction = (target - transform.position).normalized;
                transform.forward = direction;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            transform.position = target;
        }
        finally
        {
            if (_animator != null)
                _animator.SetBool(IsWalking, false);
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}