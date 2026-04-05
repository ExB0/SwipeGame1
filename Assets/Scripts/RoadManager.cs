using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private int _maxCarsNumber = 3;

    [SerializeField] private int _currentCarsNumber;

    public bool IsRoadFull()
    {
        return _currentCarsNumber >= _maxCarsNumber;
    }

    public void AddCar()
    {
        _currentCarsNumber += 1;
    }

    public void RemoveCar()
    {
        _currentCarsNumber -= 1;
    }

    public void ClearCars()
    {
        _currentCarsNumber = 0;
    }
    public bool HasCars()
    {
        return _currentCarsNumber > 0;
    }
}
