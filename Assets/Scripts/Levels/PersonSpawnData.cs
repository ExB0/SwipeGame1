using System;
using Units;

namespace PlatformPuzzle.Levels
{
    [Serializable]
    public class PersonSpawnData
    {
        public UnitType UnitType = UnitType.Person;
        public UnitColor Color;
    }
}