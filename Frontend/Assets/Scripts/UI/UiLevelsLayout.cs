using Sludge.Shared;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiLevelsLayout : MonoBehaviour
{
    public int ItemsPerRow = 12;
    public GameObject LevelPrefab;
    public List<LevelItem> LevelItems = new List<LevelItem>();

    public LevelItem GetLevelFromQualifiedId(string qualifiedName)
        => LevelItems.Where(li => li.levelScript.LevelData.GeneratedQualifiedName == qualifiedName).First();

    public void CreateLevelsSelection(List<LevelData> levels)
    {
        levels = levels.OrderBy(l => l.Difficulty).ThenBy(l => l.Id).ToList();

        LevelData.LevelDifficulty currentDifficulty = LevelData.LevelDifficulty.NotSet;
        int levelCounter = 0;

        for (int i = 0; i < levels.Count; ++i)
        {
            var level = levels[i];

            var go = Instantiate(LevelPrefab);
            var levelItem = new LevelItem
            {
                go = go,
                levelScript = go.GetComponent<UiLevel>(),
                navigation = go.GetComponent<UiNavigation>(),
            };

            if (level.Difficulty != currentDifficulty)
            {
                currentDifficulty = level.Difficulty;
                levelCounter = 1;
            }

            level.GeneratedQualifiedName = $"{LevelData.DifficultyIds[(int)level.Difficulty]}-{levelCounter:00}";

            levelItem.levelScript.LevelData = level;
            levelItem.levelScript.TextLevelNumber.text = level.GeneratedQualifiedName.Replace("-", "\n");

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