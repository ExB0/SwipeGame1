using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Cars")]
    public List<CarSpawnData> Cars;

    [Header("People Per Spawner")]
    public List<SpawnerPeopleData> Spawners;

    [Header("Score")]
    public int ScoreReward = 100;

    [Header("Timer")]
    [Range(10, 300)]
    public int TimeLimitSeconds = 120;

}
[System.Serializable]
public class CarSpawnData
{
    public UnitType UnitType = UnitType.Car;
    public UnitColor Color;
    public Vector2Int GridPosition;
}

[System.Serializable]
public class PersonSpawnData
{
    public UnitType UnitType = UnitType.Person;
    public UnitColor Color;
}

[System.Serializable]
public class SpawnerPeopleData
{
    public List<PersonSpawnData> People;
}
