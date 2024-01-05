using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModStalkerLogic : SludgeModifier
{
    public float ChaseForce = 1000.0f;
    public float RotationSpeed = 300.0f;
    public float MaxSpeed = 15.0f;
    public float ApproxBurstDuration = 0.5f;
    public float ApproxBurstCooldown = 4.0f;
    public float BurstForce = 5000;

    ParticleSystem ExhaustParticles;
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
        ExhaustParticles = SludgeUtil.FindByName(transform, "ExhaustParticles").GetComponentInChildren<ParticleSystem>();
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
        rigidBody.velocity = Vector2.zero;
        rigidBody.angularVelocity = 0;
        timeRightInFront = 0;
        burstReadyAt = 0;
        currentBurstEnd = 0;
    }

    float timeRightInFront = 0;
    float burstReadyAt = 0;
    float currentBurstEnd = 0;

    public override void EngineTick()
    {
        if (!rigidBody.simulated)
        {
            rigidBody.simulated = true;
            return;
        }

        Vector3 playerDir = Player.Position - trans.position;

        float distanceToPlayer = playerDir.magnitude;
        bool wallBetweenMeAndPlayer = Physics2D.Raycast(trans.position, playerDir.normalized, distanceToPlayer, SludgeUtil.ScanForWallFilter.layerMask);
        //if (wallBetweenMeAndPlayer)
        //    return;

        bool playerIsDangerous = GameManager.I.Player.Size == Player.PlayerSize.Large;
        if (playerIsDangerous)
        {
            playerDir *= -1;
        }

        float desiredAngle = Mathf.Atan2(playerDir.y, playerDir.x) * Mathf.Rad2Deg - 90;

        // turnspeed is inversely proportional to speed
        float speedPct = Mathf.Clamp01(rigidBody.velocity.magnitude / MaxSpeed);
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

        if ((float)GameManager.I.EngineTime < currentBurstEnd)
        {
            ExhaustParticles.Emit(2);
            rigidBody.AddForce(myLookDir * BurstForce * (float)GameManager.TickSize);
        }
        else
        {
            bool isRightInFront = dot > 0.99f;
            if (isRightInFront)
            {
                timeRightInFront += (float)GameManager.TickSize;
            }
            else
            {
                timeRightInFront = 0;
            }

            bool beginBurst = timeRightInFront > 1 && (float)GameManager.I.EngineTime > burstReadyAt;
            if (beginBurst)
            {
                SoundManager.Play(FxList.Instance.RocketBurst);

                currentBurstEnd = (float)GameManager.I.EngineTime + ApproxBurstDuration + Random.value * 0.25f;
                burstReadyAt = currentBurstEnd + ApproxBurstCooldown + Random.value;
            }
        }

        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, MaxSpeed);
    }
}
