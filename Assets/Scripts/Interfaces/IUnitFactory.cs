using Units;
using UnityEngine;

namespace InterFaces
{
    public interface IUnitFactory
    {
        GameObject Create(UnitType type, UnitColor color, Vector3 position);
    }
}