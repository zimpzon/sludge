using Newtonsoft.Json;
using Sludge.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Sludge.Utility
{
    public class PlayerProgress
    {
        public class LevelProgress
        {
            public LevelStatus LevelStatus;
            public double BestTime = -1;
        }

        public static PlayerProgress Progress = new PlayerProgress();

        public enum LevelStatus { NotCompleted = 0, Escaped = 1, Completed = 2 };
        static Dictionary<string, LevelProgress> LevelStates = new Dictionary<string, LevelProgress>();

        private const string PrefsName = "player_progress";

        public static LevelProgress GetLevelProgress(string levelUniqueId)
        {
            if (string.IsNullOrWhiteSpace(levelUniqueId))
            {
                Debug.LogError("GetLevelStatus called with empty levelUniqueId");
                return new LevelProgress();
            }

            if (!LevelStates.TryGetValue(levelUniqueId, out var result))
                return new LevelProgress();

            return result;
        }

        public static void UpdateLevelStatus(RoundResult roundResult)
        {
            if (!roundResult.Completed || UiLogic.Instance.StartCurrentScene)
                return;

            var currentProgress = GetLevelProgress(roundResult.LevelId);

            bool hasChanged = false;
            if (roundResult.EndTime < currentProgress.BestTime)
            {
                currentProgress.BestTime = roundResult.EndTime;
                hasChanged = true;
            }

            var resultStatus = roundResult.IsEliteTime ? LevelStatus.Completed : LevelStatus.Escaped;
            if (resultStatus > currentProgress.LevelStatus)
            {
                currentProgress.LevelStatus = resultStatus;
                hasChanged = true;
            }

            if (!hasChanged)
                return;

            LevelStates[roundResult.LevelId] = currentProgress;
            Save();
            Debug.Log($"Progress saved, new status for {roundResult.LevelName}: {JsonConvert.SerializeObject(currentProgress)}");
        }

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(LevelStates);
            PlayerPrefs.SetString(PrefsName, json);
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            string json = PlayerPrefs.GetString(PrefsName, null);
            if (json == null)
                return;

            LevelStates = JsonConvert.DeserializeObject<Dictionary<string, LevelProgress>>(json);
            if (LevelStates == null)
                LevelStates = new Dictionary<string, LevelProgress>();

            Debug.Log("Progress loaded: " + json);
        }
    }
}
