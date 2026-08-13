using System;
using System.Collections.Generic;
using UnityEngine;
using Units;

namespace PlatformPuzzle.Levels
{
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
}