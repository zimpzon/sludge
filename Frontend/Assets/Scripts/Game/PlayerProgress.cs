using Newtonsoft.Json;
using Sludge.UI;
using UnityEngine;

namespace Sludge.Utility
{
    public class PlayerProgress
    {
        public enum LevelNamespace { NotSet, Casual, Hard };

        public static SaveGame saveGame = new SaveGame();

        public class SaveGame
        {
            public int CasualMaxCompleted;
            public int HardMaxCompleted;
        }

        private const string PrefsName = "cazzle-savegame-v1";

        public static bool LevelIsCompleted(LevelNamespace ns, int levelId)
        {
            if (ns == LevelNamespace.Casual)
                return levelId <= saveGame.CasualMaxCompleted;
            else if (ns == LevelNamespace.Hard)
                return levelId <= saveGame.HardMaxCompleted;

            Debug.LogError($"unknown level namespace: {ns}");

            return true;
        }

        public static void UpdateProgress(RoundResult roundResult)
        {
            if (!roundResult.Completed || UiLogic.Instance.StartCurrentScene)
                return;

            if (roundResult.LevelNamespace == LevelNamespace.Casual && roundResult.LevelId > saveGame.CasualMaxCompleted)
            {
                saveGame.CasualMaxCompleted = roundResult.LevelId;
                Save();
            }
            else if (roundResult.LevelNamespace == LevelNamespace.Hard && roundResult.LevelId > saveGame.HardMaxCompleted)
            {
                saveGame.HardMaxCompleted = roundResult.LevelId;
                Save();
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
                return;

            saveGame = JsonConvert.DeserializeObject<SaveGame>(json) ?? new SaveGame();
        }
    }
}
