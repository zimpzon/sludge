using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using System.Collections;
using UnityEngine;

public class ModThrowable : SludgeModifier
{
    const float CarryDistance = 0.5f;

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
        if (entity == EntityType.StaticLevel || entity == EntityType.FakeWall)
        {
            StartCoroutine(Die());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.StaticLevel || entity == EntityType.FakeWall)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        // Chain reaction is a must!!

        // Naughty: reuse Player code for explosion effects
        var player = GameManager.Instance.Player;
        player.deathParticles.transform.position = trans.position;
        player.deathParticles.Emit(20);
        player.CameraRoot.DOShakePosition(0.1f, 0.2f);

        body.SetActive(false);
        isExploding = true;
        ripples.DoRipples();
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

        const float ActivationPlayerDistance = 0.8f * 0.8f;
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
