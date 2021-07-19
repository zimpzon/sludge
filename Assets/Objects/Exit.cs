using Sludge.SludgeObjects;
using UnityEngine;

public class Exit : SludgeObject
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        LevelManager.Instance.LevelCompleted(this);
    }
}
