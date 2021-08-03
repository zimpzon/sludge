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

        Transform trans;
        Vector3 startPos;

        public override void Reset()
        {
            trans.position = startPos;
        }

        public override void OnLoaded()
        {
            trans = transform;
            startPos = trans.position;
        }

        public override void EngineTick()
        {
            if (!Active)
                return;

            double t = SludgeUtil.TimeMod((GameManager.Instance.EngineTime + TimeOffset) * TimeMultiplier);
            t = Ease.Apply(Easing, t);
            if (PingPong)
                t = Ease.PingPong(t);

            double offsetX = SludgeUtil.Stabilize(t * Range);
            var pos = trans.position;
            pos.x = startPos.x + (float)offsetX;
            transform.position = pos;
        }
    }
}
