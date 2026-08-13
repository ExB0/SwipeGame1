using System;
using UnityEngine;
using Units;

namespace PlatformPuzzle.Levels
{
    [Serializable]
    public class CarSpawnData
    {
        public UnitType UnitType = UnitType.Car;
        public UnitColor Color;
        public Vector2Int GridPosition;
    }
}