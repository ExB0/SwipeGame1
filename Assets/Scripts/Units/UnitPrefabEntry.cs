using UnityEngine;

namespace Units
{
    [System.Serializable]
    public struct UnitPrefabEntry
    {
        public UnitType UnitType;
        public UnitColor UnitColor;
        public GameObject Prefab;
    }
}