using Assets.Scripts.Game;
using DG.Tweening;
using Sludge.Utility;
using System;
using UnityEngine;

public enum JumpState { NotSet, AscendingActive, AscendingPassive, Gravity }

public class StateParam
{
    public JumpState jumpState = JumpState.Gravity;

    public MutatorJumpType jumpType = MutatorJumpType.SingleJump;
    public MutatorTypePlayerSize playerSize = MutatorTypePlayerSize.DefaultMe;

    public Vector2 force;
    public Vector2 impulse;
    public bool isHoldingJump;
    public int airJumpsLeft = 0;

    public int jumpHoldStartTime = int.MaxValue;
    public int coyoteJumpEndTime = int.MinValue;
    public int queuedJumpEndTime = int.MinValue;
    public int disableHorizontalDirectionEndTime = int.MinValue;
    public int disabledHorizontalDirection = 0;

    public bool isHuggingLeftWall;
    public bool isHuggingRightWall;
    public bool isDescending;
    public bool isWallSliding;
}

public class Player : MonoBehaviour
{
    public StateParam StateParam = new StateParam();

    public enum PlayerSize { Small, Normal, Large };

    public static Vector3 Position;

    public AnimationClip AnimMoveLeft;
    public AnimationClip AnimMoveRight;
    public AnimationClip AnimIdle;
    string currentAnim;

    public bool ShowDebug = false;
    public float JumpHeight = 1.25f;
    public float JumpTimeToPeak = 0.3f;
    public float JumpTimeToDescend = 0.25f;
    public float JumpMaxHoldTime = 0.2f;
    public float MaxVelocity = 15.0f;
    float WallSlideMaxFall = 4.0f;
    public int WallJumpDisableHorizontalBreakingMs = 100;
    public float AirControl = 0.25f;
    public int CoyoteJumpMs = 200;
    public int QueuedJumpMs = 200;

    public float RunPeak = 10.0f;
    public float RunTimeToPeak = 0.15f;
    public float RunTimeToStop = 0.25f;

    public float WallDistance = 0.02f;

    float jumpVelocity;
    float jumpGravity;
    float fallGravity;

    float acceleration;
    float deceleration;

    public bool IgnoreEnemies = false;

    public static int PositionSampleIdx;

    public ParticleSystem BodyDeathParticles;

    double deathScheduleTime;
    bool deathScheduled;
    float targetScale;
    float currentScale;
    float wallSlidePendingParticles;

    public bool Alive = false;
    public int ExplodeParticleCount = 200;
    public float EyeScaleSurprised = 1.5f;
    public float DeathMiniDelay = 0.5f;
    [NonSerialized] public PlayerSize Size = PlayerSize.Normal;

    Animator animator;
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
    ClampedCircleDrawer circleDrawer;

    void Awake()
    {
        trans = transform;
        physicsBody = GetComponent<Rigidbody2D>();

        playerBaseScale = trans.localScale.x; // just assuming uniform scale
        eyesTransform = SludgeUtil.FindByName(trans, "Body/Face/Eyes");
        bodyRoot = SludgeUtil.FindByName(trans, "Body").gameObject;
        circleDrawer = SludgeUtil.FindByName(trans, "Body/SoftBody").GetComponent<ClampedCircleDrawer>();
        eyesBaseScale = eyesTransform.localScale;
        playerCollider = GetComponent<CircleCollider2D>();

        childSprites = GetComponentsInChildren<SpriteRenderer>();
        allColliders = GetComponentsInChildren<Collider2D>();
        animator = GetComponentInChildren<Animator>();
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

        StateParam = new StateParam();

        circleDrawer.Reset();
        eyesTransform.localScale = eyesBaseScale;

        bodyRoot.SetActive(true);
        SetAlpha(1.0f);

        Alive = true;
    }

    public void DisableCollisions(bool disable)
    {
        circleDrawer.disableCollisions = disable;

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

    public void ConveyourBeltEnter(Vector2 beltDirection)
    {
        onConveyorBeltCount++;
    }

    public void ConveyourBeltExit(Vector2 beltDirection)
    {
        onConveyorBeltCount--;

        // When resetting game, colliderexits are fired after resetting player, so we get an exit event after setting onConveyorBeltCount to 0.
        if (onConveyorBeltCount < 0)
        {
            onConveyorBeltCount = 0;
        } else if (onConveyorBeltCount == 0)
        {
            // exit force
            StateParam.impulse = Vector2.zero;
            StateParam.force = beltDirection.normalized * MaxVelocity;
        }
    }

    public void AddConveyorPulse(double x, double y)
    {
        StateParam.impulse.x += (float)x;
        StateParam.impulse.y += (float)y;
    }

    void ResetJumpHandicaps()
    {
        StateParam.coyoteJumpEndTime = int.MinValue;
        StateParam.queuedJumpEndTime = int.MinValue;
    }

    public void Teleport(Vector3 newPos)
    {
        ResetJumpHandicaps();
        GameManager.I.DustParticles.transform.position = trans.position;
        GameManager.I.DustParticles.Emit(10);
        GameManager.I.DustParticles.transform.position = newPos;
        GameManager.I.DustParticles.Emit(10);
        trans.position = newPos;
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

        if (entity == EntityType.Energy)
        {
            Kill();
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

    void PlayAnim(string name)
    {
        if (currentAnim == name)
            return;

        Debug.Log("Playing anim: " + name);
        animator.Play(name);
        currentAnim = name;
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

    bool IsJumpTapped() => GameManager.PlayerInput.IsTapped(Sludge.PlayerInputs.PlayerInput.InputType.Jump);
    bool HasQueuedJump() => StateParam.queuedJumpEndTime >= GameManager.I.EngineTimeMs;
    bool HasCoyoteJump() => StateParam.coyoteJumpEndTime >= GameManager.I.EngineTimeMs;
    bool HasGroundContact() => circleDrawer.hasGroundContact;

    void ResetJumpCount(StateParam param)
    {
        param.airJumpsLeft = MutatorUtil.GetJumpCount(param.jumpType);
    }

    bool HasAirJumpsLeft() => StateParam.airJumpsLeft > 0 || StateParam.airJumpsLeft < 0;

    void StartAirJump(StateParam param)
    {
        param.airJumpsLeft--;
        StartJump(param);
    }

    void StartJump(StateParam param)
    {
        param.force.y = jumpVelocity;
        param.jumpHoldStartTime = GameManager.I.EngineTimeMs;
        ResetJumpHandicaps();

        ParticleEmitter.I.EmitDust(trans.position, 4);
    }

    void SetState(StateParam param, JumpState state)
    {
        param.jumpState = state;
    }

    void JumpStateAscendingActive(StateParam param)
    {
        if (param.jumpState != JumpState.AscendingActive) return;

        bool jumpReleased = !GameManager.PlayerInput.JumpActive();
        if (jumpReleased)
        {
            SetState(param, JumpState.AscendingPassive);
            return;
        }

        bool reachedMaxJumpHold = GameManager.I.EngineTimeMs - param.jumpHoldStartTime > JumpMaxHoldTime * 1000;
        if (reachedMaxJumpHold)
        {
            SetState(param, JumpState.AscendingPassive);
            return;
        }
    }

    void JumpStateAscendingPassive(StateParam param)
    {
        if (param.jumpState != JumpState.AscendingPassive) return;

        bool isPastPeak = StateParam.force.y < 0;
        if (isPastPeak)
        {
            SetState(param, JumpState.Gravity);
            return;
        }
    }

    void JumpStateDescending(StateParam param)
    {
        if (param.jumpState != JumpState.Gravity) return;

        if (HasGroundContact())
        {
            ResetJumpCount(param);
            param.coyoteJumpEndTime = GameManager.I.EngineTimeMs + CoyoteJumpMs;

            if (IsJumpTapped() || HasQueuedJump())
            {
                StartJump(param);
                SetState(param, JumpState.AscendingActive);
                return;
            }
        }
        else
        {
            // not touching ground
            if (StateParam.isWallSliding)
            {
                if (IsJumpTapped() || HasQueuedJump())
                {
                    // reset air jumps when wall jumping
                    ResetJumpCount(param);
                    StateParam.force.x = StateParam.isHuggingLeftWall ? RunPeak : -RunPeak;

                    StartJump(param);
                    SetState(param, JumpState.AscendingActive);

                    StateParam.disableHorizontalDirectionEndTime = GameManager.I.EngineTimeMs + WallJumpDisableHorizontalBreakingMs;
                    StateParam.disabledHorizontalDirection = StateParam.isHuggingLeftWall ? -1 : 1;
                    return;
                }
            }

            if (IsJumpTapped())
            {
                if (HasCoyoteJump())
                {
                    StartJump(param);
                    SetState(param, JumpState.AscendingActive);
                    return;
                }

                if (HasAirJumpsLeft())
                {
                    StartAirJump(param);
                    SetState(param, JumpState.AscendingActive);
                    return;
                }
            }

            // check air jump after coyote jump so player won't lose an air jump if coyote jump is available
            if (HasQueuedJump() && HasAirJumpsLeft())
            {
                StartAirJump(param);
                SetState(param, JumpState.AscendingActive);
                return;
            }
        }
    }

    public void EngineTick()
    {
        if (!Alive)
            return;

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

        if (IsJumpTapped())
            StateParam.queuedJumpEndTime = GameManager.I.EngineTimeMs + QueuedJumpMs;

        // set every frame to reflect editor changes
        jumpVelocity = (2.0f * JumpHeight) / JumpTimeToPeak;
        jumpGravity = (-2.0f * JumpHeight) / (JumpTimeToPeak * JumpTimeToPeak);
        fallGravity = (-2.0f * JumpHeight) / (JumpTimeToDescend * JumpTimeToDescend);
        acceleration = RunPeak / RunTimeToPeak;
        deceleration = RunPeak / RunTimeToStop;

        JumpStateAscendingActive(StateParam);
        JumpStateAscendingPassive(StateParam);
        JumpStateDescending(StateParam);

        int direction = 0;

        bool horizontalDisabled = GameManager.I.EngineTimeMs < StateParam.disableHorizontalDirectionEndTime;
        bool moveLeftDisabled = StateParam.disabledHorizontalDirection < 0 && horizontalDisabled;
        bool moveRightDisabled = StateParam.disabledHorizontalDirection > 0 && horizontalDisabled;

        if (GameManager.PlayerInput.Left != 0 && !moveLeftDisabled)
        {
            PlayAnim(AnimMoveLeft.name);
            direction = -1;
        }
        else if (GameManager.PlayerInput.Right != 0 && !moveRightDisabled)
        {
            PlayAnim(AnimMoveRight.name);
            direction = 1;
        }
        else
        {
            PlayAnim(AnimIdle.name);
        }

        if (ShowDebug)
        {
            DebugLinesScript.Show("JumpState", StateParam.jumpState);
            DebugLinesScript.Show("force", StateParam.force);
            Debug.DrawRay(trans.position, trans.position + (Vector3)StateParam.force.normalized, Color.white, 1);
        }

        if (direction != 0)
        {
            float airborneModifier = HasGroundContact() ? 1.0f : AirControl;
            StateParam.force.x += acceleration * direction * (float)GameManager.TickSize * airborneModifier;
            StateParam.force.x = Mathf.Clamp(StateParam.force.x, -RunPeak, RunPeak);
        }
        else
        {
            // decelerate
            if (StateParam.force.x < 0)
            {
                StateParam.force.x += deceleration * (float)GameManager.TickSize;
                StateParam.force.x = Mathf.Min(StateParam.force.x, 0);
            }
            else
            {
                StateParam.force.x -= deceleration * (float)GameManager.TickSize;
                StateParam.force.x = Mathf.Max(StateParam.force.x, 0);
            }
        }

        StateParam.isHuggingLeftWall = circleDrawer.hasLeftContact && !HasGroundContact();
        StateParam.isHuggingRightWall = circleDrawer.hasRightContact && !HasGroundContact();
        StateParam.isDescending = StateParam.force.y < 0;

        StateParam.isWallSliding = (StateParam.isHuggingLeftWall || StateParam.isHuggingRightWall) && StateParam.isDescending;
        StateParam.isWallSliding &= StateParam.jumpType == MutatorJumpType.WallJump;

        // update simulation
        // do not apply gravity while holding jump on a new jump (state = ascending active)
        bool isOnConveyorBelt = onConveyorBeltCount > 0;
        bool useGravity = StateParam.jumpState != JumpState.AscendingActive && !isOnConveyorBelt;
        if (useGravity)
        {
            float gravity = StateParam.force.y < 0 ? fallGravity : jumpGravity;
            StateParam.force.y += gravity * (float)GameManager.TickSize;
        }

        StateParam.force.y = Mathf.Max(StateParam.force.y, -MaxVelocity);
        if (StateParam.isWallSliding)
        {
            wallSlidePendingParticles += (float)GameManager.TickSize * 15;
            while (wallSlidePendingParticles > 0)
            {
                Debug.Log("Emitting wall slide particles");
                wallSlidePendingParticles--;
                ParticleEmitter.I.EmitDust(trans.position, 1);
            }

            float speed = GameManager.PlayerInput.DownActive() ? WallSlideMaxFall * 8 : WallSlideMaxFall;
            StateParam.force.y = Mathf.Max(StateParam.force.y, -speed);
        }

        bool noForce = StateParam.force.magnitude < 0.0001f;
        if (noForce)
        {
            return;
        }

        Vector2 moveStep = StateParam.force * (float)GameManager.TickSize;
        if (isOnConveyorBelt)
            moveStep = Vector3.zero;

        void AddOneShotImpulse()
        {
            moveStep += StateParam.impulse * (float)GameManager.TickSize;
            StateParam.impulse = Vector2.zero;
        }
        AddOneShotImpulse();

        Vector2 moveStepX = new Vector2(moveStep.x, 0);
        Vector2 moveStepY = new Vector2(0, moveStep.y);

        Vector2 acceptedNewPos = TryMove(moveStepX, physicsBody.position);
        acceptedNewPos = TryMove(moveStepY, acceptedNewPos);
        physicsBody.MovePosition(acceptedNewPos);
    }

    float GetPlayerColliderRadius() => playerCollider.radius * trans.localScale.x;

    Vector2 TryMove(Vector2 step, Vector2 from)
    {
        Vector2 newPos = from + step;
        float len = step.magnitude;

        int hitsFullMove = Physics2D.CircleCastNonAlloc(from, GetPlayerColliderRadius(), step.normalized, SludgeUtil.scanHits, len, SludgeUtil.ScanForWallsLayerMask);
        if (hitsFullMove == 0)
        {
            return newPos;
        }

        // just before the actual hit
        float desiredDistance = SludgeUtil.scanHits[0].distance - WallDistance;

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
