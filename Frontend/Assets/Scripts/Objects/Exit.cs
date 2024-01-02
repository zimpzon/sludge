using Sludge.Modifiers;
using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class Exit : SludgeObject
{
    public override EntityType EntityType => EntityType.Exit;

    public ParticleSystem HighlightParticles;
    public ModRotation InnerRotation;

    SpriteRenderer[] childSprites;
    bool isActive;

    private void Awake()
    {
        childSprites = GetComponentsInChildren<SpriteRenderer>();
    }

    public override void Reset()
    {
        base.Reset();
        SetActive(false);
    }

    void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer child in childSprites)
        {
            Color col = child.color;
            col.a = alpha;
            child.color = col;
        }
    }

    void SetActive(bool active)
    {
        SetAlpha(active ? 1.0f : 0.5f);
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

        //GameManager.I.LevelCompleted(this);
    }
}
