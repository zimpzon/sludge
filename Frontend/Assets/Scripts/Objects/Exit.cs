using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Exit : SludgeObject
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = 1 << collision.gameObject.layer == SludgeUtil.PlayerLayerMask;
        if (!isPlayer)
            return;

        GameManager.Instance.LevelCompleted(this);
    }
}
