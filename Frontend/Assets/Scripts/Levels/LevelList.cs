using Newtonsoft.Json;
using Sludge.Shared;
using System.Collections.Generic;
using UnityEngine;

public static class LevelList
{
    public static List<LevelData> Levels = new List<LevelData>();

    public static void LoadLevels()
    {
		HashSet<string> idUniqueTest = new HashSet<string>();

		var allLevels = Resources.LoadAll<TextAsset>("Levels");
		for (int i = 0; i < allLevels.Length; ++i)
        {
			var levelData = JsonConvert.DeserializeObject<LevelData>(allLevels[i].text);
			if (string.IsNullOrEmpty(levelData.Name))
				levelData.Name = $"(no name)";

			if (idUniqueTest.Contains(levelData.UniqueId))
				Debug.LogError($"Level id {levelData.UniqueId} already exists. Current name: {levelData.Name}");

			idUniqueTest.Add(levelData.UniqueId);
			Levels.Add(levelData);
		}

		Debug.Log($"Loaded {Levels.Count} levels");
	}
}
