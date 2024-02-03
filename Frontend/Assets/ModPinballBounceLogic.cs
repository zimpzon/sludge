using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModPinballBounceLogic : SludgeModifier
{
    Transform trans;
    Transform bodyTrans;
    Vector3 baseScale;

    public override void OnLoaded()
    {
        trans = transform;
        baseScale = trans.localScale;
        bodyTrans = SludgeUtil.FindByName(trans, "Body").transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player || entity == EntityType.Friend)
        {
            collision.rigidbody.AddForce(-collision.contacts[0].normal * 2000f, ForceMode2D.Force);
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
