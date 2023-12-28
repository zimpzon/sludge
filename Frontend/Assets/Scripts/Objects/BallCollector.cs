using Assets.Scripts.Game;
using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class BallCollector : SludgeObject
{
    public override EntityType EntityType => EntityType.BallCollector;

    public float speed = 6.0f;
    public float ScaleAtFullSize = 0.65f;

    public Transform displayBody;
    Transform eye;
    Transform pupil;
    float eyeScale;
    float eyeScaleTarget;
    Transform trans;
    System.Random rnd;
    Vector2 basePos;
    Vector2 baseScale;

    Rigidbody2D rigidBody;
    bool isHeld;

    public override void Reset()
    {
        trans.position = basePos;
        trans.localScale = baseScale;
        rigidBody.velocity = Vector3.one * speed;
        base.Reset();
    }

    public void HoldPosition(bool hold)
    {
        if (hold)
        {
            isHeld = true;
            rigidBody.simulated = false;
            rigidBody.constraints |= RigidbodyConstraints2D.FreezePositionX;
            rigidBody.constraints |= RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            isHeld = false;
            rigidBody.simulated = true;
            rigidBody.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            rigidBody.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            rigidBody.velocity = Vector3.one;
        }
    }

    private void Awake()
    {
        trans = transform;
        basePos = trans.position;
        baseScale = trans.localScale;

        rigidBody = GetComponent<Rigidbody2D>();
        eye = transform.Find("Eye").transform;
        pupil = transform.Find("Pupil").transform;
        rnd = new System.Random((int)(trans.position.x * 100 + trans.position.y * 100));

        Reset();
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
        // add trail
    }

    void UpdateEye()
    {
        var playerDir = Player.Position - trans.position;
        float sqrPlayerDist = playerDir.sqrMagnitude;
        playerDir.Normalize();

        const float SqrLookRange = 5 * 5;
        const float MaxScale = 0.9f;

        if (GameManager.I.FrameCounter != 0) // Hacky hacky: EngineTick gets called once before starting round. Don't begin to open eyes even if player is in range.
            eyeScaleTarget = sqrPlayerDist < SqrLookRange ? MaxScale : 0;

        eyeScale += (float)((eyeScaleTarget > eyeScale) ? GameManager.TickSize * 4.0f : -GameManager.TickSize * 4.0f);
        eyeScale = Mathf.Clamp(eyeScale, 0, MaxScale);

        bool doBlink = rnd.NextDouble() < (1 / 200.0);
        if (doBlink)
            eyeScale = 0;

        pupil.localPosition = eyeScale < 0.2f ? Vector2.one * 10000 : new Vector2(playerDir.x * 0.15f, playerDir.y * 0.08f * MaxScale);
    }

    private void FixedUpdate()
    {
        if (isHeld)
            return;

        float currentScale = trans.localScale.x;
        if (currentScale < ScaleAtFullSize)
            currentScale += Time.deltaTime;
        else if (currentScale > ScaleAtFullSize)
            currentScale -= Time.deltaTime;

        trans.localScale = Vector3.one * currentScale;

        const float RecoverSpeed = 0.5f;
        if (rigidBody.velocity.sqrMagnitude < speed * speed)
        {
            rigidBody.velocity += rigidBody.velocity.normalized * Time.fixedDeltaTime * speed * RecoverSpeed;
        } else if (rigidBody.velocity.sqrMagnitude > speed * speed)
        {
            rigidBody.velocity -= rigidBody.velocity.normalized * Time.fixedDeltaTime * speed * RecoverSpeed;
        }
        else
        {
            // velocity is zero
            rigidBody.velocity = Vector2.one * 0.01f;
        }
    }

    void Update()
    {
        UpdateEye();
        eye.transform.localScale = new Vector2(1, eyeScale);
    }
}
