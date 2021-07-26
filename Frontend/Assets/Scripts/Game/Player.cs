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
    double minSpeed = 0;//0.5f;
    double breakSpeed = 1;
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
    RaycastHit2D[] scanHits = new RaycastHit2D[1];
    float tentacleLeftTargetScale = 1.0f;
    float tentacleRightTargetScale = 1.0f;
    float tentacleLeftScale = 1.0f;
    float tentacleRightScale = 1.0f;

    public void SetHomePosition()
    {
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
        deathParticles.Emit(50);
        Alive = false;
    }

    public void Prepare()
    {
        speed = minSpeed;
        trail.Clear();
        trail.enabled = false;
        Alive = true;

        playerX = homeX;
        playerY = homeY;
        angle = homeAngle;

        UpdateTransform();
        tentacleLeftTargetScale = 0;
        tentacleLeftScale = 0;
        tentacleRightTargetScale = 0;
        tentacleRightScale = 0;
        TentacleLeft.localScale = Vector2.zero;
        TentacleRight.localScale = Vector2.zero;

        trail.enabled = true;
    }

    public void EngineTick()
    {
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

        // Snapping might be detrimental when we have tentacles for feeling. The right angles are impossible to correct, better to approach a wall at an angle.
        if (false && !isTurning)
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

        double lookX = -SludgeUtil.Stabilize(Mathf.Sin((float)(Mathf.Deg2Rad * angle)));
        double lookY = SludgeUtil.Stabilize(Mathf.Cos((float)(Mathf.Deg2Rad * angle)));

        const float TentacleScanDistance = 0.5f;

        bool TentacleHit(Vector2 tentacleRoot, Vector2 tentacleDir, out double distance, out double correctionForce)
        {
            distance = 0;
            correctionForce = 0;

            int hits = Physics2D.Raycast(tentacleRoot, tentacleDir, wallScanFilter, scanHits, TentacleScanDistance);
            if (hits == 0)
                return false;

            var hitNormal = scanHits[0].normal;
            // 0 = head on, 1 = parallel, 0.707 = exactly 45 degrees.
            float facing = (float)SludgeUtil.Stabilize(Mathf.Abs(-tentacleDir.x * hitNormal.y + tentacleDir.y * hitNormal.x));

            // The tentacle is scanning at a 45 degree angle, so...
            //  0.707 = we are moving parallel to the wall
            //  0.000 = we are moving straight into the wall.
            // <0.000 = the tentacle on the other side must take care of this.
            // For each tentacle get distance and correction force. Only use the result of the closest tentacle.
            bool leaveItToOtherTentacle = facing <= 0;
            if (leaveItToOtherTentacle)
                return false;

            double collisionDanger01 = SludgeUtil.Stabilize(1 - ((1.0 / 0.707) * facing)); // Scale to 0..1 where <= 0 = no danger and 1 = ouch
            distance = scanHits[0].distance;
            correctionForce = collisionDanger01;

            return true;
        }

        // Scan 45 degrees left and right in front of player (tentacles/antennee)
        var scanLeft = SludgeUtil.LookAngle(angle + 45);
        var scanRight = SludgeUtil.LookAngle(angle - 45);
        var tentacleRootPos = new Vector2((float)(playerX + lookX * 0.25), (float)(playerY + lookY * 0.25));

        bool tentacleHitLeft = TentacleHit(tentacleRootPos, scanLeft, out double distanceLeft, out double correctionLeft);
        bool tentacleHitRight = TentacleHit(tentacleRootPos, scanRight, out double distanceRight, out double correctionRight);
        Debug.DrawRay(tentacleRootPos, scanLeft * TentacleScanDistance, tentacleHitLeft ? Color.red : Color.green);
        Debug.DrawRay(tentacleRootPos, scanRight * TentacleScanDistance, tentacleHitRight ? Color.red : Color.green);

        if (tentacleHitLeft)
            tentacleLeftScale = 0.1f;
        if (tentacleHitRight)
            tentacleRightScale = 0.1f;

        // Don't use tentacles when turning, they can prevent turning which is very annoying!
        if (!isTurning && (tentacleHitLeft || tentacleHitRight))
        {
            double correctionSign = distanceLeft > distanceRight ? -1 : 1;
            double correctionForce = distanceLeft > distanceRight ? correctionLeft : correctionRight;
            const double CorrectionScale = 1000;
            angle += SludgeUtil.Stabilize(correctionSign * correctionForce * GameManager.TickSize * CorrectionScale);
        }

        void ScaleTentacle(Transform tentacleTrans, ref float scale, float targetScale)
        {
            float diffSign = Mathf.Sign(targetScale - scale);
            scale += (float)(GameManager.TickSize * diffSign * 1.0f);

            float scaleX = scale * 0.5f + 0.5f; // Range X: 1 - 0.5
            float scaleY = scale * 0.25f + 0.75f; // Range Y: 1 - 0.75
            tentacleTrans.localScale = new Vector2(scaleX, scaleY);
        }

        tentacleLeftTargetScale = (Mathf.Sin((float)GameManager.Instance.EngineTime * 3) + 1) * 0.1f + 0.9f;
        tentacleRightTargetScale = (Mathf.Cos((float)GameManager.Instance.EngineTime * 3) + 1) * 0.1f + 0.9f;

        ScaleTentacle(TentacleLeft, ref tentacleLeftScale, tentacleLeftTargetScale);
        ScaleTentacle(TentacleRight, ref tentacleRightScale, tentacleRightTargetScale);

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
