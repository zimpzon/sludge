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

        public static Vector2 LookAngle(double angle)
        {
            float x = -(float)Stabilize(Mathf.Sin((float)(Mathf.Deg2Rad * angle)));
            float y = (float)Stabilize(Mathf.Cos((float)(Mathf.Deg2Rad * angle)));
            return new Vector2(x, y);
        }

        public static int ScanForPlayerLayerMask = LayerMask.GetMask("StaticLevel", "OutlinedObjects", "Player");
        public static int ScanForWallsLayerMask = LayerMask.GetMask("StaticLevel", "OutlinedObjects");
        public static int PlayerLayerMask = LayerMask.GetMask("Player");
        public static int StaticLevelLayerMask = LayerMask.GetMask("StaticLevel");
        public static int OutlinedLayerNumber = LayerMask.NameToLayer("OutlinedObjects");
        public static int ObjectsLayerNumber = LayerMask.NameToLayer("Objects");
    }
}
