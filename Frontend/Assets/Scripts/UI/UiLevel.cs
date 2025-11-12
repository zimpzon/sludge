using Sludge.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiLevel : MonoBehaviour
{
    public bool IsUnlocked;
    public bool HasGoldTime;
    public int LevelIndex;
    public UiLevel Next;
    public TextMeshProUGUI TextLevelNumber;
    public Image GoldImage;
    public LevelData LevelData;
}
