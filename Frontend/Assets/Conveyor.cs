using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Conveyor : SludgeObject
{
    const float SuctionPower = 10;
    const float ConveyorSpeed = 4.0f;

    Vector2 beltDirection;
    Vector2 centerLineA;
    Vector2 centerLineB;
    float beltAngle;
    Transform trans;
    bool hasPlayer;
    Material material;

    private void Start()
    {
        trans = transform;
        centerLineA = trans.TransformPoint(Vector2.left * 0.5f);
        centerLineB = trans.TransformPoint(Vector2.right * 0.5f);
        beltDirection = (centerLineB - centerLineA).normalized;
        beltAngle = (float)SludgeUtil.Stabilize(SludgeUtil.AngleNormalized0To360(Mathf.Atan2(centerLineA.x - centerLineB.x, centerLineB.y - centerLineA.y) * Mathf.Rad2Deg));
        material = GetComponent<SpriteRenderer>().material;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = (1 << collision.gameObject.layer == SludgeUtil.PlayerLayerMask);
        if (!isPlayer)
            return;

        GameManager.Instance.Player.OnConveyorBelt++;
        hasPlayer = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        bool isPlayer = (1 << collision.gameObject.layer == SludgeUtil.PlayerLayerMask);
        if (!isPlayer)
            return;

        GameManager.Instance.Player.OnConveyorBelt--;
        hasPlayer = false;
    }

    private void Update()
    {
        material.mainTextureOffset = new Vector2(Time.deltaTime, 0);

        if (!hasPlayer)
            return;

        // Pull player towards center line
        var closestPointOnCenterLine = SludgeUtil.GetClosestPointOnInfiniteLine(Player.Position, centerLineA, centerLineB);
        var directionToCenter = closestPointOnCenterLine - Player.Position;
        GameManager.Instance.Player.AddPositionImpulse(directionToCenter.x * SuctionPower, directionToCenter.y * SuctionPower);

        // Force rotation alignment with the belt
        GameManager.Instance.Player.AddOverrideRotation(beltAngle);

        // Move along the belt
        GameManager.Instance.Player.AddPositionImpulse(beltDirection.x * ConveyorSpeed, beltDirection.y * ConveyorSpeed);
    }
}
