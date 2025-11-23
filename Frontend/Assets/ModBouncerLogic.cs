using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModBouncerLogic : SludgeModifier
{
    public float speed = 8.0f;
    public Vector2 startDirection = new Vector2(1, 1).normalized;
    public LayerMask bounceLayer;

    Transform eye;
    Transform pupil;
    float eyeScale;
    float eyeScaleTarget;
    System.Random rnd;
    Vector2 basePos;
    Vector2 velocity;
    float bouncerRadius;

    public override void Reset()
    {
        transform.position = basePos;
        velocity = startDirection.normalized * speed;
    }

    private void Awake()
    {
        eye = transform.Find("Eye").transform;
        pupil = transform.Find("Pupil").transform;
        rnd = new System.Random((int)(transform.position.x * 100 + transform.position.y * 100));

    }

    public override void OnLoaded()
    {
        basePos = transform.position;
        // Get the bouncer's radius from its collider
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            bouncerRadius = collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        }
        else
        {
            bouncerRadius = 0.5f; // Default fallback
        }
        Reset();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        if (entity == EntityType.Player)
        {
            Player.I.Kill(Assets.Scripts.PlayerDeathType.None);
            return;
        }

        // Bounce off objects in the bounce layer
        if (((1 << collision.gameObject.layer) & bounceLayer) != 0)
        {
            // Always bounce at 45-degree angles - flip X or Y component
            Vector2 newVelocity = velocity;

            // Determine which axis to flip based on the collision direction
            Vector2 relativePosition = transform.position - collision.transform.position;

            if (Mathf.Abs(relativePosition.x) > Mathf.Abs(relativePosition.y))
            {
                // Hit from horizontal side - flip X
                newVelocity.x = -newVelocity.x;
            }
            else
            {
                // Hit from vertical side - flip Y
                newVelocity.y = -newVelocity.y;
            }

            velocity = newVelocity.normalized * speed;
        }
    }

    void UpdateEye()
    {
        var playerDir = Player.Position - transform.position;
        float sqrPlayerDist = playerDir.sqrMagnitude;
        playerDir.Normalize();
        const float SqrLookRange = 999 * 999;
        const float MaxScale = 0.9f;

        if (GameManager.I.FrameCounter != 0)
        {
            bool playerIsClose = sqrPlayerDist < SqrLookRange;
            bool hasOpenEye = playerIsClose;
            eyeScaleTarget = hasOpenEye ? MaxScale : 0;
        }

        eyeScale += (float)((eyeScaleTarget > eyeScale) ? GameManager.TickSize * 4.0f : -GameManager.TickSize * 4.0f);
        eyeScale = Mathf.Clamp(eyeScale, 0, MaxScale);
        bool doBlink = rnd.NextDouble() < (1 / 200.0);
        if (doBlink)
            eyeScale = 0;

        pupil.localPosition = eyeScale < 0.2f ? Vector2.one * 10000 : new Vector2(playerDir.x * 0.15f, playerDir.y * 0.08f * MaxScale);
    }

    public override void EngineTick()
    {
        Vector2 movement = velocity * (float)GameManager.TickSize;

        // Spherecast to check for walls before moving (accounts for bouncer size)
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            bouncerRadius,
            movement.normalized,
            movement.magnitude,
            bounceLayer
        );

        if (hit.collider != null)
        {
            // Hit a wall - flip velocity component based on hit direction
            Vector2 relativePosition = transform.position - (Vector3)hit.point;

            if (Mathf.Abs(relativePosition.x) > Mathf.Abs(relativePosition.y))
            {
                // Hit from horizontal side - flip X
                velocity.x = -velocity.x;
            }
            else
            {
                // Hit from vertical side - flip Y
                velocity.y = -velocity.y;
            }

            // Move a tiny bit away from the wall
            Vector2 awayFromWall = (transform.position - (Vector3)hit.point).normalized * 0.1f;
            transform.position += (Vector3)awayFromWall;
        }
        else
        {
            // No collision, move normally
            transform.position += (Vector3)movement;
        }
    }

    void Update()
    {
        UpdateEye();
        eye.transform.localScale = new Vector2(1, eyeScale);
    }
}