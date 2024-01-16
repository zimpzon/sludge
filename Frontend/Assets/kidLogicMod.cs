using Sludge.Modifiers;
using UnityEngine;

public class KidLogicMod : SludgeModifier
{
    public Transform TargetTransform;
    public float moveSpeed = 2.0f;        // Speed of movement
    public float jumpForce = 5.0f;        // Force of the jump
    public float jumpCooldown = 2.0f;     // Cooldown time for jumping in seconds

    private Rigidbody2D _rigidbody;
    private float _lastJumpTime;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _lastJumpTime = -jumpCooldown;   // Initialize so the character can jump immediately
        _rigidbody.centerOfMass = Vector2.down * 0.95f;
    }

    public override void EngineTick()
    {
        return;

        float delta = (float)GameManager.TickSize;

        // Horizontal movement towards the target
        MoveTowardsTarget();

        // Jump logic
        if (Time.time > _lastJumpTime + jumpCooldown)
        {
            Jump();
            _lastJumpTime = Time.time + Random.value * 2;
        }
    }

    private void MoveTowardsTarget()
    {
        if (TargetTransform != null)
        {
            Vector2 direction = (TargetTransform.position - transform.position).normalized;
            _rigidbody.velocity = new Vector2(direction.x * moveSpeed, _rigidbody.velocity.y);
        }
    }

    private void Jump()
    {
        // Add vertical force for the jump
        _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
