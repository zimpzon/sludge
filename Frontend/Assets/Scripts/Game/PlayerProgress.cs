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
        public static bool FirstLoadComplete = false;

        public class LevelStats
        {
            public int LevelId = -1;
            public float? BestTime;
            public int Attempts = 0;
            public bool IsCompleted;
        }

        public class SaveGame
        {
            public int TotalAttempts = 0;
            public Dictionary<int, LevelStats> CasualLevelsCompleted = new();
            public Dictionary<int, LevelStats> HardLevelsCompleted = new();
        }

        private const string PrefsName = "earl-in-space-savegame-v1";
        
        public static bool IsLevelCompleted(LevelNamespace ns, int levelId)
        {
            var stats = GetSavedStats(ns, levelId);
            return stats.IsCompleted;
        }

        public static bool HasGoldTime(LevelStats stats, float target)
        {
            return stats.BestTime > 0 && stats.BestTime <= target;
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

        static LevelStats UpdateSavedStats(RoundResult roundResult, Dictionary<int, LevelStats> levelsCompleted, out bool newBestTime)
        {
            newBestTime = false;
            if (!levelsCompleted.ContainsKey(roundResult.LevelId))
            {
                // New level completed
                Debug.Log($"New stats for {roundResult.LevelNamespace} levelId: {roundResult.LevelId}");
                float? bestTime = roundResult.Completed ? roundResult.Time : null;
                newBestTime = roundResult.Completed; // first round and completed - always best time
                var newLevelStats = new LevelStats { LevelId = roundResult.LevelId, BestTime = bestTime, Attempts = 1, IsCompleted = roundResult.Completed };
                levelsCompleted.Add(roundResult.LevelId, newLevelStats);
                Save();
                return newLevelStats;
            }
            else
            {
                // Already completed
                var existingLevelStats = levelsCompleted[roundResult.LevelId];
                existingLevelStats.Attempts++;
                existingLevelStats.IsCompleted |= roundResult.Completed;

                // New best if completed + faster then previous OR no existing best
                if (roundResult.Completed && (roundResult.Time < existingLevelStats.BestTime || !existingLevelStats.BestTime.HasValue))
                {
                    newBestTime = true;
                    Debug.Log($"New best time for {roundResult.LevelNamespace}, levelId: {roundResult.LevelId}: {existingLevelStats.BestTime} -> {roundResult.Time}");
                    existingLevelStats = levelsCompleted[roundResult.LevelId];
                    existingLevelStats.BestTime = roundResult.Time;
                }
                Save();
                return existingLevelStats;
            }
        }

        public static LevelStats UpdateWithRoundResult(RoundResult roundResult, out bool newBestTime)
        {
            newBestTime = false;

            if (UiLogic.Instance.StartCurrentScene) // We don't have a namespace if started from editor
                return new LevelStats();

            saveGame.TotalAttempts++;

            if (roundResult.LevelNamespace == LevelNamespace.Casual)
            {
                return UpdateSavedStats(roundResult, saveGame.CasualLevelsCompleted, out newBestTime);
            }
            else if (roundResult.LevelNamespace == LevelNamespace.Hard)
            {
                return UpdateSavedStats(roundResult, saveGame.HardLevelsCompleted, out newBestTime);
            }
            return new LevelStats();
        }

        public static void Save()
        {
            Debug.Log($"Saving game...");
            if (!FirstLoadComplete)
            {
                Debug.LogError("cannot save game, no load was ever done, could overwrite");
                return;
            }
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
            FirstLoadComplete = true;
        }
    }
}
