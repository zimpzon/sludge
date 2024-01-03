using System.Collections.Generic;
using UnityEngine;

namespace Sludge.Shared
{
    public class LevelDataObject
    {
        public int ObjectIdx;
        public LevelDataTransform Transform;
        public string CustomData;
        public List<string> Modifiers = new List<string>();
    }
}
