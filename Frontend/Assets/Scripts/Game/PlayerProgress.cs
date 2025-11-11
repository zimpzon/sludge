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
            // Add bronze, etc. here.
            public int LevelId = -1;
        }

        public class SaveGame
        {
            public Dictionary<int, LevelStats> CasualLevelsCompleted = new();
            public Dictionary<int, LevelStats> HardLevelsCompleted = new();
        }

        private const string PrefsName = "earl-in-space-savegame-v1";

        public static bool LevelIsCompleted(LevelNamespace ns, int levelId)
        {
            if (ns == LevelNamespace.Casual)
                return saveGame.CasualLevelsCompleted.ContainsKey(levelId);
            else if (ns == LevelNamespace.Hard)
                return saveGame.HardLevelsCompleted.ContainsKey(levelId);

            Debug.LogError($"unknown level namespace: {ns}");

            return true;
        }

        public static void UpdateProgress(RoundResult roundResult)
        {
            if (!roundResult.Completed || UiLogic.Instance.StartCurrentScene)
                return;

            if (roundResult.LevelNamespace == LevelNamespace.Casual)
            {
                if (!saveGame.CasualLevelsCompleted.ContainsKey(roundResult.LevelId))
                {
                    // New Casual level completed
                    Debug.Log($"New {roundResult.LevelNamespace} levelId completed: {roundResult.LevelId}");
                    saveGame.CasualLevelsCompleted.Add(roundResult.LevelId, new LevelStats { LevelId = roundResult.LevelId });
                    Save();
                }
                else
                {
                    // Level was already completed
                    Debug.Log($"Level was already completed: {roundResult.LevelNamespace}, levelId: {roundResult.LevelId}");
                }
            }
            else if (roundResult.LevelNamespace == LevelNamespace.Hard)
            {
                if (!saveGame.HardLevelsCompleted.ContainsKey(roundResult.LevelId))
                {
                    // New Hard level completed
                    Debug.Log($"New {roundResult.LevelNamespace} levelId completed: {roundResult.LevelId}");
                    saveGame.HardLevelsCompleted.Add(roundResult.LevelId, new LevelStats { LevelId = roundResult.LevelId });
                    Save();
                }
                else
                {
                    // Level was already completed
                }
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
