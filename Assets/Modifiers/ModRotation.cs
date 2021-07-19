using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModRotation : SludgeModifier
    {
        public double RoundsPerSecond = 0.5;
        public double StartDegrees;

        Transform trans;

        void Awake()
        {
            trans = transform;
        }

        public override void EngineTick()
        {
            float rotation = (float)SludgeUtil.Stabilize((LevelManager.Instance.EngineTime * RoundsPerSecond * 360) + StartDegrees);
            trans.rotation = Quaternion.AngleAxis((float)rotation, Vector3.back);
        }
    }
}
