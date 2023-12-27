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
        if (entity == EntityType.Player || entity == EntityType.BallCollector)
        {
            GameManager.I.DustParticles.transform.position = trans.position;
            GameManager.I.DustParticles.Emit(2);
            GameManager.I.TimePillPickup(this);
            gameObject.SetActive(false);
        }
    }
}
