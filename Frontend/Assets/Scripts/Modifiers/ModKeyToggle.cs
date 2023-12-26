using Sludge.Colors;
using Sludge.Modifiers;
using Sludge.Utility;
using System.Collections;
using UnityEngine;

public class ModKeyToggle : SludgeModifier
{
    public bool Active = true;
    public int DisableAtKeyCount = -1;
    public int EnableAtKeyCount = -1;
    public bool StartEnabled = true;

    Collider2D doorCollider;
    SpriteRenderer spriteRenderer;
    Material mat;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mat = spriteRenderer.material;
    }

    public override void Reset()
    {
        StopAllCoroutines();

        doorCollider.enabled = StartEnabled;
        spriteRenderer.enabled = StartEnabled;
        mat.SetFloat("_Visibility", StartEnabled ? 1 : 0);

        spriteRenderer.color = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor.Walls);
        this.gameObject.layer = SludgeUtil.OutlinedLayerNumber;

        if (StartEnabled)
        {
            LevelCells.Instance.SetDynamicWallRectangle(transform.position, transform.localScale.x, transform.localScale.y, blocked: true);
        }
    }

    public override void EngineTick()
    {
        if (!Active)
            return;

        if (doorCollider.enabled && GameManager.I.Keys == DisableAtKeyCount)
        {
            StopAllCoroutines();
            StartCoroutine(DisableMe());
        }

        if (!doorCollider.enabled && GameManager.I.Keys == EnableAtKeyCount)
        {
            StopAllCoroutines();
            StartCoroutine(EnableMe());
        }
    }

    IEnumerator DisableMe()
    {
        const float AnimTime = 0.5f;
        double startTime = GameManager.I.EngineTime;
        doorCollider.enabled = false;
        SoundManager.Play(FxList.Instance.FakeWallDisappear);

        LevelCells.Instance.SetDynamicWallRectangle(transform.position, transform.localScale.x, transform.localScale.y, blocked: false);

        while (true)
        {
            float t = (float)(GameManager.I.EngineTime - startTime) / AnimTime;
            mat.SetFloat("_Visibility", 1 - t);

            if (t >= 1.0f)
                break;

            yield return null;
        }

        spriteRenderer.enabled = false;
    }

    IEnumerator EnableMe()
    {
        const float AnimTime = 0.5f;
        double startTime = GameManager.I.EngineTime;
        spriteRenderer.enabled = true;
        doorCollider.enabled = true;
        SoundManager.Play(FxList.Instance.FakeWallShowUp);

        LevelCells.Instance.SetDynamicWallRectangle(transform.position, transform.localScale.x, transform.localScale.y, blocked: true);

        while (true)
        {
            float t = (float)(GameManager.I.EngineTime - startTime) / AnimTime;
            mat.SetFloat("_Visibility", t);

            if (t >= 1.0f)
                break;

            yield return null;
        }
    }
}
