using Assets.Scripts.Game;
using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModExitLogic : SludgeModifier
{
    SpriteRenderer[] childSprites;
    Transform cow;
    ParticleSystem particles;
    bool isActive;

    private void Awake()
    {
        childSprites = GetComponentsInChildren<SpriteRenderer>();
        particles = SludgeUtil.FindByName(transform, "HighlightParticles").GetComponentInChildren<ParticleSystem>();
        cow = SludgeUtil.FindByName(transform, "Cow");
    }

    public override void Reset()
    {
        SetActive(false);
        cow.transform.DOKill();
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
        cow.transform.DOKill();
        cow.transform.localScale = Vector3.one;

        particles.gameObject.SetActive(active);
        SetAlpha(active ? 1.0f : 0.4f);
        isActive = active;
    }

    public void Activate()
    {
        SetActive(true);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActive)
            return;

        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
        {
            GameManager.I.LevelCompleted();
            ShowCowPopup();
            return;
        }

        if (entity == EntityType.Friend)
        {
            var kid = collision.gameObject.GetComponent<KidLogicMod>();
            ParticleEmitter.I.EmitDust(kid.transform.position, 10);
            ParticleEmitter.I.EmitPills(kid.transform.position, 5);
            SoundManager.Play(FxList.Instance.KeyPickup);

            kid.gameObject.SetActive(false);
        }
    }

    public override void EngineTick()
    {
        if (isActive)
            return;

        bool openExit = PillManager.PillsLeft == 0;
        if (openExit)
        {
            SoundManager.Play(FxList.Instance.ExitOpen);
            SetActive(true);
        }
    }

    void ShowCowPopup()
    {
        cow.transform.DOKill();
        cow.transform.DOScale(Vector3.one * 2.0f, 0.3f);
    }
}
