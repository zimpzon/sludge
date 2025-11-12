using Assets.Scripts.Game;
using Sludge.Colors;
using Sludge.Utility;
using System;
using UnityEngine;

// IF TILEMAP COLLIDERS NOT WORKING IN RELEASE BUILD, TRY THIS:
//  https://discussions.unity.com/t/tilemap-collider-broken-on-build/204005/2

public enum JumpState { NotSet, AscendingActive, AscendingPassive, Gravity }

public class StateParam
{
    public JumpState jumpState = JumpState.Gravity;

    public MutatorJumpType jumpType = MutatorJumpType.WallJump;
    public MutatorTypePlayerSize playerSize = MutatorTypePlayerSize.DefaultMe;

    public Vector2 force;
    public Vector2 impulse;
    public bool isHoldingJump;
    public int airJumpsLeft = 0;

    public int wallCoyoteJumpEndTime = int.MinValue;
    public int jumpHoldStartTime = int.MaxValue;
    public int coyoteJumpEndTime = int.MinValue;
    public int queuedJumpEndTime = int.MinValue;
    public int horizontalMovementIdleTime = int.MaxValue;
    public int disableHorizontalDirectionEndTime = int.MinValue;
    public int disabledHorizontalDirection = 0;

    public bool isHuggingLeftWall;
    public bool isHuggingRightWall;
    public bool isDescending;
    public bool isWallSliding;
    public bool hasWallJumpEnabled = true;  // NEW: separate wall jump flag
    public int LatestDirection = 1;
}

public class Player : MonoBehaviour, IConveyorBeltPassenger
{
    public static Player I;

    public StateParam StateParam = new StateParam();

    public enum PlayerSize { Small, Normal, Large };

    public Sprite SpriteIdle;
    public Sprite SpriteJump;
    public Sprite[] SpriteRun;

    public static Vector3 Position;

    public bool ShowDebug = false;
    public bool DisableConveyors = false;

    public GameObject Eyes;
    public AnimationClip AnimMoveLeft;
    public AnimationClip AnimMoveRight;
    public AnimationClip AnimIdle;
    string currentAnim;

    public float JumpHeight = 2.0f;
    public float JumpTimeToPeak = 0.1f;
    public float JumpTimeToDescend = 0.1f;
    public float JumpMaxHoldTime = 0.2f;
    public float MaxVelocity = 25.0f;
    public float WallSlideMaxFall = 1.0f;
    public int WallJumpDisableHorizontalBreakingMs = 100; // no effect?
    public float AirControl = 1.0f;
    public int CoyoteJumpMs = 100;
    public int QueuedJumpMs = 100;
    public int timeBeforeIdleMs = 5000;

    public float RunPeak = 20.0f;
    public float RunTimeToPeak = 0.1f;
    public float RunTimeToStop = 0.1f;

    public float WallDistance = 0.02f;

    float jumpVelocity;
    float jumpGravity;
    float fallGravity;

    float acceleration;
    float deceleration;

    public static int PositionSampleIdx;

    public ParticleSystem BodyDeathParticles;

    double deathScheduleTime;
    bool deathScheduled;
    float targetScale;
    float currentScale;
    float wallSlidePendingParticles;

    public float RoundStartTime;
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
    SpriteRenderer[] childSprites;
    SpriteRenderer earlSpritesRenderer;
    GameObject bodyRoot;
    Collider2D[] allColliders;
    float playerBaseScale;
    CircleCollider2D playerCollider;
    CircleCollider2D playerSquashedCollider; // a smaller collider used to detect player is squashed between moving walls
    ClampedCircleDrawer circleDrawer;
    PillCollectorScript pillCollector;

    void Awake()
    {
        I = this;

        trans = transform;
        physicsBody = GetComponent<Rigidbody2D>();

        playerBaseScale = trans.localScale.x; // just assuming uniform scale
        bodyRoot = SludgeUtil.FindByName(trans, "Body").gameObject;
        circleDrawer = SludgeUtil.FindByName(trans, "Body/SoftBody").GetComponent<ClampedCircleDrawer>();
        playerCollider = GetComponent<CircleCollider2D>();
        playerSquashedCollider = SludgeUtil.FindByName(trans, "SquashedCollider").GetComponent<CircleCollider2D>();
        earlSpritesRenderer = SludgeUtil.FindByName(trans, "Body/EarlSprite").GetComponent<SpriteRenderer>();

        childSprites = GetComponentsInChildren<SpriteRenderer>();
        allColliders = GetComponentsInChildren<Collider2D>();
        pillCollector = GetComponentInChildren<PillCollectorScript>();
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
        Eyes.SetActive(false);

        pillCollector.enabled = true;

        bodyRoot.SetActive(true);
        PlayAnim(AnimIdle.name);
        SetAlpha(1.0f);

        earlSpritesRenderer.sprite = SpriteIdle;

        Alive = true;
    }

    public void DisableCollisions(bool disable)
    {
        pillCollector.enabled = !disable;

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

    public void OnConveyorBeltEnter(Vector2 beltDirection)
    {
        if (DisableConveyors)
            return;

        onConveyorBeltCount++;
    }

    public void OnConveyorBeltExit(Vector2 beltDirection)
    {
        if (DisableConveyors)
            return;

        onConveyorBeltCount--;

        // When resetting game, colliderexits are fired after resetting player, so we get an exit event after setting onConveyorBeltCount to 0.
        if (onConveyorBeltCount < 0)
        {
            onConveyorBeltCount = 0;
        }
        else if (onConveyorBeltCount == 0)
        {
            // exit force
            StateParam.impulse = Vector2.zero;
            StateParam.force = beltDirection.normalized * MaxVelocity;
        }
    }

    public void AddConveyorPulse(Vector2 pulse)
    {
        if (DisableConveyors)
            return;

        StateParam.impulse += pulse;
    }

    void ResetJumpHandicaps()
    {
        StateParam.coyoteJumpEndTime = int.MinValue;
        StateParam.queuedJumpEndTime = int.MinValue;
        StateParam.wallCoyoteJumpEndTime = int.MinValue;
    }

    public void Teleport(Vector3 newPos)
    {
        ResetJumpHandicaps();
        GameManager.I.DustParticles.transform.position = trans.position;
        GameManager.I.DustParticles.Emit(5);
        GameManager.I.DustParticles.transform.position = newPos;
        GameManager.I.DustParticles.Emit(5);
        trans.position = newPos;
    }

    public void SetHomePosition()
    {
        homePos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Let enemy kill player instead, had some problems with stuck bullet after hitting enemy. TileMap can kill player.
        var entityType = SludgeUtil.GetEntityType(collision.gameObject);

        if (entityType == EntityType.EnemyBehindStaticLevel)
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
        if (deathScheduled || !Alive)
            return;

        Eyes.SetActive(true);
        deathScheduleTime = GameManager.I.EngineTime + DeathMiniDelay;
        deathScheduled = true;

        // TODO: have to switch to idle or the eyes will be stuck at the side of the head
        PlayAnim(AnimIdle.name);
    }

    void ExecuteDelayedKill()
    {
        Eyes.SetActive(false);
        SoundManager.Play(FxList.Instance.PlayerDie);
        ParticleEmitter.I.EmitDust(trans.position, 8);
        GameManager.I.ShakeCamera(duration: 0.2f, strength: 0.7f);

        EmitDeathExplosionParticles(trans.position, ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor.Player));

        bodyRoot.SetActive(false);
        trans.position = Vector3.one * 5544; // move out of the way

        Alive = false;
    }

    void PlayAnim(string name)
    {
        if (currentAnim == name)
            return;

        animator.CrossFade(name, 0.1f);

        currentAnim = name;
    }

    public void EmitDeathExplosionParticles(Vector3 pos, Color mainColor, float scale = 1.0f)
    {
        // Body particles
        BodyDeathParticles.transform.position = pos;

        int particleCount = (int)(ExplodeParticleCount * scale);

        var main = BodyDeathParticles.main;
        main.startColor = mainColor;
        BodyDeathParticles.Emit(particleCount);

        // black, for no reason, but looks better?
        main.startColor = Color.black;
        BodyDeathParticles.Emit(particleCount / 10);

        // Eye particles
        main.startColor = Color.white;
        BodyDeathParticles.Emit(particleCount / 20);
    }

    public bool Debug_HasWallCoyoteJump;
    public bool Debug_IsJumpTapped;
    public bool Debug_HasQueuedJump;
    public bool Debug_HasCoyoteJump;
    public bool Debug_HasGroundContact;

    bool HasWallCoyoteJump() => StateParam.wallCoyoteJumpEndTime >= GameManager.I.EngineTimeMs;
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

        ParticleEmitter.I.EmitDust(trans.position, 3);
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

            // UPDATED: Check wall jump (including coyote time) if wall jump is enabled
            if (StateParam.hasWallJumpEnabled)
            {
                bool isCurrentlyWallSliding = StateParam.isWallSliding;
                bool hasRecentWallContact = HasWallCoyoteJump();

                if ((isCurrentlyWallSliding || hasRecentWallContact) && (IsJumpTapped() || HasQueuedJump()))
                {
                    // Determine which direction to jump based on current or recent wall contact
                    bool jumpRight = StateParam.isHuggingLeftWall ||
                                    (hasRecentWallContact && StateParam.disabledHorizontalDirection < 0);

                    StateParam.LatestDirection = jumpRight ? 1 : -1;

                    // Wall jump grants a full air jump refresh
                    ResetJumpCount(param);
                    StateParam.force.x = jumpRight ? RunPeak * 2 : -RunPeak * 2;

                    StartJump(param);
                    SetState(param, JumpState.AscendingActive);

                    StateParam.disableHorizontalDirectionEndTime = GameManager.I.EngineTimeMs + WallJumpDisableHorizontalBreakingMs;
                    StateParam.disabledHorizontalDirection = jumpRight ? 1 : -1;

                    // Clear wall coyote time after using it
                    StateParam.wallCoyoteJumpEndTime = int.MinValue;
                    return;
                }
            }

            // Then check coyote jump
            if (IsJumpTapped())
            {
                if (HasCoyoteJump())
                {
                    StartJump(param);
                    SetState(param, JumpState.AscendingActive);
                    return;
                }

                // Air jumps work independently
                if (HasAirJumpsLeft())
                {
                    StartAirJump(param);
                    SetState(param, JumpState.AscendingActive);
                    return;
                }
            }

            // Queued air jump
            if (HasQueuedJump() && HasAirJumpsLeft())
            {
                StartAirJump(param);
                SetState(param, JumpState.AscendingActive);
                return;
            }
        }
    }

    private void CheckSquashed()
    {
        // Player may slightly overlap a collider when loading a new map, causing a death on first round. Wait for it to "slide" out.
        if (Time.time < RoundStartTime + 100)
            return;

        int hits = Physics2D.OverlapCollider(playerSquashedCollider, SludgeUtil.ScanForWallFilter, SludgeUtil.colliderHits);
        bool playerWasSquished = hits > 0;
        if (playerWasSquished)
        {
            Kill();
            return;
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
            StateParam.LatestDirection = direction;
        }
        else if (GameManager.PlayerInput.Right != 0 && !moveRightDisabled)
        {
            PlayAnim(AnimMoveRight.name);
            direction = 1;
            StateParam.LatestDirection = direction;
        }

        // Look left or right
        var lookDir = 0;
        if (StateParam.isHuggingLeftWall || StateParam.LatestDirection == -1)
            lookDir = -1;
        else
            lookDir = 1;

        var scale = transform.localScale;
        scale.x = lookDir;
        transform.localScale = scale;

        bool isHorizontallyStill = Mathf.Abs(StateParam.force.x) < 0.001f;
        if (!isHorizontallyStill)
        {
            StateParam.horizontalMovementIdleTime = GameManager.I.EngineTimeMs + timeBeforeIdleMs;
        }
        else
        {
            if (GameManager.I.EngineTimeMs > StateParam.horizontalMovementIdleTime && HasGroundContact())
                PlayAnim(AnimIdle.name);
        }

        if (ShowDebug)
        {
            DebugLinesScript.Show("JumpState", StateParam.jumpState);
            DebugLinesScript.Show("force", StateParam.force);
            DebugLinesScript.Show("HasGroundContact", HasGroundContact());
            Debug.DrawRay(trans.position, trans.position + (Vector3)StateParam.force.normalized, Color.white, 1);
        }

        // if No input || airboirne = sprite idle
        // if Running = cycle run sprites
        if (HasGroundContact() && direction != 0)
        {
            int idx = (GameManager.I.EngineTimeMs / 50) % SpriteRun.Length;
            earlSpritesRenderer.sprite = SpriteRun[idx];
        }
        else
        {
            earlSpritesRenderer.sprite = StateParam.force.y > 0 ? SpriteJump : SpriteIdle;
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

        bool wasWallSliding = StateParam.isWallSliding;
        bool wasHuggingLeftWall = StateParam.isHuggingLeftWall;

        StateParam.isHuggingLeftWall = circleDrawer.hasLeftContact && !HasGroundContact();
        StateParam.isHuggingRightWall = circleDrawer.hasRightContact && !HasGroundContact();
        StateParam.isDescending = StateParam.force.y < 0;

        // Wall sliding is now independent - just needs wall contact and descending
        StateParam.isWallSliding = (StateParam.isHuggingLeftWall || StateParam.isHuggingRightWall) && StateParam.isDescending;
        StateParam.isWallSliding &= StateParam.hasWallJumpEnabled; // Only slide if wall jump is enabled

        // NEW: Set wall coyote time when leaving a wall
        if (wasWallSliding && !StateParam.isWallSliding && !HasGroundContact())
        {
            StateParam.wallCoyoteJumpEndTime = GameManager.I.EngineTimeMs + CoyoteJumpMs;
            // Remember which wall we were on for the jump direction
            StateParam.disabledHorizontalDirection = wasHuggingLeftWall ? -1 : 1;
        }

        // UPDATED: Wall sliding is now independent - just needs wall contact and descending
        StateParam.isWallSliding = (StateParam.isHuggingLeftWall || StateParam.isHuggingRightWall) && StateParam.isDescending;
        StateParam.isWallSliding &= StateParam.hasWallJumpEnabled; // Only slide if wall jump is enabled

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
                wallSlidePendingParticles--;
                ParticleEmitter.I.EmitDust(trans.position, 1);
            }

            float speed = GameManager.PlayerInput.DownActive() ? WallSlideMaxFall * 8 : WallSlideMaxFall;
            StateParam.force.y = Mathf.Max(StateParam.force.y, -speed);
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

        // climp up slopes by adjusting moveStep if a slope is detected in CheckSlope
        // how to not slide, or speed run, down?
        Vector2 moveStepX = new Vector2(moveStep.x, 0);
        float stepLen = moveStep.magnitude;
        Vector2 slopeAdjust = CheckSlope(moveStepX, physicsBody.position);
        bool isOnSlope = slopeAdjust != Vector2.zero;

        moveStep += slopeAdjust * stepLen;
        moveStep = moveStep.normalized * stepLen;

        // make jumping on narrow (flat) surfaces easier by disabling gravity when on edge
        // if this has unforseen consequences look for another solution
        bool isOnEdge = (circleDrawer.hasDropToTheLeft && circleDrawer.hasFlatSurfaceToTheRight) || (circleDrawer.hasDropToTheRight && circleDrawer.hasFlatSurfaceToTheLeft);
        if (isOnEdge && !isOnSlope && StateParam.jumpState == JumpState.Gravity)
        {
            // ONLY if flat surface
            moveStep.y = 0.0f;
        }

        physicsBody.MovePosition(physicsBody.position + moveStep);
        CheckSquashed();

        Debug_HasWallCoyoteJump = HasWallCoyoteJump();
        Debug_IsJumpTapped = IsJumpTapped();
        Debug_HasQueuedJump = HasQueuedJump();
        Debug_HasCoyoteJump = HasCoyoteJump();
        Debug_HasGroundContact = HasGroundContact();
    }

    float GetPlayerColliderRadius() => Math.Abs(playerCollider.radius * trans.localScale.x);

    Vector2 CheckSlope(Vector2 step, Vector2 from)
    {
        float len = step.magnitude;
        int hitsFullMove = Physics2D.CircleCast(from, GetPlayerColliderRadius(), step.normalized, SludgeUtil.ScanForWallFilter, SludgeUtil.scanHits, len);
        if (hitsFullMove == 0)
        {
            return Vector2.zero;
        }

        Vector2 normal = SludgeUtil.scanHits[0].normal;
        float angle = Vector2.Angle(normal, Vector2.up);
        if (Math.Abs(angle) > 45.5f)
        {
            return Vector2.zero;
        }

        // move effortlessly over > 45 degree slopes, could adjust for steepness
        Vector2 cross = Vector2.Perpendicular(normal);
        float dot = Vector2.Dot(step, cross);
        if (dot < 0)
            cross *= -1;

        //Debug.DrawLine(from, from + normal, Color.yellow, 0.05f);
        //Debug.DrawLine(from, from + cross, Color.red, 0.05f);

        float scaledBySteepness = (100 - angle) / 100;
        cross = cross.normalized * scaledBySteepness; // 45 degrees = 0.5 power, 0 degrees = 1 power
        return cross;
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
            if (dist < 0.08f)
                return;
        }

        if (!init)
            PositionSampleIdx++;

        GameManager.PlayerSamples[PositionSampleIdx].Pos = trans.position;
    }
}