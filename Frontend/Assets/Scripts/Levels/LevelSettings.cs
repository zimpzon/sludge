using Sludge.Colors;
using Sludge.Shared;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    public string LevelName = "(no name)";
    public LevelData.LevelDifficulty Difficulty;
    public double StartTimeSeconds = 20;
    public double EliteCompletionTimeSeconds = 10;
    public ColorSchemeScriptableObject ColorScheme; // Change this then save level. Note that only the name is saved, not this object.
    public string ColorSchemeName;
    public string UniqueId;
    public int SortKey = 999999;
}
