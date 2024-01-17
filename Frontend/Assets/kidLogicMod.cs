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
    }

    public Transform TargetTransform;
    public float fallGravity = 4.0f;
    public float maxVelocity = 15.0f;
    public float EyeScaleSurprised = 1.5f;
    public float DeathMiniDelay = 0.5f;

    S s = new S();
    Transform trans;
    Vector3 basePos;
    Quaternion baseRotation;
    CircleCollider2D squashedCollider; // a smaller collider used to detect squashed between moving walls
    Transform eyesTransform;
    Vector2 eyesBaseScale;

    private Rigidbody2D _rigidbody;

    void Awake()
    {
        trans = transform;
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

        _rigidbody.AddForce(s.impulse * (float)GameManager.TickSize, ForceMode2D.Impulse);
        s.impulse = Vector2.zero;

        CheckSquashed();
    }

    private void CheckSquashed()
    {
        int hits = Physics2D.OverlapCollider(squashedCollider, SludgeUtil.ScanForWallFilter, SludgeUtil.colliderHits);
        bool wasSquished = hits > 0;
        if (wasSquished)
        {
            Kill();
        }
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
        GameManager.I.CameraRoot.DOKill();
        GameManager.I.CameraRoot.DOShakePosition(1.0f, 0.4f);

        Player.I.EmitDeathExplosionParticles(trans.position, ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor.Player), scale: 0.5f);

        gameObject.SetActive(false);
        trans.position = Vector3.one * 4432; // move out of the way

        s.Alive = false;
    }
}
