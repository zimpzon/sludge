using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Exit : SludgeObject
{
    public override EntityType EntityType => EntityType.Exit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        GameManager.Instance.LevelCompleted(this);
    }
}
