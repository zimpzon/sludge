using Sludge.SludgeObjects;
using Sludge.UI;
using System;
using UnityEngine;

namespace Sludge.Utility
{
    public static class SludgeUtil
    {
        public static double Stabilize(double d)
        {
            return d;
            // Clamp to 3 decimal places to avoid errors creeping up
            //long temp = (long)(d * 1000);
            //return temp / 1000.0;
        }

        public static Vector3 StabilizeVector(Vector3 v)
            => v;
            //=> new Vector3((float)Stabilize(v.x), (float)Stabilize(v.y), (float)Stabilize(v.z));

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

        public static void SetActiveRecursive(GameObject go, bool active)
        {
            go.SetActive(active);
            // Obsolete: should already be recursive
            //for (int i = 0; i < go.transform.childCount; ++i)
            //    go.transform.GetChild(i).gameObject.SetActive(active);
        }

        public static EntityType GetEntityType(GameObject go)
        {
            int goLayer = go.layer;
            if (goLayer == 0)
                return EntityType.Nothing;

            if (1 << goLayer == PlayerLayerMask)
                return EntityType.Player;

            if (1 << goLayer == StaticLevelLayerMask)
                return EntityType.StaticLevel;

            if (1 << goLayer == PickupLayerMask)
                return EntityType.Pickup;

            var sludgeObject = go.GetComponent<SludgeObject>();
            if (sludgeObject != null)
                return sludgeObject.EntityType;

            return EntityType.Unknown;
        }

        public static Transform FindByName(Transform trans, string name)
            => trans.Find(name) ?? throw new ArgumentException($"Child with name '{name}' not found in transform '{trans.name}'");

        public static void EnableEmission(ParticleSystem particles, bool enabled, bool clearParticles = true)
        {
            var emission = particles.emission;
            emission.enabled = enabled;

            if (clearParticles)
                particles.Clear();
        }

        public static long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }

        public static string ColorWrap(string s, Color col)
            => $"<color=#{ColorUtility.ToHtmlStringRGBA(col)}>{s}</color>";

        public static int ThrowableExplosionLayerMask = LayerMask.GetMask("Objects", "OutlinedObjects");
        public static int ScanForPlayerLayerMask = LayerMask.GetMask("StaticLevel", "OutlinedObjects", "Player");
        public static int ScanForWallsLayerMask = LayerMask.GetMask("StaticLevel", "OutlinedObjects");
        public static int PlayerLayerMask = LayerMask.GetMask("Player");
        public static int StaticLevelLayerMask = LayerMask.GetMask("StaticLevel");
        public static int PickupLayerMask = LayerMask.GetMask("Pickups");
        public static int WallsAndObjectsLayerMask = LayerMask.GetMask("StaticLevel", "OutlinedObjects", "Objects");

        public static int OutlinedLayerNumber = LayerMask.NameToLayer("OutlinedObjects");
        public static int ObjectsLayerNumber = LayerMask.NameToLayer("Objects");
    }
}
