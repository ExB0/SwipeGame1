using UnityEngine;
using Units;
public interface IUnitFactory
{
    GameObject Create(UnitType type, UnitColor color, Vector3 position);
}
