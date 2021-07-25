using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Key : SludgeObject
{
    public override void Reset()
    {
        gameObject.SetActive(true);
        base.Reset();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == SludgeUtil.PlayerLayerMask)
        {
            GameManager.Instance.KeyPickup(this);
            gameObject.SetActive(false);
        }
    }
}
