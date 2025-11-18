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
        public bool UseExistingRotation = false;

        Transform trans;
        double initialRotation;

        void Awake()
        {
            trans = transform;
            initialRotation = trans.eulerAngles.z;
        }

        public override void Reset()
        {
            if (Active)
            {
                double startAngle = UseExistingRotation ? initialRotation : StartDegrees;
                trans.rotation = Quaternion.AngleAxis((float)startAngle, Vector3.back);
            }
        }

        private void Update()
        {
            if (!Active || !UseRealTime)
                return;

            double time = Time.realtimeSinceStartup;
            double startAngle = UseExistingRotation ? initialRotation : StartDegrees;
            float rotation = (float)SludgeUtil.Stabilize((time * RoundsPerSecond * 360) + startAngle);
            trans.rotation = Quaternion.AngleAxis((float)rotation, Vector3.back);
        }

        public override void EngineTick()
        {
            if (!Active || UseRealTime)
                return;

            double time = GameManager.I.EngineTime;
            double startAngle = UseExistingRotation ? initialRotation : StartDegrees;
            float rotation = (float)SludgeUtil.Stabilize((time * RoundsPerSecond * 360) + startAngle);
            trans.rotation = Quaternion.AngleAxis((float)rotation, Vector3.back);
        }
    }
}
