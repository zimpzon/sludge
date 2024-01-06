using DG.Tweening;
using Sludge.Utility;
using System;
using UnityEngine;

enum JumpState { NotSet, Grounded, AscendingActive, AscendingPassive, Descending }

class JumpStateParam
{
    public JumpState jumpState = JumpState.Descending;
    public Vector2 force;
    public bool isHoldingJump;
    public bool wasGroundedLastFrame;
    public int jumpHoldStartTime;

    public int forgivingJumpEndTime;
}

public class Player : MonoBehaviour
{
    private JumpStateParam JumpStateParam = new JumpStateParam();

    public enum PlayerSize { Small, Normal, Large };

    public static Vector3 Position;

    // forgiving jump: if player presses jump just before landing, execute jump anyways
    // airwalking: if the player jumps right after walking off a ledge, execute jump anyways
    // roof dodging: if a jump hits the roof, and there would have been room just to the left or side, slide and don't stop jump

    public float JumpHeight = 1.5f;
    public float JumpTimeToPeak = 0.5f;
    public float JumpTimeToDescend = 0.4f;
    public float JumpMaxHoldTime = 0.4f;
    public float MaxFallVelocity = 10.0f;
    public float AirControl = 0.25f;

    public int ForgivingJumpMs = 100;

    public float RunPeak = 10.0f;
    public float RunTimeToPeak = 0.15f;
    public float RunTimeToStop = 0.25f;

    public int RayCount = 32;
    public float RayScanRange = 5;

    float jumpVelocity;
    float jumpGravity;
    float fallGravity;

    float acceleration;
    float deceleration;

    Transform groundChecker;
    Transform headChecker;

    public bool IgnoreEnemies = false;

    public static int PositionSampleIdx;

    public ParticleSystem BodyDeathParticles;

    double deathScheduleTime;
    bool deathScheduled;
    float targetScale;
    float currentScale;

    public bool Alive = false;
    public int ExplodeParticleCount = 200;
    public float EyeScaleSurprised = 1.5f;
    public float DeathMiniDelay = 0.5f;
    [NonSerialized] public PlayerSize Size = PlayerSize.Normal;

    Transform trans;
    Vector3 homePos;
    Rigidbody2D physicsBody;
    int onConveyorBeltCount;
    Transform eyesTransform;
    Vector2 eyesBaseScale;
    SpriteRenderer[] childSprites;
    GameObject bodyRoot;
    Collider2D[] allColliders;
    float playerBaseScale;
    CircleCollider2D playerCollider;

    void Awake()
    {
        trans = transform;
        physicsBody = GetComponent<Rigidbody2D>();

        playerBaseScale = trans.localScale.x; // just assuming uniform scale
        groundChecker = SludgeUtil.FindByName(trans, "GroundChecker").transform;
        headChecker = SludgeUtil.FindByName(trans, "HeadChecker").transform;
        eyesTransform = SludgeUtil.FindByName(trans, "Body/Eyes");
        bodyRoot = SludgeUtil.FindByName(trans, "Body").gameObject;
        eyesBaseScale = eyesTransform.localScale;
        playerCollider = GetComponent<CircleCollider2D>();

        childSprites = GetComponentsInChildren<SpriteRenderer>();
        allColliders = GetComponentsInChildren<Collider2D>();

        jumpVelocity = (2.0f * JumpHeight) / JumpTimeToPeak;
        jumpGravity = (-2.0f * JumpHeight) / (JumpTimeToPeak * JumpTimeToPeak);
        fallGravity = (-2.0f * JumpHeight) / (JumpTimeToDescend * JumpTimeToDescend);

        acceleration = RunPeak / RunTimeToPeak;
        deceleration = RunPeak / RunTimeToStop;
    }

    public void Prepare()
    {
        onConveyorBeltCount = 0;
        PositionSampleIdx = 0;
        deathScheduleTime = float.MaxValue;
        deathScheduled = false;

        trans.localScale = Vector3.one * playerBaseScale;
        trans.position = homePos;
        currentScale = playerBaseScale;
        SetSize(PlayerSize.Normal);

        JumpStateParam = new JumpStateParam();

        eyesTransform.localScale = eyesBaseScale;

        bodyRoot.SetActive(true);
        SetAlpha(1.0f);

        Alive = true;
    }

    public void DisableCollisions(bool disable)
    {
        foreach (var col in allColliders)
            col.enabled = !disable;
    }

    public void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer child in childSprites)
        {
            Color col = child.color;
            col.a = alpha;
            child.color = col;
        }
    }

    public void ConveyourBeltEnter()
    {
    }

    public void ConveyourBeltExit()
    {
        onConveyorBeltCount--;
        // When resetting game colliderexits are fired after resetting player, so we get an exit event after setting onConveyorBeltCount to 0.
        if (onConveyorBeltCount < 0)
            onConveyorBeltCount = 0;
    }

    public void Teleport(Vector3 newPos)
    {
        GameManager.I.DustParticles.transform.position = trans.position;
        GameManager.I.DustParticles.Emit(10);
        GameManager.I.DustParticles.transform.position = newPos;
        GameManager.I.DustParticles.Emit(10);
    }

    public void AddPositionImpulse(double x, double y)
    {
    }

    public void SetHomePosition()
    {
        homePos = transform.position;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        bool harmlessHit = entity == EntityType.PlayerBullet ||
            entity == EntityType.Player ||
            entity == EntityType.Pickup ||
            entity == EntityType.BallCollector;

        if (harmlessHit)
        {
            return;
        }

        bool wallHit = entity == EntityType.FakeWall || entity == EntityType.StaticLevel;
        if (wallHit)
        {
            return;
        }
    }

    public void Kill(bool killedByWall = false)
    {
        if (killedByWall)
            return;

        if (IgnoreEnemies && !killedByWall)
            return;

        if (deathScheduled || !Alive)
            return;

        eyesTransform.localScale = eyesBaseScale * EyeScaleSurprised;
        deathScheduleTime = GameManager.I.EngineTime + DeathMiniDelay;
        deathScheduled = true;
    }

    public void ExecuteDelayedKill()
    {
        SoundManager.Play(FxList.Instance.PlayerDie);
        ParticleEmitter.I.EmitDust(trans.position, 12);
        GameManager.I.CameraRoot.DOKill();
        GameManager.I.CameraRoot.DOShakePosition(1.0f, 0.7f);

        EmitDeathExplosionParticles();
        bodyRoot.SetActive(false);
        trans.position = Vector3.one * 5544; // move out of the way

        Alive = false;
    }

    void EmitDeathExplosionParticles()
    {
        // Body particles
        BodyDeathParticles.Emit(ExplodeParticleCount);

        var main = BodyDeathParticles.main;
        var saveColor = main.startColor;

        // Leg particles
        main.startColor = Color.black;
        BodyDeathParticles.Emit(ExplodeParticleCount / 10);

        // Eye particles
        main.startColor = Color.white;
        BodyDeathParticles.Emit(ExplodeParticleCount / 20);

        main.startColor = saveColor;
    }

    bool IsFalling() => JumpStateParam.force.y < 0;
    bool IsGrounded() => Physics2D.OverlapCircle(groundChecker.position, 0.1f, SludgeUtil.ScanForWallsLayerMask);
    bool IsBumpingHead() => Physics2D.OverlapCircle(headChecker.position, 0.1f, SludgeUtil.ScanForWallsLayerMask);
    bool IsWithinForgivingJumpPeriod() => JumpStateParam.forgivingJumpEndTime > GameManager.I.EngineTimeMs;

    void JumpStateGrounded(JumpStateParam param)
    {
        if (param.jumpState != JumpState.Grounded) return;

        bool jumpTapped = GameManager.PlayerInput.IsTapped(Sludge.PlayerInputs.PlayerInput.InputType.Jump);
        bool allowForgivingJump = !param.wasGroundedLastFrame && IsWithinForgivingJumpPeriod();

        if (jumpTapped || allowForgivingJump)
        {
            if (allowForgivingJump && !jumpTapped)
                DebugLinesScript.Show("did ForgivingJump", Time.time);
            else
                DebugLinesScript.Show("did StandardJump", Time.time);

            param.force.y = jumpVelocity;
            param.jumpHoldStartTime = GameManager.I.EngineTimeMs;

            param.jumpState = JumpState.AscendingActive;
        }
    }

    void JumpStateAscendingActive(JumpStateParam param)
    {
        if (param.jumpState != JumpState.AscendingActive) return;

        bool jumpReleased = !GameManager.PlayerInput.JumpActive();
        if (jumpReleased)
        {
            param.jumpState = JumpState.AscendingPassive;
            return;
        }

        bool reachedMaxJumpHold = GameManager.I.EngineTimeMs - param.jumpHoldStartTime > JumpMaxHoldTime * 1000;
        if (reachedMaxJumpHold)
        {
            param.jumpState = JumpState.AscendingPassive;
            return;
        }

        if (IsBumpingHead())
        {
            param.jumpState = JumpState.Descending;
            return;
        }
    }

    void JumpStateAscendingPassive(JumpStateParam param)
    {
        if (param.jumpState != JumpState.AscendingPassive) return;

        if (IsFalling())
        {
            param.jumpState = JumpState.Descending;
            return;
        }

        if (IsBumpingHead())
        {
            param.jumpState = JumpState.Descending;
            return;
        }
    }

    void JumpStateDescending(JumpStateParam param)
    {
        if (param.jumpState != JumpState.Descending) return;

        if (IsGrounded())
        {
            param.jumpState = JumpState.Grounded;
            return;
        }
    }

    int hitsLeft = 0;
    int hitsRight = 0;
    int hitsUp = 0;
    int hitsDown = 0;

    void CheckRays()
    {
        float step = 360.0f / RayCount;
        for (int i = 0; i < RayCount; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, step * i) * Vector2.up;
            RaycastHit2D hit = Physics2D.Raycast(trans.position, dir, RayScanRange, SludgeUtil.ScanForWallsLayerMask);
            bool didHit = hit.collider != null;
            Debug.DrawRay(trans.position, dir * RayScanRange, didHit ? Color.red : Color.green, 0.1f);
        }
    }

    public void EngineTick()
    {
        if (!Alive)
            return;

        //CheckRays();

        if (deathScheduled)
        {
            bool deathTimeReached = GameManager.I.EngineTime >= deathScheduleTime;
            if (deathTimeReached)
            {
                deathScheduled = false;
                ExecuteDelayedKill();
            }
            return;
        }

        SetPositionSample();

        JumpStateGrounded(JumpStateParam);
        JumpStateAscendingActive(JumpStateParam);
        JumpStateAscendingPassive(JumpStateParam);
        JumpStateDescending(JumpStateParam);

        JumpStateParam.wasGroundedLastFrame = IsGrounded();

        int direction = 0;
        if (GameManager.PlayerInput.Left != 0)
        {
            direction = -1;
        }
        else if (GameManager.PlayerInput.Right != 0)
        {
            direction = 1;
        }

        DebugLinesScript.Show("JumpState", JumpStateParam.jumpState);
        DebugLinesScript.Show("force", JumpStateParam.force);
        DebugLinesScript.Show("isGrounded", IsGrounded());

        if (direction != 0)
        {
            // accelerate according to player input
            JumpStateParam.force.x += acceleration * direction * (float)GameManager.TickSize * AirControl;
            if (direction < 0)
            {
                JumpStateParam.force.x = Mathf.Clamp(JumpStateParam.force.x, -RunPeak, 0);
            }
            else
            {
                JumpStateParam.force.x = Mathf.Clamp(JumpStateParam.force.x, 0, RunPeak);
            }
        }
        else
        {
            // decelerate
            if (JumpStateParam.force.x < 0)
            {
                JumpStateParam.force.x += deceleration * (float)GameManager.TickSize * AirControl;
                JumpStateParam.force.x = Mathf.Min(JumpStateParam.force.x, 0);
            }
            else
            {
                JumpStateParam.force.x -= deceleration * (float)GameManager.TickSize * AirControl;
                JumpStateParam.force.x = Mathf.Max(JumpStateParam.force.x, 0);
            }
        }

        // update simulation
        if (JumpStateParam.jumpState != JumpState.AscendingActive && JumpStateParam.jumpState != JumpState.Grounded)
        {
            float gravity = JumpStateParam.force.y < 0 ? fallGravity : jumpGravity;
            JumpStateParam.force.y += gravity * (float)GameManager.TickSize;
        }

        JumpStateParam.force.y = Mathf.Max(JumpStateParam.force.y, -MaxFallVelocity);
        bool noForce = JumpStateParam.force.magnitude < 0.0001f;
        if (noForce)
        {
            return;
        }

        Vector2 moveStep = JumpStateParam.force * (float)GameManager.TickSize;
        Vector2 moveStepX = new Vector2(moveStep.x, 0);
        Vector2 moveStepY = new Vector2(0, moveStep.y);

        Vector2 acceptedNewPos = TryMove(moveStepX, physicsBody.position);
        acceptedNewPos = TryMove(moveStepY, acceptedNewPos);
        physicsBody.MovePosition(acceptedNewPos);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(xxx, playerCollider.radius);
    //}

    float GetPlayerColliderRadius() => playerCollider.radius * trans.localScale.x;

    Vector2 TryMove(Vector2 step, Vector2 from)
    {
        Vector2 newPos = from + step;
        //Debug.DrawRay(from, (newPos - from).normalized * 3, Color.yellow, 0.1f);
        float len = step.magnitude;

        int hitsFullMove = Physics2D.CircleCastNonAlloc(from, GetPlayerColliderRadius(), step.normalized, SludgeUtil.colliderHits, len, SludgeUtil.ScanForWallsLayerMask);
        if (hitsFullMove == 0)
        {
            return newPos;
        }

        // just before the actual hit
        float desiredDistance = SludgeUtil.colliderHits[0].distance - 0.01f;

        Vector2 validNewPos = from + step.normalized * desiredDistance;
        return validNewPos;
    }

    void SetSize(PlayerSize size)
    {
        Size = size;

        if (size == PlayerSize.Small)
        {
            targetScale = playerBaseScale * 0.75f;
        }
        else if (size == PlayerSize.Normal)
        {
            targetScale = playerBaseScale;
        }
        else if (size == PlayerSize.Large)
        {
            targetScale = playerBaseScale * 2.0f;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetSize(PlayerSize.Small);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetSize(PlayerSize.Normal);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SetSize(PlayerSize.Large);
        }

        trans.localScale = Vector3.one * currentScale;
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * 10);
    }

    void SetPositionSample(bool init = false)
    {
        if (!init && PositionSampleIdx > 0)
        {
            var prevPos = GameManager.PlayerSamples[PositionSampleIdx - 1].Pos;
            var dist = (trans.position - prevPos).magnitude;
            if (dist< 0.08f)
                return;
        }

        if (!init)
           PositionSampleIdx++;

        GameManager.PlayerSamples[PositionSampleIdx].Pos = trans.position;
    }
}
