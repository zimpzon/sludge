using Sludge.Colors;
using Sludge.Shared;
using Sludge.UI;
using Sludge.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiLevelsLayout : MonoBehaviour
{
    public int ItemsPerRow = 12;
    public GameObject LevelPrefab;
    public List<LevelItem> LevelItems = new List<LevelItem>();

    public LevelItem GetLevelFromUniqueId(string qualifiedName)
        => LevelItems.Where(li => li.levelScript.LevelData.UniqueId == qualifiedName).First();

    public void CreateLevelsSelection(List<LevelData> levels)
    {
        levels = levels.OrderBy(l => l.Difficulty).ThenBy(l => l.SortKey).ThenBy(l => l.UniqueId).ToList();

        int levelCounter = 0;
        for (int i = 0; i < levels.Count; ++i)
        {
            var level = levels[i];
            levelCounter++;

            var go = Instantiate(LevelPrefab);
            var levelItem = new LevelItem
            {
                go = go,
                levelScript = go.GetComponent<UiLevel>(),
                navigation = go.GetComponent<UiNavigation>(),
                colorApplier = go.GetComponent<UiSchemeColorApplier>(),
            };

            levelItem.levelScript.LevelData = level;

            LevelItems.Add(levelItem);
            go.transform.SetParent(this.transform, worldPositionStays: false);
        }

        UpdateVisualHints();
        SetNavigation(LevelItems);
    }

    public void UpdateVisualHints()
    {
        const float RequiredUnlockPct = 0.8f; // Pct of previous levels that must be completed.
        for (int i = 0; i < LevelItems.Count; ++i)
        {
            var levelItem = LevelItems[i];
            bool isUnlocked = Mathf.CeilToInt(UiLogic.Instance.LevelsCompletedCount * RequiredUnlockPct) >= i;

            var difficulty = levelItem.levelScript.LevelData.Difficulty;
            var levelStatus = PlayerProgress.GetLevelStatus(levelItem.levelScript.LevelData.UniqueId);
            string levelText = isUnlocked ?
                $"{LevelData.DifficultyIds[(int)difficulty]}\n{(i + 1):00}" :
                "?";

            levelItem.levelScript.Status = levelStatus;
            levelItem.levelScript.TextLevelNumber.text = levelText;
            levelItem.levelScript.IsUnlocked = isUnlocked;
            levelItem.levelScript.LevelIndex = i;

            SchemeColor backgroundColor;
            SchemeColor textColor;

            if (levelStatus == PlayerProgress.LevelStatus.Completed)
            {
                backgroundColor = SchemeColor.UiLevelCompleted;
                textColor = SchemeColor.UiTextDefault;
            }
            else if (levelStatus == PlayerProgress.LevelStatus.Elite)
            {
                backgroundColor = SchemeColor.UiLevelMastered;
                textColor = SchemeColor.UiTextDefault;
            }
            else
            {
                backgroundColor = isUnlocked ? SchemeColor.UiLevelUnlocked : SchemeColor.UiLevelLocked;
                textColor = isUnlocked ? SchemeColor.UiTextDefault : SchemeColor.UiTextDimmed;
            }

            levelItem.colorApplier.SetColor(backgroundColor);
            levelItem.levelScript.TextLevelNumber.color = ColorScheme.GetColor(GameManager.Instance.CurrentUiColorScheme, textColor);
        }
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
    public UiSchemeColorApplier colorApplier;
}