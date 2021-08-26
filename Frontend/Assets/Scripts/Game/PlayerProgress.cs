using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Sludge.Utility
{
    public class PlayerProgress
    {
        public static PlayerProgress Progress = new PlayerProgress();

        public enum LevelStatus { NotCompleted = 0, Completed = 1, Elite = 2 };
        static Dictionary<string, LevelStatus> LevelStates = new Dictionary<string, LevelStatus>();

        private const string PrefsName = "player_progress";

        public static LevelStatus GetLevelStatus(string levelUniqueId)
        {
            if (string.IsNullOrWhiteSpace(levelUniqueId))
            {
                Debug.LogError("GetLevelStatus called with empty levelUniqueId");
                return LevelStatus.NotCompleted;
            }

            LevelStatus result;
            LevelStates.TryGetValue(levelUniqueId, out result);
            return result;
        }

        public static void UpdateLevelStatus(RoundResult roundResult)
        {
            if (!roundResult.Completed)
                return;

            var currentStatus = GetLevelStatus(roundResult.LevelId);
            LevelStatus resultStatus = roundResult.IsEliteTime ? LevelStatus.Elite : LevelStatus.Completed;
            if (resultStatus <= currentStatus) // Not an improvement
                return;

            LevelStates[roundResult.LevelId] = resultStatus;
            Save();
            Debug.Log($"Progress saved, new status for {roundResult.LevelName} = {resultStatus}");
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

            LevelStates = JsonConvert.DeserializeObject<Dictionary<string, LevelStatus>>(json);
            if (LevelStates == null)
                LevelStates = new Dictionary<string, LevelStatus>();

            Debug.Log("Progress loaded: " + json);
        }
    }
}
