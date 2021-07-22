using Sludge.Utility;
using UnityEngine;

// Super Slug / Turbo Slug
public class Player : MonoBehaviour
{
    public SpriteRenderer bodyRenderer;
    public TrailRenderer trail;
    public bool Alive = false;

    public double angle = 90;
    double speed;
    double minSpeed = 0.5f;
    double breakSpeed = 1;
    double maxSpeed = 10;
    double turnSpeed = 180;
    double turnMultiplier = 8;
    double accelerateSpeed = 100;
    double friction = 25;
    Transform mainTransform;
    ParticleSystem deathParticles;
    double playerX;
    double playerY;

    void Awake()
    {
        deathParticles = GetComponentInChildren<ParticleSystem>();
        var particleMain = deathParticles.main;
        particleMain.startColor = bodyRenderer.color;
        mainTransform = transform;

        Prepare(mainTransform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        deathParticles.Emit(50);
        Alive = false;
    }

    public void Prepare(Vector3 pos)
    {
        speed = minSpeed;
        trail.Clear();
        trail.enabled = false;
        Alive = true;
        angle = 90;
        playerX = SludgeUtil.Stabilize(pos.x);
        playerY = SludgeUtil.Stabilize(pos.y);
        trail.enabled = true;
        UpdateTransform();
    }

    public void EngineTick()
    {
        if (!Alive)
            return;

        bool isTurning = false;

        if (GameManager.PlayerInput.Left != 0)
        {
            isTurning = true;
            angle -= GameManager.TickSize * (turnSpeed + speed * turnMultiplier);
            if (angle < 0)
                angle += 360;
        }

        if (GameManager.PlayerInput.Right != 0)
        {
            isTurning = true;
            angle += GameManager.TickSize * (turnSpeed + speed * turnMultiplier);
            if (angle > 360)
                angle -= 360;
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

        if (GameManager.PlayerInput.Down != 0)
        {
            speed = breakSpeed;
        }

        double lookX = SludgeUtil.Stabilize(Mathf.Sin((float)(Mathf.Deg2Rad * angle)));
        double lookY = SludgeUtil.Stabilize(Mathf.Cos((float)(Mathf.Deg2Rad * angle)));

        playerX += speed * GameManager.TickSize * lookX;
        playerY += speed * GameManager.TickSize * lookY;
        playerX = SludgeUtil.Stabilize(playerX);
        playerY = SludgeUtil.Stabilize(playerY);

        UpdateTransform();
    }

    void UpdateTransform()
    {
        mainTransform.rotation = Quaternion.AngleAxis((float)angle, Vector3.back);
        mainTransform.position = new Vector3((float)playerX, (float)playerY, 0);
    }
}
