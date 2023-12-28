using System.Collections.Generic;

namespace Sludge.Shared
{
    public class LevelTilemapData
    {
        public int TilesX;
        public int TilesY;
        public int TilesW;
        public int TilesH;
        public List<int> TileIndices = new List<int>();
    }

    public class LevelData
    {
        public enum LevelDifficulty { NotSet = 0, Easy = 1, Medium = 2, Hard = 3, Insane = 4 };
        public static readonly char[] DifficultyIds = { '?', 'E', 'M', 'H', 'I' };

        public string UniqueId;
        public string ColorSchemeName;
        public int SortKey = 999999;
        public string Name = "(no name)";
        public LevelDifficulty Difficulty;
        public double EliteCompletionTimeSeconds = 10;
        public LevelDataTransform PlayerTransform = new LevelDataTransform();
        public LevelTilemapData WallTilemap = new LevelTilemapData();
        public LevelTilemapData PillTilemap = new LevelTilemapData();
        public List<LevelDataObject> Objects = new List<LevelDataObject>();
    }
}
