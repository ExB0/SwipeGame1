using UnityEngine;

public interface IUnitFactory
{
    GameObject Create(UnitType type, UnitColor color, Vector3 position);
}
