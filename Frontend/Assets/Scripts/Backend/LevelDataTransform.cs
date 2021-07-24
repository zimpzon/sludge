using Sludge.Utility;
using UnityEngine;

namespace Sludge.Shared
{
    public class LevelDataTransform
    {
        public double PosX;
        public double PosY;
        public double ScaleX;
        public double ScaleY;
        public double RotZ;

        public static LevelDataTransform Get(Transform transform)
        {
            return new LevelDataTransform
            {
                PosX = SludgeUtil.Stabilize(transform.position.x),
                PosY = SludgeUtil.Stabilize(transform.position.y),
                ScaleX = SludgeUtil.Stabilize(transform.localScale.x),
                ScaleY = SludgeUtil.Stabilize(transform.localScale.y),
                RotZ = SludgeUtil.Stabilize(transform.rotation.eulerAngles.z),
            };
        }

        public void Set(Transform transform)
        {
            transform.position = new Vector3((float)PosX, (float)PosY, 0);
            transform.localScale = new Vector3((float)ScaleX, (float)ScaleY, 1);
            transform.rotation = Quaternion.Euler(0, 0, (float)RotZ);
        }
    }
}
