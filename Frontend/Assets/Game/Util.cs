using UnityEngine;

namespace Sludge.Utility
{
    public static class SludgeUtil
    {
        public static double Stabilize(double d)
        {
            // Clamp to 3 decimal places to avoid errors creeping up
            long temp = (long)(d * 1000);
            return temp / 1000.0;
        }

        public static double TimeMod(double time)
            => time - (int)time;

        public static int ScanForPlayerLayerMask = LayerMask.GetMask("StaticLevel", "DynamicBlocks", "Player");
        public static int PlayerLayerMask = LayerMask.GetMask("Player");
        public static int StaticLevelLayerMask = LayerMask.GetMask("StaticLevel");
    }
}
