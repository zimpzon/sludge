using Sludge.Shared;
using TMPro;
using UnityEngine;

public class UiLevel : MonoBehaviour
{
    public bool IsUnlocked;
    public int LevelIndex;
    public UiLevel Next;
    public TextMeshProUGUI TextLevelNumber;
    public LevelData LevelData;
}
