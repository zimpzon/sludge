using DG.Tweening;
using Sludge;
using Sludge.Utility;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static PlayerSample[] PlayerSamples = new PlayerSample[30000];

    public static Vector3 Position;
    public static double Angle;
    public static Quaternion Rotation;
    public static int PositionSampleIdx;

    public TrailRenderer trail;
    public bool Alive = false;

    QuadDistort ripples;
    public double angle = 90;
    double speed;
    double minSpeed = 0.5f;
    double maxSpeed = 10;
    double turnSpeed = 250;
    double turnMultiplier = 20;
    double accelerateSpeed = 80;
    double friction = 25;
    Transform trans;
    double playerX;
    double playerY;
    double homeX;
    double homeY;
    double homeAngle;
    ContactFilter2D wallScanFilter = new ContactFilter2D();
    int onConveyorBeltCount;
    public ModThrowable currentThrowable;
    LineRenderer softBody;
    Transform eyesTransform;
    Vector2 eyesBaseScale;
    double timeEnterSlimeCloud;
    int sampleIdxAtTimeOfTeleport;
    int resetTrailTime; // Trail hack when teleporting to not draw between teleporters
    float trailTime;

    // Impulses: summed up and added every frame. Then cleared.
    double impulseX;
    double impulseY;
    double impulseRotation;

    // Override: summed up and the average per frame is forcibly set, ignoring all other movement (player loses control). Then cleared.
    List<double> overrideX = new List<double>();
    List<double> overrideY = new List<double>();
    List<double> overrideRotation = new List<double>();

    // Forces: summed up and added every frame. Diminished over multiple frames.
    double forceX;
    double forceY;
    bool wasAcceleratingLastFrame;

    void Awake()
    {
        trailTime = trail.time;
        softBody = GetComponentInChildren<LineRenderer>();
        softBody.positionCount = 50; // Max number of line segments for body
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
        trail.Clear();
        trail.enabled = false;
        Alive = true;
        onConveyorBeltCount = 0;
        ripples.Reset();
        impulseX = 0;
        impulseY = 0;
        impulseRotation = 0;
        currentThrowable = null;
        timeEnterSlimeCloud = -1;
        sampleIdxAtTimeOfTeleport = 0;
        PositionSampleIdx = 0;

        overrideX.Clear();
        overrideY.Clear();
        overrideRotation.Clear();

        forceX = 0;
        forceY = 0;

        playerX = homeX;
        playerY = homeY;
        angle = homeAngle;
        eyesTransform.localScale = eyesBaseScale;

        UpdateTransform();
        SetPositionSample(init: true);
        UpdateSoftBody();

        trail.enabled = true;
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
        sampleIdxAtTimeOfTeleport = PositionSampleIdx + 1;
        trail.Clear();
        trail.time = 0;
        resetTrailTime = 2;
    }

    public void AddOverridePosition(double x, double y)
    {
        overrideX.Clear();
        overrideY.Clear();
        overrideX.Add(SludgeUtil.Stabilize(x));
        overrideY.Add(SludgeUtil.Stabilize(y));
    }

    public void AddOverrideRotation(double rotation)
    {
        overrideRotation.Clear();
        overrideRotation.Add(SludgeUtil.Stabilize(rotation));
    }

    public void AddPositionImpulse(double x, double y)
    {
        impulseX += SludgeUtil.Stabilize(x);
        impulseY += SludgeUtil.Stabilize(y);
    }

    public void AddRotationImpulse(double angle)
    {
        impulseRotation += SludgeUtil.Stabilize(angle);
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
        if (onConveyorBeltCount > 0)
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
        ripples.DoRipples();
        GameManager.Instance.DeathParticles.transform.position = trans.position;
        GameManager.Instance.DeathParticles.Emit(50);
        GameManager.Instance.CameraRoot.DORewind();
        GameManager.Instance.CameraRoot.DOShakePosition(0.1f, 0.5f);
        Alive = false;
    }

    void PlayerControls()
    {
        bool isTurning = false;

        if (GameManager.PlayerInput.Left != 0)
        {
            isTurning = true;
            angle += GameManager.TickSize * (turnSpeed + (maxSpeed - speed) * turnMultiplier);
            if (angle > 360)
                angle -= 360;
        }

        if (GameManager.PlayerInput.Right != 0)
        {
            isTurning = true;
            angle -= GameManager.TickSize * (turnSpeed + (maxSpeed - speed) * turnMultiplier);
            if (angle < 0)
                angle += 360;
        }

        if (!isTurning)
        {
            // Snap to 0, 45, 90, etc., if very close
            int snap = Mathf.RoundToInt((float)SludgeUtil.Stabilize(angle) / 45) % 8;
            double snappedAngle = snap * 45;
            double diff = angle - snappedAngle;
            double absDiff = Mathf.Abs((float)SludgeUtil.Stabilize(diff));
            if (absDiff < 1.0f)
            {
                angle = snappedAngle;
            }
            else if (absDiff < 15)
            {
                angle -= SludgeUtil.Stabilize(Mathf.Sign((float)diff) * GameManager.TickSize * turnSpeed * 0.1);
            }

            angle = SludgeUtil.AngleNormalized0To360(angle);
        }

        if (GameManager.PlayerInput.Up != 0 && onConveyorBeltCount == 0)
        {
            speed = SludgeUtil.Stabilize(speed + GameManager.TickSize * accelerateSpeed);
            if (speed > maxSpeed)
                speed = maxSpeed;
        }

        bool isAccelerating = GameManager.PlayerInput.Up != 0;
        bool accelerationStart = !wasAcceleratingLastFrame && isAccelerating;
        bool accelerationEnd = wasAcceleratingLastFrame && !isAccelerating;

        //if (GameManager.PlayerInput.UpDoubleTap != 0 && currentThrowable != null)
        if (accelerationEnd && currentThrowable != null)
        {
            currentThrowable.Throw(trans.rotation * Vector2.up, maxSpeed * 1.2);
            currentThrowable = null;
        }

        double lookX = -SludgeUtil.Stabilize(Mathf.Sin((float)(Mathf.Deg2Rad * angle)));
        double lookY = SludgeUtil.Stabilize(Mathf.Cos((float)(Mathf.Deg2Rad * angle)));

        playerX += speed * GameManager.TickSize * lookX;
        playerY += speed * GameManager.TickSize * lookY;

        wasAcceleratingLastFrame = isAccelerating;
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
        if (!Alive)
            return;
        PlayerControls();

        speed = SludgeUtil.Stabilize(speed - GameManager.TickSize * friction);
        if (speed < minSpeed)
            speed = minSpeed;

        // Impulse
        playerX += impulseX * GameManager.TickSize;
        playerY += impulseY * GameManager.TickSize;
        impulseX = 0;
        impulseY = 0;

        angle += impulseRotation;
        angle = SludgeUtil.AngleNormalized0To360(angle);
        impulseRotation = 0;

        // Force
        playerX += forceX * GameManager.TickSize;
        playerY += forceY * GameManager.TickSize;
        forceX = SludgeUtil.Stabilize(forceX - GameManager.TickSize * friction);
        if (forceX < 0)
            forceX = 0;
        forceY = SludgeUtil.Stabilize(forceY - GameManager.TickSize * friction);
        if (forceY < 0)
            forceY = 0;

        // Override
        if (overrideX.Count > 0)
        {
            double sumX = 0;
            for (int i = 0; i < overrideX.Count; ++i)
                sumX += overrideX[i];
            double avgX = sumX / overrideX.Count;
            playerX = SludgeUtil.Stabilize(avgX);
            overrideX.Clear();
        }

        if (overrideY.Count > 0)
        {
            double sumY = 0;
            for (int i = 0; i < overrideY.Count; ++i)
                sumY += overrideY[i];
            double avgY = sumY / overrideY.Count;
            playerY = SludgeUtil.Stabilize(avgY);
            overrideY.Clear();
        }

        if (overrideRotation.Count > 0)
        {
            double sumR = 0;
            for (int i = 0; i < overrideRotation.Count; ++i)
                sumR += overrideRotation[i];
            double avgR = sumR / overrideRotation.Count;
            angle = SludgeUtil.Stabilize(avgR);
            overrideRotation.Clear();
        }

        playerX = SludgeUtil.Stabilize(playerX);
        playerY = SludgeUtil.Stabilize(playerY);
        angle = SludgeUtil.Stabilize(angle);

        UpdateTransform();
        SetPositionSample();

        UpdateSoftBody();

        CheckSlimeCloud();

        if (--resetTrailTime == 0)
        {
            trail.Clear();
            trail.time = trailTime;
        }
    }

    void SetPositionSample(bool init = false)
    {
        if (!init && PositionSampleIdx > 0)
        {
            var prevPos = PlayerSamples[PositionSampleIdx - 1].Pos;
            var dist = (trans.position - prevPos).magnitude;
            if (dist < 0.08f)
                return;
        }

        if (!init)
            PositionSampleIdx++;

        PlayerSamples[PositionSampleIdx].Pos = trans.position;
        PlayerSamples[PositionSampleIdx].Angle = angle;
    }

    void UpdateSoftBody()
    {
        const double TargetDistance = 0.6;
        double distanceCovered = 0;
        Vector3 prevPoint = Vector3.zero;
        Vector3 currentPoint = Vector3.zero;

        int playerPosSampleIdx = PositionSampleIdx;
        for (int softBodyIdx = 0; softBodyIdx < softBody.positionCount; ++softBodyIdx)
        {
            bool hasPlayerPosSamples = playerPosSampleIdx >= sampleIdxAtTimeOfTeleport; // 0 is always there, set in reset
            if (hasPlayerPosSamples)
            {
                if (distanceCovered < TargetDistance)
                    currentPoint = PlayerSamples[playerPosSampleIdx].Pos;
            }
            else
            {
                // We are below 0 (or latest teleport point) in the position array, player hasn't moved enough yet (or at all).
                // Extrapolate points in the opposite direction the player is facing.
                if (distanceCovered < TargetDistance)
                {
                    double lookBackX = -SludgeUtil.Stabilize(Mathf.Sin((float)(Mathf.Deg2Rad * (angle + 180))));
                    double lookBackY = SludgeUtil.Stabilize(Mathf.Cos((float)(Mathf.Deg2Rad * (angle + 180))));
                    const double DistanceBack = 0.05;
                    currentPoint.x += (float)(lookBackX * DistanceBack);
                    currentPoint.y += (float)(lookBackY * DistanceBack);
                }
            }

            softBody.SetPosition(softBodyIdx, currentPoint);

            float currentDistance = (currentPoint - prevPoint).magnitude;
            distanceCovered += softBodyIdx == 0 ? 0 : currentDistance;
            playerPosSampleIdx--;
            prevPoint = currentPoint;
        }
    }

    void UpdateTransform()
    {
        trans.rotation = Quaternion.Euler(0, 0, (float)angle);
        trans.position = new Vector3((float)playerX, (float)playerY, 0);

        Angle = angle;
        Rotation = trans.rotation;
    }
}
