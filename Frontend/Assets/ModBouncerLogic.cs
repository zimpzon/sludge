using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModBouncerLogic : SludgeModifier
{
    public float speed = 8.0f;
    public Vector2 startDirection = new Vector2(1, 1).normalized;
    public LayerMask bounceLayer;

    Vector2 basePos;
    Vector2 velocity;
    float bouncerRadius;

    public override void Reset()
    {
        transform.position = basePos;
        velocity = startDirection.normalized * speed;
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
}