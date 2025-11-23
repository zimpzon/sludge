using Assets.Scripts;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

// This enemy will move back and forth on a platform, using raycasts to check when to turn around. If rotated in the inspector
// (only per 90 degrees) movement and raycasts will rotate accordingly, so it can both be on top of platform, below, and on vertical surfaces.
// Uses transform.position for placement.
public class ModStickerLogic : SludgeModifier
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public bool StartLeft = false;
    public bool IsStatic = false;
    public bool FollowPlatforms = true;

    [Header("Detection")]
    public LayerMask PlatformLayer;
    public float RaycastDistanceMultiplier = 1.1f;
    public float WallDetectionDistance = 0.5f;

    private Vector2 basePos;
    private Vector2 moveDirection;
    private Vector2 raycastDirection;
    private Vector2 groundDirection;
    private float initialGroundDistance;
    private bool movingLeft;

    private CircleCollider2D col;
    private float colRadius;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
    }

    public override void OnLoaded()
    {
        if (col != null)
            colRadius = col.radius * Mathf.Abs(transform.lossyScale.x);   // world space radius

        basePos = transform.position;
        // Determine directions based on rotation
        SetupDirections();

        // Set initial movement direction
        movingLeft = StartLeft;

        // Offset raycast origin away from the ground/platform direction
        Vector2 offsetOrigin = (Vector2)transform.position - groundDirection * 0.25f;

        // Perform initial raycast to platform to establish baseline distance
        RaycastHit2D platformHit = Physics2D.Raycast(offsetOrigin, groundDirection, Mathf.Infinity, PlatformLayer);
        if (platformHit.collider != null)
        {
            initialGroundDistance = platformHit.distance;
        }
        else
        {
            Debug.LogWarning($"ModStickerLogic on {gameObject.name}: No platform detected at start!");
            initialGroundDistance = 1f; // Fallback value
        }
    }

    public override void Reset()
    {
        transform.position = basePos;
        movingLeft = StartLeft;
        SetupDirections();
    }

    private void SetupDirections()
    {
        // Get rotation angle (rounded to nearest 90 degrees)
        float angle = Mathf.Round(transform.eulerAngles.z / 90f) * 90f;

        // Calculate directions based on rotation
        // Default (0�): moves horizontally, ground is down
        // 90�: moves vertically, ground is left
        // 180�: moves horizontally, ground is up
        // 270�: moves vertically, ground is right

        switch (Mathf.RoundToInt(angle))
        {
            case 0:
                moveDirection = Vector2.right;
                raycastDirection = Vector2.right;
                groundDirection = Vector2.down;
                break;
            case 90:
                moveDirection = Vector2.up;
                raycastDirection = Vector2.up;
                groundDirection = Vector2.left;
                break;
            case 180:
                moveDirection = Vector2.left;
                raycastDirection = Vector2.left;
                groundDirection = Vector2.up;
                break;
            case 270:
                moveDirection = Vector2.down;
                raycastDirection = Vector2.down;
                groundDirection = Vector2.right;
                break;
            default:
                // Fallback to 0 degrees
                moveDirection = Vector2.right;
                raycastDirection = Vector2.right;
                groundDirection = Vector2.down;
                break;
        }
    }

    private Vector2 GetSideOffset()
    {
        // +moveDirection = right/up depending on rotation.
        // movingLeft flips direction.
        Vector2 sideDir = movingLeft ? -moveDirection : moveDirection;

        // Circle edge
        return (Vector2)transform.position + sideDir * colRadius;
    }

    private Vector2 GetGroundAdjustedOffset(Vector2 sideOffset)
    {
        return sideOffset - groundDirection * 0.05f; // tiny push to avoid inside-collider issues
    }

    public override void EngineTick()
    {
        if (IsStatic)
            return;

        // Move the enemy
        Vector2 movement = (movingLeft ? -moveDirection : moveDirection) * MoveSpeed * (float)GameManager.TickSize;
        transform.position += (Vector3)movement;

        // Check if we should turn around
        if (FollowPlatforms)
        {
            CheckEdge();
        }
        else
        {
            CheckSimpleWallHit();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        if (entity == EntityType.Player)
        {
            GameManager.I.Player.Kill(PlayerDeathType.Saw);
        }
    }

    private void CheckSimpleWallHit()
    {
        Vector2 forwardDir = movingLeft ? -moveDirection : moveDirection;
        float rayDistance = colRadius + 0.2f;

        // Simple forward wall detection
        RaycastHit2D wallHit = Physics2D.Raycast(
            transform.position,
            forwardDir,
            rayDistance,
            PlatformLayer
        );

        if (wallHit.collider != null)
        {
            movingLeft = !movingLeft;
        }
    }

    private void CheckEdge()
    {
        Vector2 sideOffset = GetSideOffset();
        Vector2 offsetOrigin = GetGroundAdjustedOffset(sideOffset);

        // ----- EDGE CHECK -----
        RaycastHit2D groundHit = Physics2D.Raycast(
            offsetOrigin,
            groundDirection,
            Mathf.Infinity,
            PlatformLayer
        );

        if (groundHit.collider == null || groundHit.distance > initialGroundDistance * RaycastDistanceMultiplier)
        {
            movingLeft = !movingLeft;
            return;
        }

        // ----- WALL CHECK -----
        Vector2 forwardDir = movingLeft ? -moveDirection : moveDirection;

        RaycastHit2D wallHit = Physics2D.Raycast(
            offsetOrigin,
            forwardDir,
            WallDetectionDistance,
            PlatformLayer
        );

        if (wallHit.collider != null)
            movingLeft = !movingLeft;
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        // In editor mode, show the raycast direction based on rotation
        if (!Application.isPlaying)
        {
            SetupDirections();

            Vector2 offsetOrigin = (Vector2)transform.position - groundDirection * 0.25f;

            // Ground raycast
            Gizmos.color = Color.green;
            Vector2 rayEnd = offsetOrigin + groundDirection * 2f; // Fixed length for editor visualization
            Gizmos.DrawLine(offsetOrigin, rayEnd);
            Gizmos.DrawWireSphere(rayEnd, 0.1f);

            // Forward wall detection raycast
            Gizmos.color = Color.red;
            Vector2 forwardDir = StartLeft ? -moveDirection : moveDirection;
            Vector2 forwardEnd = offsetOrigin + forwardDir * WallDetectionDistance;
            Gizmos.DrawLine(offsetOrigin, forwardEnd);

            // Show offset origin
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(offsetOrigin, 0.05f);
            return;
        }

        if (FollowPlatforms)
        {
            // Platform following mode - show complex raycasts
            Vector2 playOffsetOrigin = (Vector2)transform.position - groundDirection * 0.25f;

            // Draw platform raycast during play
            Gizmos.color = Color.cyan;
            Vector2 playRayEnd = playOffsetOrigin + groundDirection * (initialGroundDistance * RaycastDistanceMultiplier);
            Gizmos.DrawLine(playOffsetOrigin, playRayEnd);

            // Draw initial distance threshold
            Gizmos.color = Color.yellow;
            Vector2 thresholdPoint = playOffsetOrigin + groundDirection * initialGroundDistance;
            Gizmos.DrawWireSphere(thresholdPoint, 0.1f);

            // Draw wall detection raycast
            Gizmos.color = Color.red;
            Vector2 forwardDirection = movingLeft ? -moveDirection : moveDirection;
            Vector2 wallRayEnd = playOffsetOrigin + forwardDirection * WallDetectionDistance;
            Gizmos.DrawLine(playOffsetOrigin, wallRayEnd);

            // Show offset origin
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(playOffsetOrigin, 0.05f);
        }
        else
        {
            // Simple mode - show only forward wall detection
            Vector2 forwardDir = movingLeft ? -moveDirection : moveDirection;
            float rayDistance = colRadius + 0.2f;

            Gizmos.color = Color.red;
            Vector2 rayEnd = (Vector2)transform.position + forwardDir * rayDistance;
            Gizmos.DrawLine(transform.position, rayEnd);
            Gizmos.DrawWireSphere(rayEnd, 0.05f);

            // Show object radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, colRadius);
        }
    }
}