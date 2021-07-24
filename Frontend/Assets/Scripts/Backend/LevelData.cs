using System.Collections.Generic;
using UnityEngine;

namespace Sludge.Shared
{
    public class LevelData
    {
        public string Id;
        public string Name;
        public double StartTimeSeconds;
        public double EliteCompletionTimeSeconds;
        public double PlayerX;
        public double PlayerY;
        public double PlayerAngle;
        public Vector3Int TileBounds;
        public List<int> TileIndices = new List<int>();
    }
}
