﻿using UnityEngine;

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

        public static double AngleNormalized0To360(double angle)
        {
            angle = angle % 360;

            if (angle < 0)
                angle += 360;

            return angle;
        }

        public static Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
        {
            Vector3 line_direction = line_end - line_start;
            float line_length = line_direction.magnitude;
            line_direction.Normalize();
            float project_length = Mathf.Clamp(Vector3.Dot(point - line_start, line_direction), 0f, line_length);
            return line_start + line_direction * project_length;
        }

        public static Vector3 GetClosestPointOnInfiniteLine(Vector3 point, Vector3 line_start, Vector3 line_end)
        {
            return line_start + Vector3.Project(point - line_start, line_end - line_start);
        }

        public static int ScanForPlayerLayerMask = LayerMask.GetMask("StaticLevel", "OutlinedObjects", "Player");
        public static int ScanForWallsLayerMask = LayerMask.GetMask("StaticLevel", "OutlinedObjects");
        public static int PlayerLayerMask = LayerMask.GetMask("Player");
        public static int StaticLevelLayerMask = LayerMask.GetMask("StaticLevel");
        public static int OutlinedLayerNumber = LayerMask.NameToLayer("OutlinedObjects");
        public static int ObjectsLayerNumber = LayerMask.NameToLayer("Objects");
    }
}
