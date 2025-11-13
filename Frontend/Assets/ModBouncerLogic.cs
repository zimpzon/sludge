using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModBouncerLogic : SludgeModifier
{
    public float speed = 8.0f;
    public float startingAngle = 45f; // Angle in degrees
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
        // Convert angle to radians and create velocity vector
        float angleRad = startingAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        rigidBody.linearVelocity = direction * speed;
        frameLastWallHit = 0;
        squashCounter = 0;
        base.Reset();
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

        if (entity == EntityType.Player)
        {
            Player.I.Kill(Assets.Scripts.PlayerDeathType.Bouncer);
            return;
        }
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
    }

    void Kill()
    {
        SoundManager.Play(FxList.Instance.BallCollectorDie);
        gameObject.SetActive(false);
    }

    void UpdateEye()
    {
        var playerDir = Player.Position - trans.position;
        float sqrPlayerDist = playerDir.sqrMagnitude;
        playerDir.Normalize();
        const float SqrLookRange = 999 * 999;
        const float MaxScale = 0.9f;

        if (GameManager.I.FrameCounter != 0)
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

    public override void EngineTick()
    {
        // Maintain constant speed by normalizing velocity every physics tick
        if (rigidBody.linearVelocity.sqrMagnitude > 0.01f)
        {
            rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * speed;
        }

        // Reset squash counter if not hitting wall for a few frames
        if (GameManager.I.FrameCounter > frameLastWallHit + 1)
            squashCounter = 0;
    }

    void Update()
    {
        UpdateEye();
        eye.transform.localScale = new Vector2(1, eyeScale);
    }
}