using Assets.Scripts;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModContourFollowerSimple : SludgeModifier
{
    [Header("Movement")]
    public float MoveSpeed = 1.5f;
    public bool KeepLeftWall = true; // true = keep left wall, false = keep right wall
    public bool IsStatic = false;

    [Header("Detection")]
    public LayerMask SurfaceLayer;

    private Vector2 basePos;
    private Vector2 forwardDirection = Vector2.right;
    private CircleCollider2D col;
    private float colRadius;
    private float wallDetectionDistance;
    private Rigidbody2D rb;
    bool isFirstFrame = true;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        colRadius = col.radius * Mathf.Abs(transform.lossyScale.x);
        wallDetectionDistance = colRadius * 1.8f;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void OnLoaded()
    {
        basePos = transform.position;
    }

    public override void Reset()
    {
        rb.position = basePos;
        transform.position = basePos;
        forwardDirection = Vector2.right;
        rb.linearVelocity = Vector2.zero;
        isFirstFrame = true;
    }

    public override void EngineTick()
    {
        if (IsStatic || isFirstFrame)
        {
            isFirstFrame = false;
            return;
        }

        Vector2 currentPos = rb.position;

        // Simple approach: just move forward and turn when hitting walls
        RaycastHit2D forwardHit = Physics2D.Raycast(currentPos, forwardDirection, wallDetectionDistance, SurfaceLayer);

        if (forwardHit.collider != null)
        {
            // Hit a wall ahead, turn
            float turnAngle = KeepLeftWall ? -90f : 90f; // Turn left or right based on preference
            forwardDirection = RotateVector(forwardDirection, turnAngle * Mathf.Deg2Rad);
        }
        else
        {
            // Path is clear, move forward
            Vector2 newPos = currentPos + forwardDirection * MoveSpeed * (float)GameManager.TickSize;
            rb.MovePosition(newPos);
        }
    }

    private Vector2 RotateVector(Vector2 vector, float angleRadians)
    {
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);
        return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
        {
            GameManager.I.Player.Kill(PlayerDeathType.Stalker);
        }
    }

    private void OnDrawGizmos()
    {
        if (rb == null) return;

        Vector2 currentPos = rb.position;

        // Show forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(currentPos, currentPos + forwardDirection * wallDetectionDistance);

        // Show wall detection direction
        Vector2 wallDirection = KeepLeftWall ?
            new Vector2(-forwardDirection.y, forwardDirection.x) :
            new Vector2(forwardDirection.y, -forwardDirection.x);

        Gizmos.color = KeepLeftWall ? Color.green : Color.red;
        Gizmos.DrawLine(currentPos, currentPos + wallDirection * wallDetectionDistance);

        // Show detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPos, wallDetectionDistance);
    }
}