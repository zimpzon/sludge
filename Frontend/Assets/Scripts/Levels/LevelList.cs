using Newtonsoft.Json;
using Sludge.Shared;
using System.Collections.Generic;
using UnityEngine;

public static class LevelList
{
	public static List<LevelData> CasualLevels = new List<LevelData>();
	public static List<LevelData> HardLevels = new List<LevelData>();

	public static void LoadLevels()
    {
		CasualLevels.Clear();
		HardLevels.Clear();

		var allLevels = Resources.LoadAll<TextAsset>("Levels");
		for (int i = 0; i < allLevels.Length; ++i)
        {
			LevelData levelData = JsonConvert.DeserializeObject<LevelData>(allLevels[i].text);
			levelData.SetNamespaceAndIdFromFilename(allLevels[i].name);

			if (levelData.LevelId <= 0)
				Debug.LogError("missing level id, it should have been auto-set when saving a level in the format [namespace]-[levelId]");

			if (levelData.Namespace == Sludge.Utility.PlayerProgress.LevelNamespace.NotSet)
				Debug.LogError("missing level namespacee, it should have been auto-set when saving a level in the format [namespace]-[levelId] ");

			if (levelData.Namespace == Sludge.Utility.PlayerProgress.LevelNamespace.Casual)
			{
				CasualLevels.Add(levelData);
			}
			else if (levelData.Namespace == Sludge.Utility.PlayerProgress.LevelNamespace.Hard)
            {
				HardLevels.Add(levelData);
            }
			else
            {
				Debug.LogError($"unknown level namespace: {levelData.Namespace}");
            }
		}

		Debug.Log($"Loaded {CasualLevels.Count} casual levels");
		Debug.Log($"Loaded {HardLevels.Count} hard levels");
	}
}
