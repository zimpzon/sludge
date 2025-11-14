using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModRotation : SludgeModifier
    {
        public bool Active = true;
        public double RoundsPerSecond = 0.5;
        public double StartDegrees;
        public bool UseRealTime = false;

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

        private void Update()
        {
            if (!Active || !UseRealTime)
                return;

            double time = Time.realtimeSinceStartup;
            float rotation = (float)SludgeUtil.Stabilize((time * RoundsPerSecond * 360) + StartDegrees);
            trans.rotation = Quaternion.AngleAxis((float)rotation, Vector3.back);
        }

        public override void EngineTick()
        {
            if (!Active || UseRealTime)
                return;

            double time = GameManager.I.EngineTime;
            float rotation = (float)SludgeUtil.Stabilize((time * RoundsPerSecond * 360) + StartDegrees);
            trans.rotation = Quaternion.AngleAxis((float)rotation, Vector3.back);
        }
    }
}
