using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Key : SludgeObject
{
    public override EntityType EntityType => EntityType.Key;

    public override void Reset()
    {
        gameObject.SetActive(true);
        base.Reset();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
        {
            GameManager.Instance.KeyPickup(this);
            gameObject.SetActive(false);
        }
    }
}
