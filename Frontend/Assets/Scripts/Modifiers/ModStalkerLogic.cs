using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModStalkerLogic : SludgeModifier
{
    float ChaseForce = 500.0f;
    float RotationSpeed = 100.0f;
    float MaxSpeed = 8.0f;

    Transform trans;
    Rigidbody2D rigidBody;
    AnimatedAnt ant;
    Vector3 basePos;
    Quaternion baseRot;

    private void Awake()
    {
        trans = transform;
        rigidBody = GetComponent<Rigidbody2D>();
        ant = GetComponentInChildren<AnimatedAnt>();
        ant.animationOffset = Mathf.Clamp01((float)(basePos.x * 0.117 + basePos.y * 0.3311));
        ant.animationSpeedScale = 1;
    }

    public override void OnLoaded()
    {
        trans = transform;
        basePos = trans.position;
        baseRot = trans.rotation;
    }

    public override void Reset()
    {
        ant.animationSpeedScale = 1;
        trans.position = basePos;
        trans.rotation = baseRot;
        rigidBody.simulated = false;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0;
        timeRightInFront = 0;
        burstReadyAt = 0;
        currentBurstEnd = 0;
    }

    float timeRightInFront = 0;
    float burstReadyAt = 0;
    float currentBurstEnd = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        if (entity == EntityType.Player)
        {
            GameManager.I.Player.Kill(Assets.Scripts.PlayerDeathType.None);
        }
    }

    public override void EngineTick()
    {
        if (!rigidBody.simulated)
        {
            rigidBody.simulated = true;
            return;
        }

        Vector3 playerDir = Player.Position - trans.position;

        float distanceToPlayer = playerDir.magnitude;
        //bool wallBetweenMeAndPlayer = Physics2D.Raycast(trans.position, playerDir.normalized, distanceToPlayer, SludgeUtil.ScanForWallFilter.layerMask);
        //if (wallBetweenMeAndPlayer)
        //    return;

        bool playerIsDangerous = GameManager.I.Player.Size == Player.PlayerSize.Large;
        if (playerIsDangerous)
        {
            playerDir *= -1;
        }

        float desiredAngle = Mathf.Atan2(playerDir.y, playerDir.x) * Mathf.Rad2Deg - 90;

        // turnspeed is inversely proportional to speed
        float speedPct = Mathf.Clamp01(rigidBody.linearVelocity.magnitude / MaxSpeed);
        float scaledTurnSpeed = RotationSpeed * (1 - Mathf.Clamp(speedPct, 0.0f, 0.75f));

        var targetRot = Quaternion.Euler(0, 0, desiredAngle);
        float step = RotationSpeed * (float)GameManager.TickSize * scaledTurnSpeed;

        trans.rotation = Quaternion.RotateTowards(trans.rotation, targetRot, step);
        Vector2 myLookDir = trans.localRotation * Vector2.up;

        // facing player dot: -1 directly away, 0 perpendicular, 1 directly towards
        float dot = Vector2.Dot(playerDir, myLookDir);

        // use less force the more wrong the desired direction is
        float force = ChaseForce * Mathf.Clamp01(dot) * (float)GameManager.TickSize;
        rigidBody.AddForce(myLookDir * force);

        bool isRightInFront = dot > 0.99f;
        if (isRightInFront)
        {
            timeRightInFront += (float)GameManager.TickSize;
        }
        else
        {
            timeRightInFront = 0;
        }

        rigidBody.linearVelocity = Vector3.ClampMagnitude(rigidBody.linearVelocity, MaxSpeed);
    }
}
