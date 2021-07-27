using Sludge.Shared;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiLevelsLayout : MonoBehaviour
{
    public int ItemsPerRow = 12;
    public GameObject LevelPrefab;
    public List<LevelItem> LevelItems = new List<LevelItem>();

    public LevelItem GetLevelFromId(string levelId)
        => LevelItems.Where(li => li.levelScript.levelData.Id == levelId).First();

    public void CreateLevelsSelection(List<LevelData> levels)
    {
        for (int i = 0; i < levels.Count; ++i)
        {
            var go = Instantiate(LevelPrefab);
            var levelItem = new LevelItem
            {
                go = go,
                levelScript = go.GetComponent<UiLevel>(),
                navigation = go.GetComponent<UiNavigation>(),
            };

            levelItem.levelScript.levelData = levels[i];
            levelItem.levelScript.TextLevelId.text = levels[i].Id;

            LevelItems.Add(levelItem);
            go.transform.SetParent(this.transform, worldPositionStays: false);
        }

        SetNavigation(LevelItems);
    }

    void SetNavigation(List<LevelItem> levels)
    {
        for (int i = 0; i < levels.Count; ++i)
        {
            var level = levels[i];
            int idxUp = i - ItemsPerRow;
            int idxDown = i + ItemsPerRow;

            if (idxUp >= 0)
                level.navigation.Up = levels[idxUp].go;

            if (idxDown < levels.Count)
                level.navigation.Down = levels[idxDown].go;

            if (i > 0)
                level.navigation.Left = levels[i - 1].go;

            if (i < levels.Count - 1)
                level.navigation.Right = levels[i + 1].go;
        }
    }
}

public class LevelItem
{
    public GameObject go;
    public UiLevel levelScript;
    public UiNavigation navigation;
}