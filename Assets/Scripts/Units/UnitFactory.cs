using System.Collections.Generic;

using UnityEngine;

using InterFaces;

namespace Units
{
    public class UnitFactory : MonoBehaviour, IUnitFactory
    {
        [SerializeField] private List<UnitPrefabEntry> _unitPrefabs;

        private Dictionary<(UnitType, UnitColor), GameObject> _prefabMap;

        private void Awake()
        {
            if (_unitPrefabs == null || _unitPrefabs.Count == 0)
            {
                Debug.LogError("UnitFactory: _unitPrefabs is null or empty");
                enabled = false;
                return;
            }

            _prefabMap = new Dictionary<(UnitType, UnitColor), GameObject>();

            foreach (UnitPrefabEntry entry in _unitPrefabs)
            {
                if (entry.Prefab == null)
                {
                    Debug.LogWarning($"UnitFactory: prefab for {entry.UnitType} with color" +
                                     $"{entry.UnitColor} is null, skipping");
                    continue;
                }

                (UnitType, UnitColor) key = (entry.UnitType, entry.UnitColor);

                if (!_prefabMap.ContainsKey(key))
                {
                    _prefabMap.Add(key, entry.Prefab);
                }
                else
                {
                    Debug.LogWarning($"UnitFactory: duplicate key {key} found, skipping");
                }
            }

            if (_prefabMap.Count == 0)
            {
                Debug.LogError("UnitFactory: no valid prefabs loaded");
                enabled = false;
            }
        }

        public GameObject Create(UnitType type, UnitColor color, Vector3 position)
        {
            if (_prefabMap == null || _prefabMap.Count == 0)
            {
                Debug.LogError("UnitFactory: prefab map not initialized");
                return null;
            }

            if (_prefabMap.TryGetValue((type, color), out GameObject prefab))
            {
                return Instantiate(prefab, position, Quaternion.identity);
            }

            Debug.LogError($"UnitFactory: no prefab for {type} with color {color}");
            return null;
        }
    }
}