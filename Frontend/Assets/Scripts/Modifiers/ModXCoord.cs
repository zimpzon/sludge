using Sludge.Easing;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModXCoord : SludgeModifier
    {
        public bool Active = true;
        public float Range = 5;
        public double TimeOffset = 0.0;
        public double TimeMultiplier = 1.0;
        public bool PingPong = true;
        public Easings Easing = Easings.Linear;

        Vector3 startPos;

        void Awake()
        {
            startPos = transform.position;
        }

        public override void EngineTick()
        {
            if (!Active)
                return;

            double t = SludgeUtil.TimeMod(GameManager.Instance.EngineTime * TimeMultiplier + TimeOffset);
            t = Ease.Apply(Easing, t);
            if (PingPong)
                t = Ease.PingPong(t);

            transform.position = startPos + Vector3.right * ((float)t * Range);
        }
    }
}
