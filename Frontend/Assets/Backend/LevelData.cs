using System.Collections;
using System.Collections.Generic;

namespace Sludge.Shared
{
    public class LevelData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float StartTimeMs { get; set; }
        public float TimeMasterMs { get; set; }
        public int PlayerStartX { get; set; }
        public int PlayerStartY { get; set; }
    }
}
