using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModPinballBounceLogicBox : SludgeModifier
{
    Transform trans;
    Transform bodyTrans;

    public override void OnLoaded()
    {
        trans = transform;
        bodyTrans = SludgeUtil.FindByName(trans, "Body").transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
        {
            var normal = collision.contacts[0].normal;
            var direction = Vector2.Dot(normal, Vector2.up) > 0.7f ? Vector2.down : Vector2.up;
            Player.I.AddForceDirection(direction);
            bodyTrans.DOKill(complete: true);
            bodyTrans.DOPunchScale(Vector3.one * 0.2f, 0.2f);
            SoundManager.Play(FxList.Instance.PlayerJumpJumpPad);
        }
    }

    public override void Reset()
    {
    }

    public override void EngineTick()
    {
    }
}
