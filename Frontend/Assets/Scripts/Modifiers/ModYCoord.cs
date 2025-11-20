using Sludge.Easing;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModYCoord : SludgeModifier
    {
        [Header("Movement Settings")]
        public bool Active = true;

        [Tooltip("Starting normalized position (0-1) along movement range")]
        [Range(0f, 1f)]
        public float StartT = 0f;

        [Tooltip("Half-range of Y movement from baseY")]
        public float ScaleT = 5f;

        [Tooltip("Time multiplier for movement speed")]
        public float TimeMultiplier = 1f;

        public bool PingPong = true;
        public Easings Easing = Easings.Linear;

        private Transform trans;
        private Vector3 basePos;

        public override void OnLoaded()
        {
            if (!Active) return;

            trans = transform;
            basePos = trans.position;

            SetPosition();
        }

        private float GetT()
        {
            if (!Active) return 0f;
            if (GameManager.I == null)
                return 0;

            // Start at StartT, apply time multiplier
            double t = GameManager.I.EngineTime * TimeMultiplier + StartT;

            // Apply pingpong wrap
            t = SludgeUtil.TimeMod(t, PingPong);

            // Apply easing
            t = Ease.Apply(Easing, t);

            return (float)t;
        }

        private Vector3 GetTargetPosition(float t)
        {
            // Map t=0..1 to -ScaleT..+ScaleT around baseY
            float newY = basePos.y + (t - 0.5f) * 2f * ScaleT;
            return new Vector3(basePos.x, newY, basePos.z);
        }

        private void SetPosition()
        {
            float t = GetT();
            Vector3 targetPos = GetTargetPosition(t);
            Vector3 currentPos = trans.position;
            trans.position = new Vector3(currentPos.x, targetPos.y, currentPos.z);
        }

        public override void EngineTick()
        {
            if (!Active) return;
            SetPosition();
        }
    }
}
