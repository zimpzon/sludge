using Sludge.Shared;
using Sludge.Utility;
using TMPro;
using UnityEngine;

public class UiLevel : MonoBehaviour
{
    public PlayerProgress.LevelStatus Status;
    public bool IsUnlocked;
    public int LevelIndex;
    public TextMeshProUGUI TextLevelNumber;
    public LevelData LevelData;
}
