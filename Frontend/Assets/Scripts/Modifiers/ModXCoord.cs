using Sludge.Easing;
using Sludge.Utility;
using UnityEngine;
namespace Sludge.Modifiers
{
    public class ModXCoord : SludgeModifier
    {
        [Header("Movement Settings")]
        public bool Active = true;
        [Tooltip("Starting normalized position (0-1) along movement range")]
        [Range(0f, 1f)]
        public float StartT = 0f;
        [Tooltip("Half-range of X movement from baseX")]
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
            // Map t=0..1 to -ScaleT..+ScaleT around baseX
            float newX = basePos.x + (t - 0.5f) * 2f * ScaleT;
            return new Vector3(newX, basePos.y, basePos.z);
        }
        private void SetPosition()
        {
            float t = GetT();
            Vector3 newPos = GetTargetPosition(t);
            trans.position = newPos;
        }
        public override void EngineTick()
        {
            if (!Active) return;
            SetPosition();
        }
    }
}