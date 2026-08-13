using SavingSystem;
using UnityEngine;
using YG;

namespace PlatformPuzzle.Levels
{
    public class LevelProgressService : MonoBehaviour
    {
        [SerializeField] private LeaderboardYG _leaderboardYG;
        [SerializeField] private string _leaderboardName = "LeaderBoardYG";

        public int CompleteLevel(LevelData levelData, int currentLevelIndex)
        {
            if (levelData == null)
            {
                Debug.LogError("LevelProgressService: LevelData is null");
                return 0;
            }

            SaveData data = SaveSystem.Load();

            int reward = levelData.ScoreReward;

            data.TotalScore += reward;

            if (data.TotalScore > data.BestScore)
            {
                data.BestScore = data.TotalScore;

                YG2.SetLeaderboard(
                    _leaderboardName,
                    data.BestScore
                );

                if (_leaderboardYG != null)
                {
                    _leaderboardYG.UpdateLB();
                }
            }

            if (currentLevelIndex >= data.UnlockedLevel)
            {
                data.UnlockedLevel = currentLevelIndex + 1;
            }

            SaveSystem.Save(data);

            return reward;
        }
    }
}
