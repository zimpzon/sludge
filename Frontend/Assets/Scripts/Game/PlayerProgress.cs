using Newtonsoft.Json;
using Sludge.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Sludge.Utility
{
    public class PlayerProgress
    {
        public static string NamespaceDisplayName(LevelNamespace ns)
        {
            if (ns == LevelNamespace.Casual)
                return "Casual";
            else if (ns == LevelNamespace.Hard)
                return "Hard";

            return $"Unknown ns: {ns}";
        }

        public enum LevelNamespace { NotSet, Casual, Hard };

        public static SaveGame saveGame = new SaveGame();

        public class LevelStats
        {
            public int LevelId = -1;
            public float BestTime = -1;
            public int Attempts = -1;
        }

        public class SaveGame
        {
            public Dictionary<int, LevelStats> CasualLevelsCompleted = new();
            public Dictionary<int, LevelStats> HardLevelsCompleted = new();
        }

        private const string PrefsName = "earl-in-space-savegame-v1";

        public static bool IsLevelCompleted(LevelNamespace ns, int levelId)
        {
            if (ns == LevelNamespace.Casual)
                return saveGame.CasualLevelsCompleted.ContainsKey(levelId);
            else if (ns == LevelNamespace.Hard)
                return saveGame.HardLevelsCompleted.ContainsKey(levelId);

            Debug.LogError($"unknown level namespace: {ns}");

            return true;
        }

        public static LevelStats GetSavedStats(LevelNamespace ns, int levelId)
        {
            if (ns == LevelNamespace.NotSet)
                return new LevelStats();

            var dict = ns == LevelNamespace.Casual ? saveGame.CasualLevelsCompleted : saveGame.HardLevelsCompleted;
            if (!dict.TryGetValue(levelId, out var stats))
                stats = new LevelStats();

            return stats;
        }

        static void UpdateSavedStats(RoundResult roundResult, Dictionary<int, LevelStats> levelsCompleted)
        {
            if (!levelsCompleted.ContainsKey(roundResult.LevelId))
            {
                // New level completed
                Debug.Log($"New stats for {roundResult.LevelNamespace} levelId: {roundResult.LevelId}");
                var newLevelStats = new LevelStats { LevelId = roundResult.LevelId, BestTime = roundResult.Time, Attempts = 1 };
                levelsCompleted.Add(roundResult.LevelId, newLevelStats);
            }
            else
            {
                // Already completed
                var existingLevelStats = levelsCompleted[roundResult.LevelId];
                existingLevelStats.Attempts++;

                if (roundResult.Completed && roundResult.Time < existingLevelStats.BestTime)
                {
                    Debug.Log($"New best time for {roundResult.LevelNamespace}, levelId: {roundResult.LevelId}: {existingLevelStats.BestTime} -> {roundResult.Time}");
                    existingLevelStats = levelsCompleted[roundResult.LevelId];
                    existingLevelStats.BestTime = roundResult.Time;
                }
            }
            Save();
        }

        public static void UpdateWithRoundResult(RoundResult roundResult)
        {
            if (UiLogic.Instance.StartCurrentScene) // We don't have a namespace if started from editor
                return;

            if (roundResult.LevelNamespace == LevelNamespace.Casual)
            {
                UpdateSavedStats(roundResult, saveGame.CasualLevelsCompleted);
            }
            else if (roundResult.LevelNamespace == LevelNamespace.Hard)
            {
                UpdateSavedStats(roundResult, saveGame.HardLevelsCompleted);
            }
        }

        public static void Save()
        {
            Debug.Log($"Saving game...");
            string json = JsonConvert.SerializeObject(saveGame);
            PlayerPrefs.SetString(PrefsName, json);
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            Debug.Log("Loading game...");
            saveGame = new SaveGame();

            string json = PlayerPrefs.GetString(PrefsName, null);
            if (json == null)
            {
                saveGame = new SaveGame();
                Debug.Log("Empy SaveGame loaded");
            }
            else
            {
                saveGame = JsonConvert.DeserializeObject<SaveGame>(json) ?? new SaveGame();
            }

            if (saveGame.CasualLevelsCompleted is null)
                saveGame.CasualLevelsCompleted = new();

            if (saveGame.HardLevelsCompleted is null)
                saveGame.HardLevelsCompleted = new();

            Debug.Log("Existing SaveGame loaded");
        }
    }
}
