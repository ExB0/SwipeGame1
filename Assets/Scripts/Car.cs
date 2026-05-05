using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour, IColorMatchable
{
    [SerializeField] private Color _color;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private SplineAnimate _splineAnimator;
    [SerializeField] private int _seats = 2;
    [SerializeField] private float _leaveDistance = 20f;
    [SerializeField] private GameObject[] _passengers;
    [SerializeField] private ParticleSystem _smoke;

    public Color GetColor() => _color;

    private LevelConstructor _levelConstructor;
    private Transform _roadPoint;
    private Rigidbody _rigidbody;
    private ScaleShakeEffect _scaleShakeEffect;
    private PathFinder _pathFinder;
    private bool _isMoving = false;
    private float _reachedDistance = 0.7f;
    private GridManager _gridManager;
    private RoadManager _roadManager;
    private bool _leaving = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _pathFinder = new PathFinder();
        _splineAnimator = GetComponent<SplineAnimate>();
        _color = GetComponent<MeshRenderer>().material.color;
        _scaleShakeEffect = GetComponent<ScaleShakeEffect>();
    }

    private void Start()
    {
        _gridManager = GridManager.Instance;
        _roadManager = FindAnyObjectByType<RoadManager>();
        _levelConstructor = LevelConstructor.Instance;
        _smoke.Stop();
    }

    public void OnClick()
    {
        if (_scaleShakeEffect.IsShaking) return;

        if (_roadManager.IsRoadFull())
        {
            _scaleShakeEffect.Shake();
            return;
        } 

        if (_isMoving) return;
        
        UniTask.Void(async () => await HandleClick());
    }

    private async UniTask HandleClick()
    {
        Cell currentCell = GetCurrentCell();

        if (currentCell == null) return;

        if (_gridManager.GetExitCells().Contains(currentCell))
        {
            _isMoving = true;
            Debug.Log($"{currentCell.GridPosition}");
            currentCell.TryClearCar();
            await MoveToPosition(transform.position + Vector3.forward * 1f);

            await MoveToPosition(_roadPoint.position);
            await PlaySplineAnimator();
            return;
        }

        Vector2Int startPos = currentCell.GridPosition;
        List<Cell> exitCells = _gridManager.GetExitCells();

        if (exitCells.Count == 0)
        {
            Debug.LogWarning("Нет выхода Ало");
            return;
        }

        Cell bestExit = FindClosestExit(startPos, exitCells);
        List<Vector2Int> path = _pathFinder.FindPath(startPos, bestExit.GridPosition);

        if (path != null && path.Count > 0)
        {
            _roadManager.AddCar();
            await MoveAlongPath(path);
        }
        else
        {
            _scaleShakeEffect.Shake();
        }
    }

    private Cell FindClosestExit(Vector2Int startPos, List<Cell> exitCells)
    {
        Cell bestExit = exitCells[0];
        float bestDistance = Vector2Int.Distance(startPos, bestExit.GridPosition);

        foreach (var exit in exitCells)
        {
            float dist = Vector2Int.Distance(startPos, exit.GridPosition);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestExit = exit;
            }
        }
        return bestExit;
    }

    public bool TryGetSeat(out Transform seat)
    {
        if (_seats <= 0)
        {
            seat = null;
            return false;
        }

        _seats--;

        seat = transform;

        if (_seats >= 0 && _seats < _passengers.Length)
        {
            _passengers[_seats].SetActive(true);
        }

        return true;
    }

    public void SetSpline(SplineContainer splineContainer)
    {
        if (_splineAnimator == null) return;

        _splineAnimator.enabled = false;
        _splineAnimator.Container = splineContainer;
    }

    public void SetRoad(Transform point) => _roadPoint = point;

    private async UniTask MoveAlongPath(List<Vector2Int> path)
    {
        var token = this.GetCancellationTokenOnDestroy();

        _isMoving = true;
        _rigidbody.isKinematic = false;

        Cell currentCell = GetCurrentCell();
        currentCell?.TryClearCar();

        foreach (Vector2Int nextCellPos in path)
        {
            token.ThrowIfCancellationRequested();

            Cell nextCell = _gridManager.GetCell(nextCellPos);
            if (nextCell == null) continue;

            while (nextCell.IsBlocked)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.DelayFrame(1, cancellationToken: token);
            }

            nextCell.TryApplyCar(this);
            _smoke.Clear();
            _smoke.Play();
            await MoveToPosition(nextCell.transform.position + Vector3.up * 0.5f);
            nextCell.TryClearCar();
        }
       _gridManager.RemoveCar(this);
        Vector3 exitPosition = transform.position;
        await MoveToPosition(exitPosition + Vector3.forward * 3f);

        await MoveToPosition(_roadPoint.position);
        await PlaySplineAnimator();
    }

    private async UniTask PlaySplineAnimator()
    {
        var token = this.GetCancellationTokenOnDestroy();

        if (_splineAnimator != null && _splineAnimator.Container != null)
        {
            _rigidbody.isKinematic = true;
            _splineAnimator.enabled = true;

            await UniTask.NextFrame(token);

            _splineAnimator.Play();

            while (_splineAnimator.NormalizedTime < 0.99f)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield(token);
            }

            CheckAfterCircle();
        }
    }

    private Cell GetCurrentCell()
    {
        float minDist = float.MaxValue;
        Cell closestCell = null;

        foreach (var cell in _gridManager.GetAllCells())
        {
            if (cell == null) continue;

            float dist = Vector3.Distance(transform.position, cell.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestCell = cell;
            }
        }
        return closestCell;
    }

    private async UniTask MoveToPosition(Vector3 targetPosition)
    {
        var token = this.GetCancellationTokenOnDestroy();

        if (Vector3.Distance(transform.position, targetPosition) <= _reachedDistance)
        {
            return;
        }

        while (true)
        {
             token.ThrowIfCancellationRequested();

            Vector3 direction = targetPosition - transform.position;
            float distance = direction.magnitude;

            if (distance <= _reachedDistance) break;

            if (distance > 0.5f)
            {
                direction.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _rigidbody.MoveRotation(Quaternion.Slerp(
                    _rigidbody.rotation,
                    targetRotation,
                    Time.deltaTime * _rotationSpeed
                ));
            }
            _rigidbody.MovePosition(Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _moveSpeed * Time.deltaTime
            ));

            await UniTask.Yield();
        }
    }

    private async UniTask LeaveSpline()
    {
        var token = this.GetCancellationTokenOnDestroy();

        _splineAnimator.enabled = false;
        _rigidbody.isKinematic = false;
        _isMoving = true;

        Vector3 targetPosition = transform.position + transform.forward * _leaveDistance;

        while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
        {
            token.ThrowIfCancellationRequested();

            _rigidbody.MovePosition(Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _moveSpeed * Time.deltaTime
            ));

            await UniTask.Yield(token);
        }

        _levelConstructor.CheckWinCondition();

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void CheckAfterCircle()
    {
        if (_leaving) return;

        if (_seats <= 0)
        {
            _roadManager.RemoveCar();
            _leaving = true;
            LeaveSpline().Forget();
        }
        else
        {
            PlaySplineAnimator().Forget();
        }
    }

    private void OnDrawGizmos()
    {
        if (_roadPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _roadPoint.position);
        }
    }
}
