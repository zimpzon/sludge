using Sludge.Utility;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Vector3 Position;
    public static double Angle;
    public static Quaternion Rotation;

    public SpriteRenderer bodyRenderer;
    public TrailRenderer trail;
    public bool Alive = false;

    public double angle = 90;
    double speed;
    double minSpeed = 0.5f;
    double maxSpeed = 10;
    double turnSpeed = 180;
    double turnMultiplier = 8;
    double accelerateSpeed = 100;
    double friction = 25;
    Transform trans;
    ParticleSystem deathParticles;
    double playerX;
    double playerY;
    double homeX;
    double homeY;
    double homeAngle;
    ContactFilter2D wallScanFilter = new ContactFilter2D();
    int onConveyorBeltCount;

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

    public void ConveyourBeltEnter()
    {
        Debug.Log($"Enter, frame = {GameManager.Instance.FrameCounter}");
        onConveyorBeltCount++;
    }

    public void ConveyourBeltExit()
    {
        Debug.Log($"Exit, frame = {GameManager.Instance.FrameCounter}");
        onConveyorBeltCount--;
        // When resetting game colliderexits are fired after resetting player, so we get an exit event after setting onConveyorBeltCount to 0.
        if (onConveyorBeltCount < 0)
            onConveyorBeltCount = 0;
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

    void Awake()
    {
        deathParticles = GetComponentInChildren<ParticleSystem>();
        var particleMain = deathParticles.main;
        particleMain.startColor = bodyRenderer.color;
        trans = transform;
        wallScanFilter.SetLayerMask(SludgeUtil.ScanForWallsLayerMask);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (onConveyorBeltCount > 0)
            return;

        Kill();
    }

    public void Kill()
    {
        deathParticles.Emit(50);
        Alive = false;
    }

    public void Prepare()
    {
        Debug.Log($"Player.Prepare()");
        speed = minSpeed;
        trail.Clear();
        trail.enabled = false;
        Alive = true;
        onConveyorBeltCount = 0;

        impulseX = 0;
        impulseY = 0;
        impulseRotation = 0;

        overrideX.Clear();
        overrideY.Clear();
        overrideRotation.Clear();

        forceX = 0;
        forceY = 0;

        playerX = homeX;
        playerY = homeY;
        angle = homeAngle;

        UpdateTransform();

        trail.enabled = true;
    }

    void PlayerControls()
    {
        bool isTurning = false;

        if (GameManager.PlayerInput.Left != 0)
        {
            isTurning = true;
            angle += GameManager.TickSize * (turnSpeed + speed * turnMultiplier);
            if (angle > 360)
                angle -= 360;
        }

        if (GameManager.PlayerInput.Right != 0)
        {
            isTurning = true;
            angle -= GameManager.TickSize * (turnSpeed + speed * turnMultiplier);
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

        double lookX = -SludgeUtil.Stabilize(Mathf.Sin((float)(Mathf.Deg2Rad * angle)));
        double lookY = SludgeUtil.Stabilize(Mathf.Cos((float)(Mathf.Deg2Rad * angle)));
        playerX += speed * GameManager.TickSize * lookX;
        playerY += speed * GameManager.TickSize * lookY;
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
    }

    void UpdateTransform()
    {
        trans.rotation = Quaternion.Euler(0, 0, (float)angle);
        trans.position = new Vector3((float)playerX, (float)playerY, 0);

        Position = trans.position;
        Angle = angle;
        Rotation = trans.rotation;
    }
}
