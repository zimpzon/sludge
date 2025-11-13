using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModPinballBounceLogic : SludgeModifier
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
            var direction = collision.contacts[0].normal;
            Player.I.AddForceDirection(-direction);
            bodyTrans.DOKill(complete: true);
            bodyTrans.DOPunchScale(Vector3.one * 0.2f, 0.2f);
        }
    }

    public override void Reset()
    {
    }

    public override void EngineTick()
    {
    }
}
