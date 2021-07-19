using Sludge.SludgeObjects;
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
        LevelManager.Instance.KeyPickup(this);
        gameObject.SetActive(false);
    }
}
