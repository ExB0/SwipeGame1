using UnityEngine;
using Units;
[RequireComponent(typeof(Collider))]
public class Cell : MonoBehaviour, IClickable
{
    [SerializeField] private Car _currentCar;
    [SerializeField] private bool _isObstacle;
    [SerializeField] private float _carYOffset = 3f;

    [SerializeField] private MeshRenderer _groundRenderer;
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _availableMaterial;

    public Vector2Int GridPosition { get; private set; }
    public bool IsReserved { get; private set; }
    public bool IsBlocked => HasCar || IsReserved || _isObstacle;
    public bool HasCar => _currentCar != null;
    public bool IsObstacle => _isObstacle;
    public Car CurrentCar => _currentCar;

    public void Initialize(Vector2Int gridPos) => GridPosition = gridPos;
    public void Reserve() => IsReserved = true;
    public void Unreserve() => IsReserved = false;

    public bool TrySetCar(Car car)
    {
        if (car == null || HasCar) return false;

        _currentCar = car;
        car.transform.position = transform.position + Vector3.down * _carYOffset;
        car.transform.SetParent(transform);
        return true;
    }

    public bool TryApplyCar(Car car)
    {
        if (car == null || IsBlocked) return false;
        
        _currentCar = car;
        return true;
    }

    public bool TryClearCar()
    {
        if (!HasCar) return false;

        _currentCar.transform.SetParent(null);
        _currentCar = null;
        return true;
    }
    public void SetObstacle(bool value)
    {
        _isObstacle = value;

        if (_isObstacle)
            SetAvailable(false);
    }
    public void SetAvailable(bool available)
    {
        _groundRenderer.material = available
            ? _availableMaterial
            : _normalMaterial;
    }


    public void OnClick()
    {
        _currentCar?.OnClick();
    }
}