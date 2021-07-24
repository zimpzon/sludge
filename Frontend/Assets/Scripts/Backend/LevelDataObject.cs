using Sludge.Modifiers;
using System.Collections.Generic;

namespace Sludge.Shared
{
    public class LevelDataObject
    {
        public int ObjectIdx;
        public LevelDataTransform Transform;
        public List<string> Modifiers = new List<string>();
    }
}
