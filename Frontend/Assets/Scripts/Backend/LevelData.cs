using System.Collections.Generic;

namespace Sludge.Shared
{
    public class LevelData
    {
        public enum LevelDifficulty { NotSet = 0, Easy = 1, Medium = 2, Hard = 3, Insane = 4 };
        public static readonly char[] DifficultyIds = { '?', 'E', 'M', 'H', 'I' };

        public string GeneratedQualifiedName;
        public string Id = "(id)";
        public string Name = "(no name)";
        public LevelDifficulty Difficulty;
        public double StartTimeSeconds = 20;
        public double EliteCompletionTimeSeconds = 10;
        public LevelDataTransform PlayerTransform = new LevelDataTransform();
        public int TilesX;
        public int TilesY;
        public int TilesW;
        public int TilesH;
        public List<int> TileIndices = new List<int>();
        public List<LevelDataObject> Objects = new List<LevelDataObject>();
    }
}
