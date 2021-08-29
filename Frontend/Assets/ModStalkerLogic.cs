using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

// Do not overuse this guy. Small positions errors can get out of hand when turning corners.
public class ModStalkerLogic : SludgeModifier
{
    //double[] xx = new double[60 * 60];
    //double[] yy = new double[60 * 60];

    float speedNear = 3;
    float speedFar = 2;
    Transform trans;
    Collider2D myCollider;
    AnimatedAnt ant;
    RaycastHit2D[] scanHits = new RaycastHit2D[1];
    ContactFilter2D scanFilter = new ContactFilter2D();
    double posX;
    double posY;
    double basePosX;
    double basePosY;

    private void Awake()
    {
        trans = transform;
        myCollider = GetComponent<Collider2D>();
        ant = GetComponentInChildren<AnimatedAnt>();
        ant.GetComponent<CircleCollider2D>().enabled = false;
        scanFilter.SetLayerMask(SludgeUtil.WallsAndObjectsLayerMask);
        basePosX = SludgeUtil.Stabilize(trans.position.x);
        basePosY = SludgeUtil.Stabilize(trans.position.y);
        ant.animationOffset = Mathf.Clamp01((float)(basePosX * 0.117 + basePosY * 0.3311));
        ant.animationSpeedScale = 1;
    }

    public override void Reset()
    {
        ant.animationSpeedScale = 1;
        posX = basePosX;
        posY = basePosY;
        UpdateTransform(angle: 0);
    }
    
    void UpdateTransform(double angle)
    {
        trans.rotation = Quaternion.Euler(0, 0, (float)angle);
        trans.position = new Vector3((float)posX, (float)posY, 0);
    }

    public override void EngineTick()
    {
        var playerDir = Player.Position - trans.position;
        double playerDistance = SludgeUtil.Stabilize(playerDir.magnitude);
        ant.animationSpeedScale = playerDistance > 5 ? 1 : 2;
        double speed = playerDistance > 5 ? speedFar : speedNear;

        playerDir *= (float)SludgeUtil.Stabilize(1.0 / playerDistance);
        playerDir.x = (float)SludgeUtil.Stabilize(playerDir.x);
        playerDir.y = (float)SludgeUtil.Stabilize(playerDir.y);

        //if (!GameManager.Instance.IsReplay)
        //{
        //    xx[GameManager.Instance.FrameCounter] = posX;
        //    yy[GameManager.Instance.FrameCounter] = posY;
        //}
        //else
        //{
        //    float errorX = Mathf.Abs((float)(xx[GameManager.Instance.FrameCounter] - posX));
        //    float errorY = Mathf.Abs((float)(yy[GameManager.Instance.FrameCounter] - posY));
        //    float error = errorX + errorY;
        //    DebugLinesScript.Show(gameObject.name, error);
        //}

        double angle = SludgeUtil.Stabilize(Mathf.Atan2(-playerDir.x, playerDir.y) * Mathf.Rad2Deg);

        const float Stopdistance = 0.15f;

        var scanDir = playerDir;
        int hitCount = myCollider.Cast(scanDir, scanFilter, scanHits, Stopdistance);
        if (hitCount >= 1)
        {
            var scanA = scanDir.x > scanDir.y ? new Vector2(scanDir.x, 0) : new Vector2(0, scanDir.y);
            var scanB = scanDir.x > scanDir.y ? new Vector2(0, scanDir.y) : new Vector2(scanDir.x, 0);
            hitCount = myCollider.Cast(scanA, scanFilter, scanHits, Stopdistance);
            if (hitCount >= 1)
            {
                hitCount = myCollider.Cast(scanB, scanFilter, scanHits, Stopdistance);
                if (hitCount >= 1)
                {
                    UpdateTransform(angle);
                    return;
                }
                else
                {
                    scanDir = scanB;
                }
            }
            else
            {
                scanDir = scanA;
            }
        }

        posX = SludgeUtil.Stabilize(posX + speed * GameManager.TickSize * scanDir.x);
        posY = SludgeUtil.Stabilize(posY + speed * GameManager.TickSize * scanDir.y);

        UpdateTransform(angle);
    }
}
