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

        Rigidbody2D _rigidbody;
        Collider2D _collider;
        Transform trans;
        Vector3 startPos;

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
            _collider = GetComponent<Collider2D>();
        }

        public override void EngineTick()
        {
            if (!Active)
                return;

            double t = GameManager.I.EngineTime * TimeMultiplier + CurrentlyAt;
            t = SludgeUtil.TimeMod(t, PingPong);
            t = Ease.Apply(Easing, t);

            bool hasRigidbody = _rigidbody != null;

            Vector3 pos = hasRigidbody ? _rigidbody.position : trans.position;
            float newY = Mathf.Lerp(T0(startPos).y, T1(startPos).y, (float)t);

            if (!hasRigidbody)
            {
                transform.position = pos;
                return;
            }

            Vector3 newPos = new Vector3(pos.x, newY, pos.z);
            Vector3 moveVec = newPos - pos;
            // TODO: find where (if) we hit the player during the move

            _rigidbody.MovePosition(newPos);
        }
    }
}
