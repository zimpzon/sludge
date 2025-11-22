using Assets.Scripts;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;


// Simple enemy that moves back and forth in a straight line, turning around only when hitting walls directly in its path.
// Rotation in inspector (90 degree increments) changes movement direction: 0°=horizontal, 90°=vertical, etc.
// Perfect for placing in open areas away from platforms - no edge detection or wall-following behavior.
public class ModStickerLogic : SludgeModifier
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public bool StartLeft = false;
    public bool IsStatic = false;

    [Header("Detection")]
    public LayerMask WallLayer;

    private Vector2 basePos;
    private Vector2 moveDirection;
    private bool movingLeft;

    private CircleCollider2D col;
    private float colRadius;
    bool isFirstFrame = true;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        colRadius = col.radius * Mathf.Abs(transform.lossyScale.x);   // world space radius
    }

    public override void OnLoaded()
    {
        basePos = transform.position;
    }

    private void Start()
    {
        // Determine directions based on rotation
        SetupDirections();

        // Set initial movement direction
        movingLeft = StartLeft;
    }

    public override void Reset()
    {
        transform.position = basePos;
        movingLeft = StartLeft;
        isFirstFrame = true;
        SetupDirections();
    }

    private void SetupDirections()
    {
        // Get rotation angle (rounded to nearest 90 degrees)
        float angle = Mathf.Round(transform.eulerAngles.z / 90f) * 90f;

        // Calculate movement direction based on rotation
        switch (Mathf.RoundToInt(angle))
        {
            case 0:
                moveDirection = Vector2.right;
                break;
            case 90:
                moveDirection = Vector2.up;
                break;
            case 180:
                moveDirection = Vector2.left;
                break;
            case 270:
                moveDirection = Vector2.down;
                break;
            default:
                // Fallback to 0 degrees
                moveDirection = Vector2.right;
                break;
        }
    }


    public override void EngineTick()
    {
        if (IsStatic)
            return;

        if (isFirstFrame)
        {
            isFirstFrame = false;
            return;
        }

        // Move the enemy
        Vector2 movement = (movingLeft ? -moveDirection : moveDirection) * MoveSpeed * Time.deltaTime;
        transform.position += (Vector3)movement;

        // Check if we should turn around
        CheckWallHit();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        if (entity == EntityType.Player)
        {
            GameManager.I.Player.Kill(PlayerDeathType.Saw);
        }
    }

    private void CheckWallHit()
    {
        // Simple forward wall detection - raycast in movement direction
        Vector2 forwardDir = movingLeft ? -moveDirection : moveDirection;

        // Raycast from center of object in movement direction
        float rayDistance = colRadius + 0.2f; // A bit more distance for better detection
        RaycastHit2D wallHit = Physics2D.Raycast(
            transform.position,
            forwardDir,
            rayDistance,
            WallLayer
        );

        // Turn around if we hit a wall
        if (wallHit.collider != null)
        {
            movingLeft = !movingLeft;
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        // Setup directions for visualization
        if (!Application.isPlaying)
        {
            SetupDirections();
        }

        // Show movement direction and raycast range
        Vector2 forwardDir = movingLeft ? -moveDirection : moveDirection;
        float rayDistance = colRadius + 0.2f; // Same as CheckWallHit

        Gizmos.color = Color.red;
        Vector3 rayEnd = transform.position + (Vector3)(forwardDir * rayDistance);
        Gizmos.DrawLine(transform.position, rayEnd);
        Gizmos.DrawWireSphere(rayEnd, 0.05f);

        // Show object radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, colRadius);
    }
}