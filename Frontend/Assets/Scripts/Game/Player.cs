using Sludge.Utility;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Vector3 Position;

    public SpriteRenderer bodyRenderer;
    public TrailRenderer trail;
    public Transform TentacleLeft;
    public Transform TentacleRight;
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

    public void SetHomePosition()
    {
        Debug.Log($"Setting player home");
        homeX = SludgeUtil.Stabilize(trans.position.x);
        homeY = SludgeUtil.Stabilize(trans.position.y);
        homeAngle = SludgeUtil.Stabilize(trans.rotation.eulerAngles.z);
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
        Kill();
        Debug.Log($"frame: {GameManager.Instance.FrameCounter}");
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

        playerX = homeX;
        playerY = homeY;
        angle = homeAngle;

        UpdateTransform();

        trail.enabled = true;
    }

    public void EngineTick()
    {
        Debug.Log($"Player.Tick, alive: {Alive}");

        if (!Alive)
            return;

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
        }

        angle = SludgeUtil.Stabilize(angle);

        speed = SludgeUtil.Stabilize(speed - GameManager.TickSize * friction);
        if (speed < minSpeed)
            speed = minSpeed;

        if (GameManager.PlayerInput.Up != 0)
        {
            speed = SludgeUtil.Stabilize(speed + GameManager.TickSize * accelerateSpeed);
            if (speed > maxSpeed)
                speed = maxSpeed;
        }

        double lookX = -SludgeUtil.Stabilize(Mathf.Sin((float)(Mathf.Deg2Rad * angle)));
        double lookY = SludgeUtil.Stabilize(Mathf.Cos((float)(Mathf.Deg2Rad * angle)));

        playerX += speed * GameManager.TickSize * lookX;
        playerY += speed * GameManager.TickSize * lookY;
        playerX = SludgeUtil.Stabilize(playerX);
        playerY = SludgeUtil.Stabilize(playerY);

        UpdateTransform();
    }

    void UpdateTransform()
    {
        trans.rotation = Quaternion.Euler(0, 0, (float)angle);
        trans.position = new Vector3((float)playerX, (float)playerY, 0);

        Position = trans.position;
    }
}
