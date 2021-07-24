using Newtonsoft.Json;
using Sludge.Shared;
using System.Collections.Generic;
using UnityEngine;

public static class LevelList
{
    public static List<LevelData> Levels = new List<LevelData>();

    public static void LoadLevels()
    {
		int count = 1;
		while (true)
		{
			var textAsset = Resources.Load<TextAsset>($"Levels/Easy/{count}");
			if (textAsset == null)
				break;

			var levelData = JsonConvert.DeserializeObject<LevelData>(textAsset.text);
			levelData.Id = $"E{count:00}";
			if (string.IsNullOrEmpty(levelData.Name))
				levelData.Name = $"Level {count}";

			Levels.Add(levelData);
			count++;
		}

		Debug.Log($"Loaded {Levels.Count} levels");
	}
}
