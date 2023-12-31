using Sludge.Easing;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModXCoord : SludgeModifier
    {
        public bool Active = true;
        [Range(0, 100)] public float Range = 5;
        [Range(0, 1)] public float CurrentlyAt = 0.5f;
        public double TimeMultiplier = 1.0;
        public bool PingPong = true;
        public Easings Easing = Easings.Linear;

        Transform trans;
        Vector3 startPos;

        Vector3 T0(Vector3 from) => from + Vector3.left * Range * CurrentlyAt;
        Vector3 T1(Vector3 from) => from + Vector3.right * Range * (1 - CurrentlyAt);

        private void OnDrawGizmos()
        {
            if (!Active)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(T0(transform.position), Vector3.one * 0.75f);
            Gizmos.DrawCube(T1(transform.position), Vector3.one * 0.75f);
            Gizmos.DrawLine(T0(transform.position), T1(transform.position));
        }

        public override void Reset()
        {
            trans = transform;
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

            double t = GameManager.I.EngineTime * TimeMultiplier + CurrentlyAt;
            t = SludgeUtil.TimeMod(t, PingPong);

            t = Ease.Apply(Easing, t);

            var pos = trans.position;
            pos.x = Mathf.Lerp(T0(startPos).x, T1(startPos).x, (float)t);
            transform.position = pos;
        }
    }
}
