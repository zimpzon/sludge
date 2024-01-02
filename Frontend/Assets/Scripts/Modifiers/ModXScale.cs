using Sludge.Easing;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModXScale : SludgeModifier
    {
        public bool Active = true;
        [Range(0, 5)] public float From = 1;
        [Range(0, 5)] public float To = 1;
        public double TimeMultiplier = 1.0;
        public double TimeOffset = 0.0;
        public bool PingPong = true;
        public Easings Easing = Easings.Linear;

        Transform trans;
        Vector3 startScale;

        public override void Reset()
        {
            trans = transform;
            trans.position = startScale;
        }

        public override void OnLoaded()
        {
            trans = transform;
            startScale = trans.localScale;
        }

        public override void EngineTick()
        {
            if (!Active)
                return;

            double t = GameManager.I.EngineTime * TimeMultiplier + TimeOffset;
            t = SludgeUtil.TimeMod(t, PingPong);

            t = Ease.Apply(Easing, t);

            float range = To - From;
            float value = From + range * (float)t;

            var scale = trans.localScale;
            scale.x = startScale.x * value;
            transform.localScale = scale;
        }
    }
}
