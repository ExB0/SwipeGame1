using UnityEngine;

public enum UnitType { Car, Person }
public enum UnitColor { Red, Green, Blue }

[System.Serializable]
public struct UnitPrefabEntry
{
    public UnitType unitType;
    public UnitColor unitColor;
    public GameObject prefab;
}