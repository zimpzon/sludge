using Assets.Scripts;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModContourFollower : SludgeModifier
{
    [Header("Movement")]
    public float MoveSpeed = 1.5f;
    public float SurfaceDistance = 0.6f; // Distance to maintain from surfaces
    public bool ClockwiseMovement = true;
    public bool IsStatic = false;

    [Header("Detection")]
    public LayerMask SurfaceLayer;
    public int RayCount = 12; // Number of rays to cast in a circle
    public float DetectionRadius = 2.0f; // Max distance to detect surfaces

    private Vector2 basePos;
    private Vector2 currentDirection = Vector2.right;
    private Vector2 lastSurfaceNormal = Vector2.up;
    private CircleCollider2D col;
    private float colRadius;
    private Rigidbody2D rb;
    bool isFirstFrame = true;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        colRadius = col.radius * Mathf.Abs(transform.lossyScale.x);

        // Setup Rigidbody2D
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
        currentDirection = Vector2.right;
        lastSurfaceNormal = Vector2.up;
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

        // Find the closest surface using radial raycasting
        SurfaceInfo surfaceInfo = FindClosestSurface(currentPos);

        if (surfaceInfo.hasHit)
        {
            // Calculate desired position maintaining distance from surface
            Vector2 idealPos = surfaceInfo.point + surfaceInfo.normal * (SurfaceDistance + colRadius);

            // Calculate tangent direction for movement
            Vector2 tangent = Vector2.Perpendicular(surfaceInfo.normal);
            if (!ClockwiseMovement) tangent = -tangent;

            // Adjust tangent to maintain current movement flow
            if (Vector2.Dot(tangent, currentDirection) < 0)
                tangent = -tangent;

            currentDirection = tangent.normalized;
            lastSurfaceNormal = surfaceInfo.normal;

            // Move along the surface contour
            Vector2 targetPos = idealPos + currentDirection * MoveSpeed * (float)GameManager.TickSize;

            // Smooth movement towards target position
            Vector2 moveVector = Vector2.Lerp(currentPos, targetPos, 0.8f) - currentPos;
            rb.MovePosition(currentPos + moveVector);
        }
        else
        {
            // No surface found - continue in current direction but slower
            Vector2 fallbackMove = currentDirection * MoveSpeed * 0.3f * (float)GameManager.TickSize;
            rb.MovePosition(currentPos + fallbackMove);
        }
    }

    private SurfaceInfo FindClosestSurface(Vector2 origin)
    {
        float closestDistance = float.MaxValue;
        Vector2 closestPoint = Vector2.zero;
        Vector2 averageNormal = Vector2.zero;
        bool hasHit = false;
        int hitCount = 0;

        // Cast rays in a circle to find surfaces
        for (int i = 0; i < RayCount; i++)
        {
            float angle = (i * 360f / RayCount) * Mathf.Deg2Rad;
            Vector2 rayDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, DetectionRadius, SurfaceLayer);
            if (hit.collider != null)
            {
                hasHit = true;
                hitCount++;

                // Weight closer hits more heavily
                float distance = hit.distance;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = hit.point;
                }

                // Accumulate normals for smoother surface following
                averageNormal += hit.normal;
            }
        }

        if (hasHit && hitCount > 0)
        {
            averageNormal = (averageNormal / hitCount).normalized;
            return new SurfaceInfo
            {
                hasHit = true,
                point = closestPoint,
                normal = averageNormal,
                distance = closestDistance
            };
        }

        return new SurfaceInfo { hasHit = false };
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

        // Show detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPos, DetectionRadius);

        // Show surface distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(currentPos, SurfaceDistance + colRadius);

        // Show current direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(currentPos, currentPos + currentDirection * 0.8f);

        // Show surface normal
        Gizmos.color = Color.red;
        Gizmos.DrawLine(currentPos, currentPos + lastSurfaceNormal * 0.5f);

        // Show raycast pattern
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < RayCount; i++)
            {
                float angle = (i * 360f / RayCount) * Mathf.Deg2Rad;
                Vector2 rayDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 rayEnd = currentPos + rayDirection * DetectionRadius * 0.3f;
                Gizmos.DrawLine(currentPos, rayEnd);
            }
        }
    }

    private struct SurfaceInfo
    {
        public bool hasHit;
        public Vector2 point;
        public Vector2 normal;
        public float distance;
    }
}