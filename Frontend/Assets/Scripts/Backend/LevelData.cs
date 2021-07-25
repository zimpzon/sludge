using System.Collections.Generic;

namespace Sludge.Shared
{
    public class LevelData
    {
        public string Id;
        public string Name;
        public double StartTimeSeconds;
        public double EliteCompletionTimeSeconds;
        public LevelDataTransform PlayerTransform;
        public int TilesX;
        public int TilesY;
        public int TilesW;
        public int TilesH;
        public List<int> TileIndices = new List<int>();
        public List<LevelDataObject> Objects = new List<LevelDataObject>();
    }
}
