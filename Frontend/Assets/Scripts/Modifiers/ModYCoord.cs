using Sludge.Easing;
using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModYCoord : SludgeModifier
    {
        public bool Active = true;
        [Range(0, 100)] public float Range = 5;
        [Range(0, 1)] public float CurrentlyAt = 0.5f;
        public double TimeMultiplier = 1.0;
        public bool PingPong = true;
        public Easings Easing = Easings.Linear;
        public float GizmoTime = 0.0f;

        Transform trans;
        Vector3 startPos;
        Rigidbody2D _rigidbody;

        Vector3 T0(Vector3 from) => from + Vector3.up * Range * CurrentlyAt;
        Vector3 T1(Vector3 from) => from + Vector3.down * Range * (1 - CurrentlyAt);

        private void OnDrawGizmos()
        {
            if (!Active)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(T0(transform.position), Vector3.one * 0.75f);
            Gizmos.DrawCube(T1(transform.position), Vector3.one * 0.75f);
            Gizmos.DrawLine(T0(transform.position), T1(transform.position));

            float t = GetT(GizmoTime);
            float gizmoY = Mathf.Lerp(T0(transform.position).y, T1(transform.position).y, t);
            Gizmos.DrawCube(transform.position - Vector3.up * gizmoY, Vector3.one * 0.75f);
        }

        private void OnValidate()
        {
            startPos = transform.position;
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
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        float GetT(float time)
        {
            double t = time * TimeMultiplier + CurrentlyAt;
            t = SludgeUtil.TimeMod(t, PingPong);

            t = Ease.Apply(Easing, t);
            return (float)t;
        }

        public override void EngineTick()
        {
            if (!Active)
                return;

            float t = GetT((float)GameManager.I.EngineTime);
            bool hasRigidbody = _rigidbody != null;

            Vector3 pos = hasRigidbody ? _rigidbody.position : trans.position;
            float newY = Mathf.Lerp(T0(startPos).y, T1(startPos).y, (float)t);

            Vector3 newPos = new Vector3(pos.x, newY, pos.z);

            if (!hasRigidbody)
            {
                transform.position = newPos;
                return;
            }

            _rigidbody.MovePosition(newPos);
        }
    }
}
