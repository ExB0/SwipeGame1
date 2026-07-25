using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Splines;
using System.Collections.Generic;
using System.Threading;


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

    [SerializeField] private AudioSource _engineStartAudio;

    public Color GetColor() => _color;
    public bool IsMoving => _isMoving;


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
    private CancellationToken _levelToken;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _pathFinder = new PathFinder();
        _splineAnimator = GetComponent<SplineAnimate>();
        _scaleShakeEffect = GetComponent<ScaleShakeEffect>();

        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            _color = renderer.material.color;
        else
            Debug.LogError($"{name}: MeshRenderer not found");

        if (_splineAnimator == null)
            Debug.LogError($"{name}: SplineAnimate not found");

        if (_scaleShakeEffect == null)
            Debug.LogError($"{name}: ScaleShakeEffect not found");

        if (_smoke == null)
            Debug.LogWarning($"{name}: smoke particle is missing");

        if (_passengers == null)
            Debug.LogWarning($"{name}: passengers array is null");
    }


    private void Start()
    {
        _gridManager = GridManager.Instance;
        _roadManager = FindAnyObjectByType<RoadManager>();
        _levelConstructor = LevelConstructor.Instance;

        if (_gridManager == null)
            Debug.LogError($"{name}: GridManager not found");

        if (_roadManager == null)
            Debug.LogError($"{name}: RoadManager not found");

        if (_levelConstructor != null)
            _levelToken = _levelConstructor.LevelToken;

        if (_smoke != null)
            _smoke.Stop();
    }



    public void OnClick()
    {
        if (_gridManager == null || _roadManager == null)
            return;

        if (_scaleShakeEffect != null && _scaleShakeEffect.IsShaking)
            return;

        if (_isMoving)
            return;

        if (_roadManager.IsRoadFull())
        {
            _scaleShakeEffect?.Shake();
            return;
        }

        UniTask.Void(async () => await HandleClick());
    }
    private async UniTask HandleClick()
    {
        Cell currentCell = GetCurrentCell();

        if (currentCell == null)
            return;

        if (!TryGetPathToExit(currentCell, out List<Vector2Int> path))
        {
            _scaleShakeEffect?.Shake();
            return;
        }

        _isMoving = true;

        currentCell.SetAvailable(false);
        currentCell.TryClearCar();

        _roadManager.AddCar();
        _roadManager.UpdateCells();

        if (path.Count == 0)
        {
            _isMoving = true;

            await MoveToPosition(transform.position + Vector3.forward * 1f);
            await MoveToPosition(_roadPoint.position);
            await PlaySplineAnimator();

            return;
        }

        await MoveAlongPath(path);
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
    public bool CanMoveFrom(Cell currentCell)
    {
        return TryGetPathToExit(currentCell, out _);
    }

    public void SetRoad(Transform point) => _roadPoint = point;

    private async UniTask MoveAlongPath(List<Vector2Int> path)
    {
        var token = this.GetCancellationTokenOnDestroy();

        PlayEngineSound();

        _rigidbody.isKinematic = false;

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

            _roadManager.UpdateCells();

            if (_smoke != null)
            {
                _smoke.Clear();
                _smoke.Play();
            }

            await MoveToPosition(nextCell.transform.position + Vector3.up * 0.5f);

            nextCell.TryClearCar();

            _roadManager.UpdateCells();
        }

        _gridManager.RemoveCar(this);

        _roadManager.UpdateCells();

        Vector3 exitPosition = transform.position;
        await MoveToPosition(exitPosition + Vector3.forward * 3f);

        _roadManager.UpdateCells();

        await MoveToPosition(_roadPoint.position);
        await PlaySplineAnimator();
    }

    private bool TryGetPathToExit(Cell currentCell, out List<Vector2Int> path)
    {
        path = null;

        GridManager grid = GridManager.Instance;

        if (grid == null)
            return false;

        if (currentCell == null)
            return false;

        List<Cell> exitCells = grid.GetExitCells();

        if (exitCells == null || exitCells.Count == 0)
            return false;

        if (exitCells.Contains(currentCell))
        {
            path = new List<Vector2Int>();
            return true;
        }

        Cell bestExit = null;
        List<Vector2Int> bestPath = null;

        foreach (var exit in exitCells)
        {
            if (exit == null)
                continue;

            List<Vector2Int> candidatePath = _pathFinder.FindPath(
                currentCell.GridPosition,
                exit.GridPosition
            );

            if (candidatePath == null || candidatePath.Count == 0)
                continue;

            if (bestPath == null || candidatePath.Count < bestPath.Count)
            {
                bestPath = candidatePath;
                bestExit = exit;
            }
        }

        path = bestPath;

        return path != null;
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
            _levelConstructor.CheckWinCondition();
        }
        else
        {
            PlaySplineAnimator().Forget();
        }
    }
    private void PlayEngineSound()
    {
        if (_engineStartAudio != null)
        {
            _engineStartAudio.pitch = Random.Range(0.95f, 1.05f);
            _engineStartAudio.volume = Random.Range(0.85f, 1f);

            _engineStartAudio.Play();
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
