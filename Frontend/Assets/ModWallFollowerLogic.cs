using Assets.Scripts;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModWallFollowerLogic : SludgeModifier
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public bool IsStatic = false;
    public Vector2 StartDirection = Vector2.right;

    [Header("Detection")]
    public LayerMask PlatformLayer;

    private Vector2 basePos;
    private Vector2 forwardDirection;
    private CircleCollider2D col;
    private float colRadius;
    private float wallDetectionDistance;

    private void Awake()
    {
        basePos = transform.position;
        col = GetComponent<CircleCollider2D>();
        colRadius = col.radius * Mathf.Abs(transform.lossyScale.x);
        wallDetectionDistance = colRadius * 1.2f;
    }

    private void Start()
    {
        forwardDirection = StartDirection.normalized;
    }

    public override void Reset()
    {
        transform.position = basePos;
        forwardDirection = StartDirection.normalized;
    }

    public override void EngineTick()
    {
        if (IsStatic) return;

        Vector2 currentPos = transform.position;

        // Check for wall ahead
        if (CheckForWall(currentPos, forwardDirection))
        {
            // Hit a wall, try to turn
            if (!TryTurn())
            {
                // Both sides blocked, reverse direction
                forwardDirection = -forwardDirection;
            }
        }
        else
        {
            // Path clear, move forward
            transform.position += (Vector3)(forwardDirection * MoveSpeed * Time.deltaTime);
        }
    }

    private bool CheckForWall(Vector2 pos, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, direction, wallDetectionDistance, PlatformLayer);
        return hit.collider != null;
    }

    private bool TryTurn()
    {
        Vector2 currentPos = transform.position;
        Vector2 rightDir = new Vector2(-forwardDirection.y, forwardDirection.x);
        Vector2 leftDir = new Vector2(forwardDirection.y, -forwardDirection.x);

        // Check right first
        if (!CheckForWall(currentPos, rightDir))
        {
            forwardDirection = rightDir;
            return true;
        }
        // Then check left
        else if (!CheckForWall(currentPos, leftDir))
        {
            forwardDirection = leftDir;
            return true;
        }

        // Both sides blocked
        return false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
        {
            GameManager.I.Player.Kill(PlayerDeathType.Follower);
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 currentPos = transform.position;

        // Show current position
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(currentPos, 0.1f);

        // Show collider
        if (col != null)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(currentPos, colRadius);
        }

        // Show forward direction and wall check
        Gizmos.color = Color.green;
        Vector2 forwardEnd = currentPos + forwardDirection * wallDetectionDistance;
        Gizmos.DrawLine(currentPos, forwardEnd);
        Gizmos.DrawWireSphere(forwardEnd, 0.05f);

        // Show side directions
        Vector2 rightDir = new Vector2(-forwardDirection.y, forwardDirection.x);
        Vector2 leftDir = new Vector2(forwardDirection.y, -forwardDirection.x);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(currentPos, currentPos + rightDir * wallDetectionDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(currentPos, currentPos + leftDir * wallDetectionDistance);
    }
}
