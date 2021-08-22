using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class TimePill : SludgeObject
{
    public override EntityType EntityType => EntityType.TimePill;

    Transform trans;

    public override void Reset()
    {
        trans = transform;
        gameObject.SetActive(true);
        base.Reset();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
        {
            GameManager.Instance.DustParticles.transform.position = trans.position;
            GameManager.Instance.DustParticles.Emit(2);
            GameManager.Instance.TimePillPickup(this);
            gameObject.SetActive(false);
        }
    }
}
