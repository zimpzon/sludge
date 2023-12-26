using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModRotation : SludgeModifier
    {
        public bool Active = true;
        public double RoundsPerSecond = 0.5;
        public double StartDegrees;

        Transform trans;

        void Awake()
        {
            trans = transform;
        }

        public override void Reset()
        {
            if (Active)
                trans.rotation = Quaternion.AngleAxis((float)StartDegrees, Vector3.back);
        }

        public override void EngineTick()
        {
            if (!Active)
                return;

            float rotation = (float)SludgeUtil.Stabilize((GameManager.I.EngineTime * RoundsPerSecond * 360) + StartDegrees);
            trans.rotation = Quaternion.AngleAxis((float)rotation, Vector3.back);
        }
    }
}
