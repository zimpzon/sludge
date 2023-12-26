using Sludge.Modifiers;
using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Exit : SludgeObject
{
    public override EntityType EntityType => EntityType.Exit;

    public ParticleSystem HighlightParticles;
    public ModRotation InnerRotation;

    bool isActive;

    public override void Reset()
    {
        base.Reset();
        SetActive(false);
    }

    void SetActive(bool active)
    {
        InnerRotation.GetComponent<ModRotation>().Active = active;
        isActive = active;
    }

    public void Activate()
    {
        SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        GameManager.I.LevelCompleted(this);
    }
}
