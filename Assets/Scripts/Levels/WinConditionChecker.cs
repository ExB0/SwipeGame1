using UnityEngine;

using Buildings;
using PlatformPuzzle.Managers;

namespace PlatformPuzzle.Levels
{
    public class WinConditionChecker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridCarSpawner _gridCarSpawner;
        [SerializeField] private RoadManager _roadManager;
        [SerializeField] private Spawner[] _spawners;

        public bool IsWinConditionMet()
        {
            if (!AreSpawnersFinished())
                return false;

            if (_gridCarSpawner != null && _gridCarSpawner.HasActiveCars())
                return false;

            if (_roadManager != null && _roadManager.HasCars())
                return false;

            return true;
        }

        private bool AreSpawnersFinished()
        {
            if (_spawners == null || _spawners.Length == 0)
                return true;

            foreach (Spawner spawner in _spawners)
            {
                if (spawner == null)
                    continue;

                if (!spawner.IsFinished())
                    return false;
            }

            return true;
        }
    }
}