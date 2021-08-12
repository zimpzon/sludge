using Newtonsoft.Json;
using Sludge.Shared;
using System.Collections.Generic;
using UnityEngine;

public static class LevelList
{
    public static List<LevelData> Levels = new List<LevelData>();

    public static void LoadLevels()
    {
		var allLevels = Resources.LoadAll<TextAsset>("Levels");
		for (int i = 0; i < allLevels.Length; ++i)
        {
			var levelData = JsonConvert.DeserializeObject<LevelData>(allLevels[i].text);
			if (string.IsNullOrEmpty(levelData.Name))
				levelData.Name = $"(no name)";

			Levels.Add(levelData);
		}

		Debug.Log($"Loaded {Levels.Count} levels");
	}
}
