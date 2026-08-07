using UnityEngine;

namespace Units
{
    public enum UnitType
    {
        Car,
        Person
    }

    public enum UnitColor
    {
        Red,
        Green,
        Blue
    }

    [System.Serializable]
    public struct UnitPrefabEntry
    {
        public UnitType UnitType;
        public UnitColor UnitColor;
        public GameObject Prefab;
    }
}