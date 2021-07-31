using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModTargetLaserTracker : SludgeModifier
{
    const float WidthMin = 0.001f;
    const float WidthMax = 0.15f;
    const float KillTime = 2f;

    LineRenderer lineRenderer;
    RaycastHit2D[] scanHits = new RaycastHit2D[1];
    ContactFilter2D scanForPlayerFilter = new ContactFilter2D();
    ContactFilter2D scanForWallFilter = new ContactFilter2D();
    Transform trans;
    double timeInSight;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        trans = transform;
        scanForPlayerFilter.SetLayerMask(SludgeUtil.ScanForPlayerLayerMask);
        scanForWallFilter.SetLayerMask(SludgeUtil.ScanForWallsLayerMask);
    }

    public override void Reset()
    {
        timeInSight = 0;
    }

    public override void EngineTick()
    {
        var playerDir = (Player.Position - trans.position);
        playerDir.x = (float)SludgeUtil.Stabilize(playerDir.x);
        playerDir.y = (float)SludgeUtil.Stabilize(playerDir.y);

        var hit = Physics2D.Raycast(trans.position, playerDir, scanForPlayerFilter, scanHits);
        int hitMask = 1 << scanHits[0].transform.gameObject.layer;
        bool hasLoS = hitMask == SludgeUtil.PlayerLayerMask;
        lineRenderer.enabled = hasLoS;

        if (!hasLoS)
        {
            timeInSight = 0;
            return;
        }

        // We have LoS, find out where we hit a wall behind the player.
        hit = Physics2D.Raycast(trans.position, playerDir, scanForWallFilter, scanHits);

        double killT = 1.0 - ((KillTime - timeInSight) / KillTime);
        lineRenderer.widthMultiplier = (float)((WidthMax - WidthMin) * killT + WidthMin);

        lineRenderer.SetPosition(0, trans.position);
        lineRenderer.SetPosition(1, scanHits[0].point);

        if (timeInSight >= KillTime)
        {
            timeInSight = 0;
            Debug.Log("DIE");
        }

        timeInSight += GameManager.TickSize;
    }
}
