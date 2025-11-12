using Sludge.Colors;
using Sludge.Shared;
using Sludge.Utility;
using System.Collections.Generic;
using UnityEngine;

public class UiLevelsLayout : MonoBehaviour
{
    public int ItemsPerRow = 12;
    public GameObject LevelPrefab;
    public List<LevelItem> LevelItems = new List<LevelItem>();

    PlayerProgress.LevelNamespace _levelNamespace;

    public void CreateLevelsSelection(List<LevelData> levels, PlayerProgress.LevelNamespace levelNamespace)
    {
        _levelNamespace = levelNamespace;

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

    public LevelItem GetLevelFromId(int id)
    {
        int levelIdx = id - 1;
        if (LevelItems.Count == 0)
            Debug.LogError($"add at least one level in each namespace!");

        bool defaultToFirstItem = levelIdx <= 0 || levelIdx >= LevelItems.Count;
        if (defaultToFirstItem)
            return LevelItems[0];

        return LevelItems[levelIdx];
    }

    public void UpdateVisualHints()
    {
        bool prevWasCompleted = false;

        for (int i = 0; i < LevelItems.Count; ++i)
        {
            var levelItem = LevelItems[i];
            int levelId = levelItem.levelScript.LevelData.LevelId;
            var savedStats = PlayerProgress.GetSavedStats(_levelNamespace, levelId);
            bool hasGoldTime = PlayerProgress.HasGoldTime(savedStats,levelItem.levelScript.LevelData.TargetTime);

            bool isCompleted = PlayerProgress.IsLevelCompleted(_levelNamespace, levelItem.levelScript.LevelData.LevelId);
            bool isFirstLevel = levelId == 1;
            bool isUnlocked = false;

            if (isCompleted)
            {
                isUnlocked = true;
                prevWasCompleted = true;
                Debug.Log($"-----------> {_levelNamespace} LevelId {levelItem.levelScript.LevelData.LevelId} unlocked = {isUnlocked} (levelCompleted)");
            }
            else if (prevWasCompleted)
            {
                isUnlocked = true;
                prevWasCompleted = false;
                Debug.Log($"-----------> {_levelNamespace} LevelId {levelItem.levelScript.LevelData.LevelId} unlocked = {isUnlocked} (PrevWasCompleted)");
            }
            else if (isFirstLevel)
            {
                isUnlocked = true;
                prevWasCompleted = false;
                Debug.Log($"-----------> {_levelNamespace} LevelId {levelItem.levelScript.LevelData.LevelId} unlocked = {isUnlocked} (isFirstLevel)");
            }
            else
            {
                // One of the remaining unclocked levels
                prevWasCompleted = false;
            }

            string levelText = isUnlocked ? $"{i + 1}" : "-";
            levelItem.levelScript.TextLevelNumber.text = levelText;
            levelItem.levelScript.GoldImage.enabled = hasGoldTime;
            levelItem.levelScript.IsUnlocked = isUnlocked;
            levelItem.levelScript.HasGoldTime = hasGoldTime;
            levelItem.levelScript.LevelIndex = i;
            if (i < LevelItems.Count - 1)
                levelItem.levelScript.Next = LevelItems[i + 1].levelScript;

            SchemeColor backgroundColor;
            SchemeColor textColor;

            if (isUnlocked)
            {
                backgroundColor = SchemeColor.UiLevelMastered;
                textColor = SchemeColor.UiTextDefault;
            }
            else
            {
                backgroundColor = isUnlocked ? SchemeColor.UiLevelUnlocked : SchemeColor.UiLevelLocked;
                textColor = isUnlocked ? SchemeColor.UiTextDefault : SchemeColor.UiTextDimmed;
            }

            levelItem.levelScript.TextLevelNumber.color = ColorScheme.GetColor(GameManager.I.CurrentUiColorScheme, textColor);
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