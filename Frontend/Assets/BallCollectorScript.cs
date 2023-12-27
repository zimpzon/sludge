using Assets.Scripts.Game;
using Sludge.Utility;
using UnityEngine;

public class BallCollectorScript : MonoBehaviour
{
    public float speed = 3.0f;
    public Transform displayBody;

    Transform trans;
    Vector3 dir = Vector2.one;
    Rigidbody2D rigidBody;

    private void Awake()
    {
        trans = transform;
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.velocity = Vector3.one * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contactPoint = collision.contacts[0];
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Pickup)
        {
            return;
        }

        PillManager.EatPill(contactPoint.point, PillEater.Ball);
        // bounce on hits
        // add eyes
        // add trail
    }

    private void FixedUpdate()
    {
        if (rigidBody.velocity.sqrMagnitude < speed * speed)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * speed;
        }
    }

    void Update()
    {
        float t = (float)GameManager.I.EngineTime;
    }
}
