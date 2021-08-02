using Sludge.Easing;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModYCoord : SludgeModifier
    {
        public bool Active = true;
        public float Range = 5;
        public double TimeOffset = 0.0;
        public double TimeMultiplier = 1.0;
        public bool PingPong = true;
        public Easings Easing = Easings.Linear;

        Transform trans;
        Vector3 startPos;

        void Awake()
        {
            startPos = transform.position;
            trans = transform;
        }

        public override void EngineTick()
        {
            if (!Active)
                return;

            double t = SludgeUtil.TimeMod((GameManager.Instance.EngineTime + TimeOffset) * TimeMultiplier);
            t = Ease.Apply(Easing, t);
            if (PingPong)
                t = Ease.PingPong(t);

            double offsetY = SludgeUtil.Stabilize(t * Range);
            var pos = trans.position;
            pos.y = startPos.y + (float)offsetY;
            transform.position = pos;
        }
    }
}
