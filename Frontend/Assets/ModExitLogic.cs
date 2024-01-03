using Assets.Scripts.Game;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModExitLogic : SludgeModifier
{
    SpriteRenderer[] childSprites;
    ParticleSystem particles;
    bool isActive;

    private void Awake()
    {
        childSprites = GetComponentsInChildren<SpriteRenderer>();
        particles = SludgeUtil.FindByName(transform, "HighlightParticles").GetComponentInChildren<ParticleSystem>();
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
        particles.gameObject.SetActive(active);
        SetAlpha(active ? 1.0f : 0.2f);
        isActive = active;
    }

    public void Activate()
    {
        SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
            return;

        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        GameManager.I.LevelCompleted();
    }

    public override void EngineTick()
    {
        if (isActive)
            return;

        bool openExit = PillManager.PillsLeft == 0;
        if (openExit)
        {
            SetActive(true);
        }
    }
}
