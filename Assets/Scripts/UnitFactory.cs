using UnityEngine;
using System.Collections.Generic;

public class UnitFactory : MonoBehaviour, IUnitFactory
{
    [SerializeField] private List<UnitPrefabEntry> unitPrefabs;

    private Dictionary<(UnitType, UnitColor), GameObject> _prefabMap;

    private void Awake()
    {
        _prefabMap = new Dictionary<(UnitType, UnitColor), GameObject>();

        foreach (var entry in unitPrefabs)
        {
            var key = (entry.unitType, entry.unitColor);
            if (!_prefabMap.ContainsKey(key))
                _prefabMap.Add(key, entry.prefab);
            else
                Debug.LogWarning($"Нет данного объекта {key}");
        }
    }

    public GameObject Create(UnitType type, UnitColor color, Vector3 position)
    {
        if (_prefabMap.TryGetValue((type, color), out var prefab))
            return Instantiate(prefab, position, Quaternion.identity);

        Debug.LogError($"Нет такого префаба {type} со цветом {color}");
        return null;
    }
}