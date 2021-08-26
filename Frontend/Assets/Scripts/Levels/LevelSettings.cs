using Sludge.Shared;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    public string LevelName = "(no name)";
    public LevelData.LevelDifficulty Difficulty;
    public double StartTimeSeconds = 20;
    public double EliteCompletionTimeSeconds = 10;
    public string UniqueId;
    public int SortKey = 999999;
}
