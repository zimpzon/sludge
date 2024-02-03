using DG.Tweening;
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
        public int uprightAttempts;
        public float idleTime;
    }

    public Transform TargetTransform;
    public float fallGravity = 4.0f;
    public float maxVelocity = 15.0f;
    public float EyeScaleSurprised = 1.5f;
    float DeathMiniDelay = 0.1f;

    Transform _speechTrans;
    public float nudgeForce = 1f;     // Adjust this value to control the nudge strength
    public float attemptInterval = 1f; // Time interval in seconds between attempts to upright itself
    private Rigidbody2D rb;
    private Collider2D _collider;
    private float nextUprightAttemptTime = 0f;

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
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        squashedCollider = SludgeUtil.FindByName(trans, "SquashedCollider").GetComponent<CircleCollider2D>();
        eyesTransform = SludgeUtil.FindByName(trans, "Body/Eyes");
        eyesBaseScale = eyesTransform.localScale;
        _speechTrans = SludgeUtil.FindByName(trans, "Speech");
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
        _speechTrans.gameObject.SetActive(false);

        base.Reset();
        ShowSpeech();
    }

    void ShowSpeech()
    {
        _speechTrans.gameObject.SetActive(true);
        _speechTrans.gameObject.transform.localScale = Vector3.zero;
        _speechTrans.DOScale(1.0f, 0.25f).SetEase(Ease.InCubic);
        _speechTrans.DOScale(0.0f, 0.25f).SetEase(Ease.InCubic).SetDelay(3.0f);
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
        float angle = Mathf.DeltaAngle(transform.eulerAngles.z, 0);
        float absAngle = Mathf.Abs(angle);

        // Determine the side of the square to apply the force
        Vector2 forceDirection = angle > 0 ? Vector2.right : Vector2.left;
        Vector2 forcePoint = rb.position + (forceDirection * _collider.bounds.extents.x);

        float force = absAngle > 120f ? nudgeForce * 2.0f : nudgeForce;
        force *= 1.0f + Mathf.Min(s.uprightAttempts * 0.1f, 1.3f);
        rb.AddForceAtPosition(Vector2.up * force, forcePoint, ForceMode2D.Impulse);
        if (s.uprightAttempts++ > 4)
        {
            s.uprightAttempts = 0;
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

        //bool isStill = rb.velocity.magnitude < 0.2f;
        //bool isNearGround = Physics2D.Raycast(rb.position, Vector2.down, SludgeUtil.ScanForWallsLayerMask);
        //Debug.DrawLine(rb.position, rb.position + Vector2.down, isNearGround ? Color.green : Color.yellow, 0.2f);

        //float absAngle = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0));
        //bool isVertical = absAngle < 2;
        //bool isUpright = absAngle < 45;

        //_speechTrans.gameObject.SetActive(isNearGround && isStill && isVertical);

        //s.idleTime += isStill && isUpright ? (float)GameManager.TickSize : 0;
        //DebugLinesScript.Show("IdleTime", s.idleTime);

        //if (isUpright)
        //    s.uprightAttempts = 0;

        //if (!isUpright && isStill && Time.time >= nextUprightAttemptTime)
        //{
        //    s.idleTime = 0;
        //    AttemptToUpright();
        //    nextUprightAttemptTime = Time.time + attemptInterval;
        //}
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
