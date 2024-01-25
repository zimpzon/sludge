using Sludge.Colors;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class KidLogicMod : SludgeModifier, IConveyorBeltPassenger
{
    class S
    {
        public int onConveyorBeltCount;
        public float forceY;
        public Vector2 impulse;
        public double deathScheduleTime;
        public bool deathScheduled;
        public bool Alive = true;
        public float eyeScale = 1.0f;
        public float eyeScaleTarget = 1.0f;
    }

    public Transform TargetTransform;
    public float fallGravity = 4.0f;
    public float maxVelocity = 15.0f;
    public float EyeScaleSurprised = 1.5f;
    float DeathMiniDelay = 0.1f;

    public float nudgeForce = 1f;     // Adjust this value to control the nudge strength
    public float attemptInterval = 1f; // Time interval in seconds between attempts to upright itself
    private Rigidbody2D rb;
    private float nextAttemptTime = 0f;

    S s = new S();
    Transform trans;
    Vector3 basePos;
    Quaternion baseRotation;
    CircleCollider2D squashedCollider; // a smaller collider used to detect squashed between moving walls
    Transform eyesTransform;
    Vector2 eyesBaseScale;

    private Rigidbody2D _rigidbody;

    public override void OnLoaded()
    {
        trans = transform;
        rb = GetComponent<Rigidbody2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        squashedCollider = SludgeUtil.FindByName(trans, "SquashedCollider").GetComponent<CircleCollider2D>();
        eyesTransform = SludgeUtil.FindByName(trans, "Body/Eyes");
        eyesBaseScale = eyesTransform.localScale;

        basePos = transform.position;
        baseRotation = transform.rotation;
    }

    public override void Reset()
    {
        s = new S();
        gameObject.SetActive(true);
        transform.position = basePos;
        transform.rotation = baseRotation;
        eyesTransform.localScale = eyesBaseScale;

        base.Reset();
    }

    public void AddConveyorPulse(Vector2 pulse)
    {
        s.impulse += pulse;
    }

    public void OnConveyorBeltEnter(Vector2 beltDirection)
    {
        s.onConveyorBeltCount++;
        _rigidbody.gravityScale = 0;
    }

    public void OnConveyorBeltExit(Vector2 beltDirection)
    {
        s.onConveyorBeltCount--;
        _rigidbody.gravityScale = 1;

        s.impulse = Vector2.zero;
        _rigidbody.velocity = beltDirection.normalized * maxVelocity;
    }

    private void AttemptToUpright()
    {
        // Calculate the angle difference from upright position
        float angle = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0));

        // Check if the square is on its side or worse
        if (angle > 45f)
        {
            // Determine the side of the square to apply the force
            Vector2 forceDirection = angle > 0 ? Vector2.right : Vector2.left;
            Vector2 forcePoint = rb.position + (forceDirection * (rb.GetComponent<Collider2D>().bounds.extents.x));

            float force = angle > 120f ? nudgeForce * 2.0f : nudgeForce;
            // Apply an upward force at the determined side of the square
            rb.AddForceAtPosition(Vector2.up * force, forcePoint, ForceMode2D.Impulse);
        }
    }

    void UpdateEyes()
    {
        var playerDir = Player.Position - trans.position;
        float sqrPlayerDist = playerDir.sqrMagnitude;
        playerDir.Normalize();

        const float SqrLookRange = 8 * 8;
        const float MaxScale = 0.9f;

        s.eyeScaleTarget = sqrPlayerDist < SqrLookRange ? MaxScale : MaxScale * 0.9f;

        s.eyeScale += (float)((s.eyeScaleTarget > s.eyeScale) ? GameManager.TickSize * 4.0f : -GameManager.TickSize * 4.0f);
        s.eyeScale = Mathf.Clamp(s.eyeScale, 0, MaxScale);

        bool doBlink = Random.value < (1 / 200.0);
        if (doBlink)
            s.eyeScale = 0;

        //pupil.localPosition = eyeScale < 0.2f ? Vector2.one * 10000 : new Vector2(playerDir.x * 0.1f, playerDir.y * 0.08f * MaxScale);
    }

    public override void EngineTick()
    {
        if (!s.Alive)
            return;

        if (s.deathScheduled)
        {
            bool deathTimeReached = GameManager.I.EngineTime >= s.deathScheduleTime;
            if (deathTimeReached)
            {
                s.deathScheduled = false;
                ExecuteDelayedKill();
            }
            return;
        }

        if (s.onConveyorBeltCount > 0)
        {
            _rigidbody.AddForce(s.impulse * (float)GameManager.TickSize, ForceMode2D.Impulse);
        }
        else
        {
            _rigidbody.AddForce(Vector2.down * (float)GameManager.TickSize, ForceMode2D.Impulse);
        }

        s.impulse = Vector2.zero;
        CheckSquashed();

        // Check if it's time to try to upright itself
        bool isStill = rb.velocity.magnitude < 0.1f && Mathf.Abs(rb.angularVelocity) < 0.1f;
        if (Time.time >= nextAttemptTime && isStill)
        {
            AttemptToUpright();
            nextAttemptTime = Time.time + attemptInterval;
        }
    }

    private void CheckSquashed()
    {
        if (GameManager.I.FrameCounter == 0) // Hacky hacky: EngineTick gets called once before starting round. could we be starting in a wall?
            return;

        int hits = Physics2D.OverlapCollider(squashedCollider, SludgeUtil.ScanForWallFilter, SludgeUtil.colliderHits);
        bool wasSquished = hits > 0;
        if (wasSquished)
        {
            Kill();
        }
    }

    public void OnArmedEnergyHit()
    {
        Kill();
    }

    public void Kill()
    {
        if (s.deathScheduled || !s.Alive)
            return;

        eyesTransform.localScale = eyesBaseScale * EyeScaleSurprised;
        s.deathScheduleTime = GameManager.I.EngineTime + DeathMiniDelay;
        s.deathScheduled = true;
    }

    public void ExecuteDelayedKill()
    {
        SoundManager.Play(FxList.Instance.PlayerDie);
        ParticleEmitter.I.EmitDust(trans.position, 4);
        GameManager.I.ShakeCamera(duration: 1.0f, strength: 0.4f);

        Player.I.EmitDeathExplosionParticles(trans.position, ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor.Player), scale: 0.5f);

        gameObject.SetActive(false);
        trans.position = Vector3.one * 4432; // move out of the way

        s.Alive = false;
    }
}
