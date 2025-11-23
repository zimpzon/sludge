using Assets.Scripts;
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
            public int Completions = 0;
        }

        public class SaveGame
        {
            public int TotalAttempts = 0;
            public int TotalDeaths = 0;
            public Dictionary<PlayerDeathType, int> DeathsByType = new();
            public Dictionary<int, LevelStats> CasualLevelsSeen = new();
            public Dictionary<int, LevelStats> HardLevelsSeen = new();
        }

        private const string PrefsName = "earl-in-space-savegame-v1";

        public static bool IsLevelCompleted(LevelNamespace ns, int levelId)
        {
            var stats = GetSavedStats(ns, levelId);
            return stats.Completions > 0;
        }

        public static bool HasGoldTime(LevelStats stats, float target)
        {
            return stats.BestTime > 0 && stats.BestTime <= target;
        }

        public static LevelStats GetSavedStats(LevelNamespace ns, int levelId)
        {
            if (ns == LevelNamespace.NotSet)
                return new LevelStats();

            var dict = ns == LevelNamespace.Casual ? saveGame.CasualLevelsSeen : saveGame.HardLevelsSeen;
            if (!dict.TryGetValue(levelId, out var stats))
                stats = new LevelStats();

            return stats;
        }

        static LevelStats UpdateSavedStats(RoundResult roundResult, Dictionary<int, LevelStats> levelsCompleted, out bool newBestTime)
        {
            newBestTime = false;
            if (!levelsCompleted.ContainsKey(roundResult.LevelId))
            {
                // Round done for new level
                Debug.Log($"Round done for first seen level {roundResult.LevelNamespace} levelId: {roundResult.LevelId}, completed: {roundResult.Completed}");
                float? bestTime = roundResult.Completed ? roundResult.Time : null;
                newBestTime = roundResult.Completed; // first round and completed - always best time
                var newLevelStats = new LevelStats { LevelId = roundResult.LevelId, BestTime = bestTime, Attempts = 1, Completions = roundResult.Completed ? 1 : 0 };
                levelsCompleted.Add(roundResult.LevelId, newLevelStats);
                Save();
                return newLevelStats;
            }
            else
            {
                // Round done for existing level
                var existingLevelStats = levelsCompleted[roundResult.LevelId];
                existingLevelStats.Attempts++;
                if (roundResult.Completed)
                    existingLevelStats.Completions++;

                Debug.Log($"Round done for already seen level {roundResult.LevelNamespace} levelId: {roundResult.LevelId}, completed: {existingLevelStats.Completions}");

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
                return UpdateSavedStats(roundResult, saveGame.CasualLevelsSeen, out newBestTime);
            }
            else if (roundResult.LevelNamespace == LevelNamespace.Hard)
            {
                return UpdateSavedStats(roundResult, saveGame.HardLevelsSeen, out newBestTime);
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

            if (saveGame.CasualLevelsSeen is null)
                saveGame.CasualLevelsSeen = new();

            if (saveGame.HardLevelsSeen is null)
                saveGame.HardLevelsSeen = new();

            Debug.Log("Existing SaveGame loaded");
            FirstLoadComplete = true;
        }
    }
}
