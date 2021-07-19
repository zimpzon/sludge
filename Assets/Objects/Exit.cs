using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Exit : SludgeObject
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == SludgeUtil.PlayerLayerMask)
            LevelManager.Instance.LevelCompleted(this);
    }
}
