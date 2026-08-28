using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

using Grid;
using InterFaces;
using PlatformPuzzle.Levels;
using PlatformPuzzle.Managers;
using PlatformPuzzle.Pathfinder;

namespace Units
{
    [RequireComponent(typeof(Rigidbody))]
    public class Car : MonoBehaviour, IColorMatchable
    {
        private const float RotationThreshold = 0.5f;
        private const float SplineCompletionThreshold = 0.99f;
        private const float MoveOffset = 1f;
        private const float ExitOffset = 3f;
        private const float PositionOffset = 0.5f;
        private const int WaitFrameCount = 1;
        private const float ReachedDistance = 0.7f;

        [SerializeField] private Color _color;
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private SplineAnimate _splineAnimator;
        [SerializeField] private int _seats = 2;
        [SerializeField] private float _leaveDistance = 20f;
        [SerializeField] private GameObject[] _passengers;
        [SerializeField] private CarAudio _carAudio;
        [SerializeField] private CarEffects _carEffects;

        private LevelFlowController _flowController;
        private Transform _roadPoint;
        private Rigidbody _rigidbody;
        private PathFinder _pathFinder;
        private bool _isMoving;
        private GridManager _gridManager;
        private GridCarSpawner _gridCarSpawner;
        private RoadManager _roadManager;
        private bool _leaving;

        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (_splineAnimator == null)
            {
                _splineAnimator = GetComponent<SplineAnimate>();
            }

            if (_carAudio == null)
            {
                _carAudio = GetComponent<CarAudio>();
            }

            if (_carEffects == null)
            {
                _carEffects = GetComponent<CarEffects>();
            }

            MeshRenderer renderer = GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                _color = renderer.material.color;
            }
            else
            {
                Debug.LogError($"{name}: MeshRenderer not found");
            }

            if (_splineAnimator == null)
            {
                Debug.LogError($"{name}: SplineAnimate not found");
            }

            if (_carAudio == null)
            {
                Debug.LogWarning($"{name}: CarAudio is missing");
            }

            if (_carEffects == null)
            {
                Debug.LogWarning($"{name}: CarEffects is missing");
            }

            if (_passengers == null)
            {
                Debug.LogWarning($"{name}: passengers array is null");
            }
        }

        private void OnDrawGizmos()
        {
            if (_roadPoint == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _roadPoint.position);
        }

        public Color GetColor()
        {
            return _color;
        }

        public void Initialize(
            GridManager gridManager,
            GridCarSpawner gridCarSpawner,
            RoadManager roadManager,
            LevelFlowController flowController,
            PathFinder pathFinder)
        {
            _gridManager = gridManager;
            _gridCarSpawner = gridCarSpawner;
            _roadManager = roadManager;
            _flowController = flowController;
            _pathFinder = pathFinder;

            _carEffects?.StopSmoke();
        }

        public void StartMove()
        {
            if (_gridManager == null ||
                _roadManager == null)
            {
                Debug.LogError($"{name}: Car was not initialized");
                return;
            }

            if (_carEffects != null &&
                _carEffects.IsShaking)
            {
                return;
            }

            if (_isMoving)
            {
                return;
            }

            if (_roadManager.IsRoadFull())
            {
                _carEffects?.Shake();
                return;
            }

            UniTask.Void(async () => await HandleClick());
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

            if (_passengers != null &&
                _seats >= 0 &&
                _seats < _passengers.Length &&
                _passengers[_seats] != null)
            {
                _passengers[_seats].SetActive(true);
            }

            return true;
        }

        public void SetSpline(SplineContainer splineContainer)
        {
            if (_splineAnimator == null)
            {
                return;
            }

            _splineAnimator.enabled = false;
            _splineAnimator.Container = splineContainer;
        }

        public void SetRoad(Transform point)
        {
            _roadPoint = point;
        }

        public bool CanMoveFrom(Cell currentCell)
        {
            return TryGetPathToExit(currentCell, out _);
        }

        private async UniTask HandleClick()
        {
            Cell currentCell = GetCurrentCell();

            if (currentCell == null)
            {
                return;
            }

            if (!TryGetPathToExit(
                    currentCell,
                    out List<Vector2Int> path))
            {
                _carEffects?.Shake();
                return;
            }

            _isMoving = true;

            currentCell.SetAvailable(false);
            currentCell.TryClearCar();

            _roadManager.AddCar();
            _roadManager.UpdateCells();

            if (path.Count == 0)
            {
                await MoveToPosition(
                    transform.position +
                    Vector3.forward * MoveOffset
                );

                await MoveToPosition(_roadPoint.position);
                await PlaySplineAnimator();

                return;
            }

            await MoveAlongPath(path);
        }

        private async UniTask MoveAlongPath(List<Vector2Int> path)
        {
            CancellationToken token =
                this.GetCancellationTokenOnDestroy();

            _carAudio?.PlayEngineStart();

            _rigidbody.isKinematic = false;

            foreach (Vector2Int nextCellPos in path)
            {
                token.ThrowIfCancellationRequested();

                Cell nextCell =
                    _gridManager.GetCell(nextCellPos);

                if (nextCell == null)
                {
                    continue;
                }

                while (nextCell.IsBlocked)
                {
                    token.ThrowIfCancellationRequested();

                    await UniTask.DelayFrame(
                        WaitFrameCount,
                        cancellationToken: token
                    );
                }

                nextCell.TryApplyCar(this);

                _roadManager.UpdateCells();

                _carEffects?.PlaySmoke();

                await MoveToPosition(
                    nextCell.transform.position +
                    Vector3.up * PositionOffset
                );

                nextCell.TryClearCar();

                _roadManager.UpdateCells();
            }

            _gridCarSpawner.RemoveCar(this);

            _roadManager.UpdateCells();

            Vector3 exitPosition = transform.position;

            await MoveToPosition(
                exitPosition + Vector3.forward * ExitOffset
            );

            _roadManager.UpdateCells();

            await MoveToPosition(_roadPoint.position);
            await PlaySplineAnimator();
        }

        private bool TryGetPathToExit(
            Cell currentCell,
            out List<Vector2Int> path)
        {
            path = null;

            if (_gridManager == null ||
                _pathFinder == null)
            {
                return false;
            }

            if (currentCell == null)
            {
                return false;
            }

            List<Cell> exitCells =
                _gridManager.GetExitCells();

            if (exitCells == null ||
                exitCells.Count == 0)
            {
                return false;
            }

            if (exitCells.Contains(currentCell))
            {
                path = new List<Vector2Int>();
                return true;
            }

            List<Vector2Int> bestPath = null;

            foreach (Cell exit in exitCells)
            {
                if (exit == null)
                {
                    continue;
                }

                List<Vector2Int> candidatePath =
                    _pathFinder.FindPath(
                        currentCell.GridPosition,
                        exit.GridPosition,
                        _gridManager
                    );

                if (candidatePath == null ||
                    candidatePath.Count == 0)
                {
                    continue;
                }

                if (bestPath == null ||
                    candidatePath.Count < bestPath.Count)
                {
                    bestPath = candidatePath;
                }
            }

            path = bestPath;

            return path != null;
        }

        private async UniTask PlaySplineAnimator()
        {
            CancellationToken token =
                this.GetCancellationTokenOnDestroy();

            if (_splineAnimator == null ||
                _splineAnimator.Container == null)
            {
                return;
            }

            _rigidbody.isKinematic = true;
            _splineAnimator.enabled = true;

            await UniTask.NextFrame(token);

            _splineAnimator.Play();

            while (_splineAnimator.NormalizedTime <
                   SplineCompletionThreshold)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield(token);
            }

            HandleCircleCompleted();
        }

        private Cell GetCurrentCell()
        {
            if (_gridManager == null)
            {
                Debug.LogError($"{name}: GridManager is null in GetCurrentCell");
                return null;
            }
            
            float minDistance = float.MaxValue;
            Cell closestCell = null;

            foreach (Cell cell in _gridManager.GetAllCells())
            {
                if (cell == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    transform.position,
                    cell.transform.position
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCell = cell;
                }
            }

            return closestCell;
        }

        private async UniTask MoveToPosition(Vector3 targetPosition)
        {
            CancellationToken token =
                this.GetCancellationTokenOnDestroy();

            if (Vector3.Distance(
                    transform.position,
                    targetPosition) <= ReachedDistance)
            {
                return;
            }

            while (Vector3.Distance(
                       transform.position,
                       targetPosition) > ReachedDistance)
            {
                token.ThrowIfCancellationRequested();

                Vector3 direction =
                    targetPosition - transform.position;

                float distance = direction.magnitude;

                if (distance > RotationThreshold)
                {
                    direction.Normalize();

                    Quaternion targetRotation =
                        Quaternion.LookRotation(direction);

                    _rigidbody.MoveRotation(
                        Quaternion.Slerp(
                            _rigidbody.rotation,
                            targetRotation,
                            Time.deltaTime * _rotationSpeed
                        )
                    );
                }

                _rigidbody.MovePosition(
                    Vector3.MoveTowards(
                        transform.position,
                        targetPosition,
                        _moveSpeed * Time.deltaTime
                    )
                );

                await UniTask.Yield();
            }
        }

        private async UniTask LeaveSpline()
        {
            CancellationToken token =
                this.GetCancellationTokenOnDestroy();

            _splineAnimator.enabled = false;
            _rigidbody.isKinematic = false;
            _isMoving = true;

            Vector3 targetPosition =
                transform.position +
                transform.forward * _leaveDistance;

            while (Vector3.Distance(
                       transform.position,
                       targetPosition) > RotationThreshold)
            {
                token.ThrowIfCancellationRequested();

                _rigidbody.MovePosition(
                    Vector3.MoveTowards(
                        transform.position,
                        targetPosition,
                        _moveSpeed * Time.deltaTime
                    )
                );

                await UniTask.Yield(token);
            }

            _flowController?.CheckWinCondition();

            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void HandleCircleCompleted()
        {
            if (_leaving)
            {
                return;
            }

            if (_seats <= 0)
            {
                _roadManager.RemoveCar();
                _leaving = true;
                LeaveSpline().Forget();
                _flowController?.CheckWinCondition();
            }
            else
            {
                PlaySplineAnimator().Forget();
            }
        }
    }
}