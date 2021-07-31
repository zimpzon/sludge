using System.Collections.Generic;

namespace Sludge.Shared
{
    public class LevelData
    {
        public string Id = "(id)";
        public string Name = "(no name)";
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
