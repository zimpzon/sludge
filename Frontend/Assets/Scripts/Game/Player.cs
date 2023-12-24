using DG.Tweening;
using Sludge.Utility;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Vector3 Position;
    public static double Angle;
    public static Quaternion Rotation;
    public static int PositionSampleIdx;

    public Transform LegL0;
    public Transform LegR0;
    public Transform LegL1;
    public Transform LegR1;
    public Transform LegL2;
    public Transform LegR2;
    public PlayerBullet PlayerBullet;

    Vector2 LegL0Base;
    Vector2 LegR0Base;
    Vector2 LegL1Base;
    Vector2 LegR1Base;
    Vector2 LegL2Base;
    Vector2 LegR2Base;
    bool lockLegMovement = true;

    public bool Alive = false;

    QuadDistort ripples;
    public double angle = 90;
    double speed;
    double speedX;
    double speedY;
    double minSpeed = 0.0f;
    public double maxSpeed = 10;
    public double accelerateSpeed = 300;
    public double friction = 50.0f;
    Transform trans;
    double playerX;
    double playerY;
    double homeX;
    double homeY;
    double homeAngle;
    ContactFilter2D wallScanFilter = new ContactFilter2D();
    int onConveyorBeltCount;
    public ModThrowable currentThrowable;
    Transform eyesTransform;
    Vector2 eyesBaseScale;
    double timeEnterSlimeCloud;

    // Impulses: summed up and added every frame. Then cleared.
    double impulseX;
    double impulseY;

    // Forces: summed up and added every frame. Diminished over multiple frames.
    double forceX;
    double forceY;

    void Awake()
    {
        LegL0Base = LegL0.localPosition;
        LegR0Base = LegR0.localPosition;
        LegL1Base = LegL1.localPosition;
        LegR1Base = LegR1.localPosition;
        LegL2Base = LegL2.localPosition;
        LegR2Base = LegR2.localPosition;

        ripples = GetComponentInChildren<QuadDistort>();
        trans = transform;
        wallScanFilter.SetLayerMask(SludgeUtil.ScanForWallsLayerMask);
        eyesTransform = SludgeUtil.FindByName(trans, "Body/Eyes");
        eyesBaseScale = eyesTransform.localScale;
    }

    public void Prepare()
    {
        Debug.Log($"Player.Prepare()");
        speed = minSpeed;
        Alive = true;
        onConveyorBeltCount = 0;
        ripples.Reset();
        impulseX = 0;
        impulseY = 0;
        currentThrowable = null;
        timeEnterSlimeCloud = -1;
        PositionSampleIdx = 0;

        forceX = 0;
        forceY = 0;
        speedX = 0;
        speedY = 0;

        playerX = homeX;
        playerY = homeY;
        angle = homeAngle;
        eyesTransform.localScale = eyesBaseScale;

        UpdateTransform();
        PlayerBullet.Reset();
    }

    public void ThrowablePickedUp(ModThrowable throwable)
    {
        currentThrowable = throwable;
    }

    public void ConveyourBeltEnter()
    {
        onConveyorBeltCount++;
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
        GameManager.Instance.DustParticles.transform.position = trans.position;
        GameManager.Instance.DustParticles.Emit(10);
        GameManager.Instance.DustParticles.transform.position = newPos;
        GameManager.Instance.DustParticles.Emit(10);

        playerX = SludgeUtil.Stabilize(newPos.x);
        playerY = SludgeUtil.Stabilize(newPos.y);
    }

    public void AddPositionImpulse(double x, double y)
    {
        impulseX += SludgeUtil.Stabilize(x);
        impulseY += SludgeUtil.Stabilize(y);
    }

    public void AddPositionForce(double x, double y)
    {
        forceX += SludgeUtil.Stabilize(x);
        forceY += SludgeUtil.Stabilize(y);
    }

    public void SetHomePosition()
    {
        homeX = SludgeUtil.Stabilize(trans.position.x);
        homeY = SludgeUtil.Stabilize(trans.position.y);
        homeAngle = SludgeUtil.Stabilize(trans.rotation.eulerAngles.z);
        Debug.Log($"Setting player home: {homeX}, {homeY}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.PlayerBullet)
            return;

        Kill();
    }

    public void ExitSlimeCloud()
    {
        timeEnterSlimeCloud = -1;
        eyesTransform.localScale = eyesBaseScale;
    }

    public void InSlimeCloud()
    {
        if (timeEnterSlimeCloud > 0)
            return;

        timeEnterSlimeCloud = GameManager.Instance.EngineTime;
        eyesTransform.localScale = eyesBaseScale * 1.5f;
    }

    public void Kill()
    {
        SoundManager.Play(FxList.Instance.PlayerDie);
        ripples.DoRipples();
        GameManager.Instance.DeathParticles.transform.position = trans.position;
        GameManager.Instance.DeathParticles.Emit(50);
        GameManager.Instance.CameraRoot.DORewind();
        GameManager.Instance.CameraRoot.DOShakePosition(0.1f, 0.5f);
        Alive = false;
    }

    float legOffset = 0;

    void CheckShoot()
    {
        if (GameManager.PlayerInput.Shoot != 0 && !PlayerBullet.Alive)
        {
            var look = SludgeUtil.LookAngle(trans.rotation.eulerAngles.z);
            const float BulletSpeed = 40;
            PlayerBullet.DX = SludgeUtil.Stabilize(look.x * BulletSpeed);
            PlayerBullet.DY = SludgeUtil.Stabilize(look.y * BulletSpeed);
            PlayerBullet.X = SludgeUtil.Stabilize(trans.position.x + look.x * 0.5);
            PlayerBullet.Y = SludgeUtil.Stabilize(trans.position.y + look.y * 0.5);

            PlayerBullet.Fire();
            //SoundManager.Play(FxList.Instance.PlayerShoot);
        }
    }

    void PlayerControls4Dir()
    {
        bool hasPlayerHorizontalInput = false;
        bool hasPlayerVerticalInput = false;

        if (GameManager.PlayerInput.Left != 0)
        {
            speedX -= accelerateSpeed * GameManager.TickSize;
            hasPlayerHorizontalInput = true;
        }

        if (GameManager.PlayerInput.Right != 0)
        {
            speedX += accelerateSpeed * GameManager.TickSize;
            hasPlayerHorizontalInput = true;
        }

        if (GameManager.PlayerInput.Up != 0)
        {
            speedY += accelerateSpeed * GameManager.TickSize;
            hasPlayerVerticalInput = true;
        }

        if (GameManager.PlayerInput.Down != 0)
        {
            speedY -= accelerateSpeed * GameManager.TickSize;
            hasPlayerVerticalInput = true;
        }

        bool hasPlayerInput = hasPlayerHorizontalInput || hasPlayerVerticalInput;

        speedX = Mathf.Clamp((float)speedX, (float)-maxSpeed, (float)maxSpeed);
        speedY = Mathf.Clamp((float)speedY, (float)-maxSpeed, (float)maxSpeed);

        // Speed curves
        //  _________
        // /         \

        if (!hasPlayerHorizontalInput)
            Friction(ref speedX);

        if (!hasPlayerVerticalInput)
            Friction(ref speedY);

        var moveVec = new Vector2((float)speedX, (float)speedY);
        bool isMoving = moveVec.sqrMagnitude > 0;

        lockLegMovement = !isMoving;

        const float legSpeed = 20;
        if (hasPlayerInput)
        {
            angle = Mathf.Atan2((float)speedY, (float)speedX) * Mathf.Rad2Deg - 90;
        }

        if (isMoving)
        {
            legOffset += (float)(GameManager.TickSize * legSpeed);
        }

        if (!isMoving && currentThrowable != null)
        {
            currentThrowable.Throw(trans.rotation * Vector2.up, maxSpeed * 1.2);
            currentThrowable = null;
        }

        playerX += speedX * GameManager.TickSize;
        playerY += speedY * GameManager.TickSize;
    }

    void Friction(ref double a)
    {
        if (a > 0)
        {
            a -= friction * GameManager.TickSize;
            if (a < 0)
                a = 0;
        }
        else if (a < 0)
        {
            a += friction * GameManager.TickSize;
            if (a > 0)
                a = 0;
        }
    }

    void CheckSlimeCloud()
    {
        if (timeEnterSlimeCloud < 0)
            return;

        double timeInSlimeCloud = GameManager.Instance.EngineTime - timeEnterSlimeCloud;
        if (timeInSlimeCloud >= 1)
            Kill();
    }

    public void EngineTick()
    {
        CheckShoot();
        PlayerBullet.Tick();

        if (!Alive)
            return;

        PlayerControls4Dir();

        speed = SludgeUtil.Stabilize(speed - GameManager.TickSize * friction);
        if (speed < minSpeed)
            speed = minSpeed;

        // Impulse
        playerX += impulseX * GameManager.TickSize;
        playerY += impulseY * GameManager.TickSize;
        impulseX = 0;
        impulseY = 0;

        // Force
        playerX += forceX * GameManager.TickSize;
        playerY += forceY * GameManager.TickSize;
        forceX = SludgeUtil.Stabilize(forceX - GameManager.TickSize * friction);
        if (forceX < 0)
            forceX = 0;
        forceY = SludgeUtil.Stabilize(forceY - GameManager.TickSize * friction);
        if (forceY < 0)
            forceY = 0;

        playerX = SludgeUtil.Stabilize(playerX);
        playerY = SludgeUtil.Stabilize(playerY);

        UpdateTransform();

        CheckSlimeCloud();
    }

    void UpdateLegs()
    {
        SetLegOffset(LegL2, LegL2Base, legOffset, 0.0f);
        SetLegOffset(LegL0, LegL0Base, legOffset, 0.8f);
        SetLegOffset(LegL1, LegL1Base, legOffset, 1.6f);

        SetLegOffset(LegR2, LegR2Base, legOffset, 2.0f);
        SetLegOffset(LegR0, LegR0Base, legOffset, 2.8f);
        SetLegOffset(LegR1, LegR1Base, legOffset, 3.6f);
    }

    void SetLegOffset(Transform leg, Vector2 legBase, float baseOffset, float legOffset)
    {
        float legRange = 0.3f;
        float offset = (Mathf.PingPong(baseOffset + legOffset, 2) - 1) * legRange;
        if (lockLegMovement)
            offset = 0;

        leg.localPosition = legBase + Vector2.up * offset;
    }

    private void Update()
    {
        UpdateLegs();
    }

    void UpdateTransform()
    {
        trans.rotation = Quaternion.Euler(0, 0, (float)angle);
        trans.position = new Vector3((float)playerX, (float)playerY, 0);

        Angle = angle;
        Rotation = trans.rotation;
    }
}
