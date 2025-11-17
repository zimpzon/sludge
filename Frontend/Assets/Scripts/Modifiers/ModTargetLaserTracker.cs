using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModTargetLaserTracker : SludgeModifier
{
    const float WidthMin = 0.05f;
    const float WidthMax = 0.08f;
    public float KillTime = 0.5f;
    public float BulletSpeed = 5;
    public float BulletDelay = 2.0f;

    public Transform Body;

    LineRenderer lineRenderer;
    Transform trans;
    double timeInSight;
    int nextBulletTimeMs;
    ModTimeToggle timeToggle;

    private void Awake()
    {
        timeToggle = GetComponent<ModTimeToggle>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        trans = transform;
    }

    public override void Reset()
    {
        timeInSight = 0;
        nextBulletTimeMs = 0;
        lineRenderer.enabled = false;
    }

    public override void EngineTick()
    {
        var playerDir = (Player.Position - trans.position);
        const float radius = 0.01f;
        int hit = Physics2D.CircleCast(trans.position, radius, playerDir, SludgeUtil.ScanForPlayerFilter, SludgeUtil.scanHits);
        if (hit == 0)
            return;

        int hitMask = 1 << SludgeUtil.scanHits[0].transform.gameObject.layer;
        bool hasLoS = hitMask == SludgeUtil.PlayerLayerMask;
        lineRenderer.enabled = hasLoS;

        if (!hasLoS)
        {
            timeInSight = 0;
            nextBulletTimeMs = 0;
            return;
        }

        // We have LoS, find out where we hit a wall behind the player.
        Physics2D.Raycast(trans.position, playerDir, SludgeUtil.ScanForWallFilter, SludgeUtil.scanHits);

        double killT = 1.0 - ((KillTime - timeInSight) / KillTime);
        lineRenderer.widthMultiplier = (float)((WidthMax - WidthMin) * killT + WidthMin);

        lineRenderer.SetPosition(0, trans.position);
        lineRenderer.SetPosition(1, SludgeUtil.scanHits[0].point);

        if (timeToggle != null && !timeToggle.IsOn())
            return;

        if (timeInSight >= KillTime)
        {
            if (GameManager.I.EngineTimeMs >= nextBulletTimeMs)
            {
                nextBulletTimeMs = GameManager.I.EngineTimeMs + (int)(BulletDelay * 1000);

                var bullet = BulletManager.Instance.Get();
                if (bullet != null)
                {
                    const float StartOffset = 0.75f;
                    playerDir.Normalize();
                    bullet.DX = SludgeUtil.Stabilize(playerDir.x * BulletSpeed);
                    bullet.DY = SludgeUtil.Stabilize(playerDir.y * BulletSpeed);
                    bullet.X = SludgeUtil.Stabilize(trans.position.x + playerDir.x * StartOffset);
                    bullet.Y = SludgeUtil.Stabilize(trans.position.y + playerDir.y * StartOffset);

                    Body.DOKill();
                    Body.DOPunchScale(Vector3.one * 0.25f, 0.2f);

                    SoundManager.Play(FxList.Instance.EnemyShoot);
                }
            }
        }

        timeInSight = SludgeUtil.Stabilize(timeInSight + GameManager.TickSize);
        if (timeInSight > KillTime)
            timeInSight = KillTime;
    }
}
