using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using System.Collections;
using UnityEngine;

public class ModThrowable : SludgeModifier
{
    const float CarryDistance = 0.5f;
    const float ExplosionRadius = 2.0f;
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
        StopAllCoroutines();
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

        StartCoroutine(DieCo());
    }

    IEnumerator DieCo()
    {
        if (!wasThrown) // Exploding while held by player
            GameManager.Instance.Player.ThrowablePickedUp(null);

        // Naughty: reuse Player code for explosion effects
        var player = GameManager.Instance.Player;
        player.deathParticles.transform.position = trans.position;
        player.deathParticles.Emit(20);
        player.CameraRoot.DOShakePosition(0.1f, 0.2f);

        body.SetActive(false);
        isExploding = true;
        ripples.DoRipples();

        int hitCount = Physics2D.OverlapCircleNonAlloc(trans.position, ExplosionRadius, scanResults, SludgeUtil.ThrowableExplosionLayerMask);

        // Immediate death
        for (int i = 0; i < hitCount; ++i)
        {
            var entity = SludgeUtil.GetEntityType(scanResults[i].gameObject);
            if (entity == EntityType.Enemy)
                SludgeUtil.SetActiveRecursive(scanResults[i].gameObject, false);
        }

        // Delayed death
        yield return new WaitForSeconds(0.2f);

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
        }

        double endTime = GameManager.Instance.EngineTime + 0.5f;
        while (GameManager.Instance.EngineTime < endTime)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        Reset();
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
            return;

        const float ActivationPlayerDistance = 0.7f * 0.7f;
        const float SqrActivationPlayerDistance = ActivationPlayerDistance * ActivationPlayerDistance;

        var playerDir = Player.Position - trans.position;
        if (!ownedByPlayer && GameManager.Instance.Player.currentThrowable == null)
        {
            if (playerDir.sqrMagnitude <= SqrActivationPlayerDistance)
            {
                ownedByPlayer = true;
                GameManager.Instance.Player.ThrowablePickedUp(this);
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
