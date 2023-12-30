using Assets.Scripts.Game;
using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class BallCollector : SludgeObject
{
    public override EntityType EntityType => EntityType.BallCollector;

    public float speed = 14.0f;
    public float ScaleAtFullSize = 1f;

    public Transform displayBody;
    Transform eye;
    Transform pupil;
    float eyeScale;
    float eyeScaleTarget;
    Transform trans;
    System.Random rnd;
    Vector2 basePos;
    Vector2 baseScale;
    int frameLastWallHit;
    int squashCounter;

    Rigidbody2D rigidBody;
    bool isHeld;

    public override void Reset()
    {
        trans.position = basePos;
        trans.localScale = baseScale;
        rigidBody.velocity = Vector3.one * speed;
        frameLastWallHit = 0;
        squashCounter = 0;

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
            rigidBody.velocity = Vector3.zero;
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        ContactPoint2D contactPoint = collision.contacts[0];
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        bool wallHit = entity == EntityType.FakeWall || entity == EntityType.StaticLevel;
        if (wallHit)
        {
            frameLastWallHit = GameManager.I.FrameCounter;
            if (squashCounter++ > 2)
            {
                Kill();
                return;
            }
        }

        if (entity != EntityType.Pickup)
        {
            return;
        }

        PillManager.EatPill(contactPoint.point, PillEater.Ball);
    }

    void Kill()
    {
        SoundManager.Play(FxList.Instance.BallCollectorDie);
        GameManager.I.DeathParticles.transform.position = trans.position;
        GameManager.I.DeathParticles.Emit(3);

        gameObject.SetActive(false);
    }

    void UpdateEye()
    {
        var playerDir = Player.Position - trans.position;
        float sqrPlayerDist = playerDir.sqrMagnitude;
        playerDir.Normalize();

        const float SqrLookRange = 999 * 999;
        const float MaxScale = 0.9f;

        if (GameManager.I.FrameCounter != 0) // Hacky hacky: EngineTick gets called once before starting round. Don't begin to open eyes even if player is in range.
        {
            bool playerIsClose = sqrPlayerDist < SqrLookRange;
            bool hasOpenEye = !isHeld && playerIsClose;
            eyeScaleTarget = hasOpenEye ? MaxScale : 0;
        }

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

        const float RecoverSpeed = 0.75f;
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
        // reset squash counter if not hitting wall for a few frames
        if (GameManager.I.FrameCounter > frameLastWallHit + 1)
            squashCounter = 0;

        PillManager.EatPill(trans.position, PillEater.Ball);

        UpdateEye();
        eye.transform.localScale = new Vector2(1, eyeScale);
    }
}
