using Sludge.Utility;
using System.Collections;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModTumblerLogic : SludgeModifier
    {
        enum State { LookForPlayer, WarmUp, Move };

        public Transform Eye;
        public Transform Pupil;
        public Transform BodyRoot;

        SpriteRenderer spikeRenderer;
        IEnumerator lookForPlayer;
        IEnumerator warmUp;
        IEnumerator move;
        Vector3 moveDir;
        Transform trans;
        RaycastHit2D[] scanHits = new RaycastHit2D[1];
        ContactFilter2D scanForPlayerFilter = new ContactFilter2D();
        Vector3 homePos;
        double x;
        double y;
        double rotation;
        double rotationSpeed;
        double speed = 15.0;
        State state;
        float eyeScale;
        float eyeScaleTarget;

        void Awake()
        {
            trans = transform;
            scanForPlayerFilter.SetLayerMask(SludgeUtil.ScanForPlayerLayerMask);
            spikeRenderer = transform.Find("BodyRoot/Spikes").gameObject.GetComponent<SpriteRenderer>();
        }

        public override void OnLoaded()
        {
            trans = transform;
            homePos = trans.position;
        }

        public override void Reset()
        {
            x = SludgeUtil.Stabilize(homePos.x);
            y = SludgeUtil.Stabilize(homePos.y);
            rotation = 0;
            rotationSpeed = 0;
            spikeRenderer.enabled = false;
            moveDir = Vector3.zero;
            state = State.LookForPlayer;

            lookForPlayer = WaitForPlayerInSight();
            warmUp = Warmup();
            move = Move();
            UpdateTransform();
            eyeScale = 0;
            eyeScaleTarget = 0;
        }

        public override void EngineTick()
        {
            UpdateEye();

            switch (state)
            {
                case State.LookForPlayer: lookForPlayer.MoveNext(); break;
                case State.WarmUp: warmUp.MoveNext(); break;
                case State.Move: move.MoveNext(); break;
            }
        }

        void UpdateEye()
        {
            var playerDir = Player.Position - trans.position;
            float sqrPlayerDist = playerDir.sqrMagnitude;
            playerDir.Normalize();

            const float SqrLookRange = 8 * 8;
            const float MaxScale = 0.9f;
            eyeScaleTarget = sqrPlayerDist < SqrLookRange ? MaxScale : 0;
            if (state == State.Move)
                eyeScaleTarget = MaxScale;

            eyeScale += (float)((eyeScaleTarget > eyeScale) ? GameManager.TickSize * 4.0f : -GameManager.TickSize * 4.0f);
            eyeScale = Mathf.Clamp(eyeScale, 0, MaxScale);

            Pupil.localPosition = eyeScale < 0.2f ? Vector2.one * 10000 : new Vector2(playerDir.x * 0.1f, playerDir.y * 0.08f * MaxScale);

            Eye.transform.localScale = new Vector2(1, eyeScale);
        }

        void UpdateTransform()
        {
            BodyRoot.rotation = Quaternion.AngleAxis((float)rotation, Vector3.back);
            trans.position = new Vector3((float)x, (float)y, 0);
        }

        IEnumerator Warmup()
        {
            spikeRenderer.enabled = true;

            int iterations = 40;
            double step = 15;
            while (state == State.WarmUp && iterations-- > 0)
            {
                rotation += rotationSpeed * GameManager.TickSize;
                UpdateTransform();
                rotationSpeed += step;
                yield return null;
            }
            state = State.Move;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (1 << collision.gameObject.layer != SludgeUtil.PlayerLayerMask)
                Reset();
        }

        IEnumerator Move()
        {
            while (state == State.Move)
            {
                x += speed * GameManager.TickSize * moveDir.x;
                y += speed * GameManager.TickSize * moveDir.y;
                x = SludgeUtil.Stabilize(x);
                y = SludgeUtil.Stabilize(y);
                rotation += rotationSpeed * GameManager.TickSize;
                UpdateTransform();

                yield return null;
            }
        }

        IEnumerator WaitForPlayerInSight()
        {
            while (state == State.LookForPlayer)
            {
                var playerDir = (Player.Position - trans.position);
                playerDir.x = (float)SludgeUtil.Stabilize(playerDir.x);
                playerDir.y = (float)SludgeUtil.Stabilize(playerDir.y);

                // No reason to scan if not close to axis aligned
                float absDistanceX = Mathf.Abs(playerDir.x);
                float absDistanceY = Mathf.Abs(playerDir.y);
                bool closeToXAxis = absDistanceY < 1;
                bool closeToYAxis = absDistanceX < 1;

                if (closeToXAxis || closeToYAxis)
                {
                    Physics2D.CircleCast(trans.position, 0.1f, playerDir, scanForPlayerFilter, scanHits);
                    int hitMask = 1 << scanHits[0].transform.gameObject.layer;
                    bool hasLoS = hitMask == SludgeUtil.PlayerLayerMask;
                    Debug.DrawLine(trans.position, scanHits[0].point, hasLoS ? Color.green : Color.red);
                    if (hasLoS)
                    {
                        moveDir = playerDir;
                        if (closeToXAxis)
                            moveDir.y = 0;
                        else
                            moveDir.x = 0;

                        moveDir.Normalize();
                        state = State.WarmUp;
                    }
                }
                yield return null;
            }
        }
    }
}
