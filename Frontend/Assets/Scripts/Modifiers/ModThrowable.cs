using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModThrowable : SludgeModifier
{
    const float CarryDistance = 0.5f;
    const float ExplosionRadius = 2.0f;
    const float ResetDelay = 1.0f;
    static Collider2D[] scanResults = new Collider2D[20];

    Transform trans;
    bool ownedByPlayer;
    bool wasThrown;
    bool isExploding;
    Vector3 homePos;
    double throwPosX;
    double throwPosY;
    double throwDirX;
    double throwDirY;
    double throwSpeed;
    QuadDistort ripples;
    GameObject body;
    double resetTime;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
    }

    public override void OnLoaded()
    {
        trans = transform;
        homePos = trans.position;
        ripples = GetComponentInChildren<QuadDistort>();
        body = trans.Find("Body").gameObject;
    }

    public override void Reset()
    {
        ripples.Reset();
        ownedByPlayer = false;
        wasThrown = false;
        isExploding = false;
        trans.position = homePos;
        body.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.StaticLevel || entity == EntityType.FakeWall || entity == EntityType.Enemy)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.StaticLevel || entity == EntityType.FakeWall || entity == EntityType.Enemy)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isExploding)
            return;

        if (!wasThrown) // Exploding while held by player
            GameManager.I.Player.ThrowablePickedUp(null);

        SoundManager.Play(FxList.Instance.ThrownBombExplode);
        GameManager.I.CameraRoot.DOShakePosition(0.1f, 0.2f);

        body.SetActive(false);
        isExploding = true;
        ripples.DoRipples();

        int hitCount = Physics2D.OverlapCircleNonAlloc(trans.position, ExplosionRadius, scanResults, SludgeUtil.ThrowableExplosionLayerMask);

        // Some objects die immediately
        for (int i = 0; i < hitCount; ++i)
        {
            if (scanResults[i].gameObject == this.gameObject)
                continue;

            var entity = SludgeUtil.GetEntityType(scanResults[i].gameObject);
            if (entity == EntityType.Throwable)
            {
                var throwable = scanResults[i].gameObject.GetComponent<ModThrowable>();
                throwable.Die();
            }
            else if (entity == EntityType.Enemy)
            {
                GameManager.I.KillEnemy(scanResults[i].gameObject);
            }
        }

        resetTime = GameManager.I.EngineTime + ResetDelay;
    }

    public void Throw(Vector2 direction, double speed)
    {
        wasThrown = true;
        direction.Normalize();
        throwPosX = SludgeUtil.Stabilize(trans.position.x);
        throwPosY = SludgeUtil.Stabilize(trans.position.y);
        throwDirX = SludgeUtil.Stabilize(direction.x);
        throwDirY = SludgeUtil.Stabilize(direction.y);
        throwSpeed = SludgeUtil.Stabilize(speed);
    }

    public override void EngineTick()
    {
        if (isExploding)
        {
            if (GameManager.I.EngineTime > resetTime)
            {
                GameManager.I.DustParticles.transform.position = homePos;
                GameManager.I.DustParticles.Emit(2);
                Reset();
            }
        }

        const float ActivationPlayerDistance = 0.7f * 0.7f;
        const float SqrActivationPlayerDistance = ActivationPlayerDistance * ActivationPlayerDistance;

        var playerDir = Player.Position - trans.position;
        if (!ownedByPlayer && GameManager.I.Player.currentThrowable == null)
        {
            if (playerDir.sqrMagnitude <= SqrActivationPlayerDistance)
            {
                SoundManager.Play(FxList.Instance.ThrownBombPickedUp);
                ownedByPlayer = true;
                GameManager.I.Player.ThrowablePickedUp(this);
            }
        }

        if (ownedByPlayer)
        {
            if (wasThrown)
            {
                // Thrown
                throwPosX = SludgeUtil.Stabilize(throwPosX + GameManager.TickSize * throwSpeed * throwDirX);
                throwPosY = SludgeUtil.Stabilize(throwPosY + GameManager.TickSize * throwSpeed * throwDirY);
                trans.position = new Vector2((float)throwPosX, (float)throwPosY);
            }
            else
            {
                // Being carried
                var newPos = Player.Position + (Player.Rotation * (Vector3.up * CarryDistance));
                trans.position = new Vector2((float)SludgeUtil.Stabilize(newPos.x), (float)SludgeUtil.Stabilize(newPos.y));
            }
        }
    }
}
