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
    Transform trans;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trans = transform;
    }

    public override void Reset()
    {
        StopAllCoroutines();

        doorCollider.enabled = StartEnabled;
        spriteRenderer.enabled = StartEnabled;
        spriteRenderer.color = ColorScheme.GetColor(GameManager.Instance.CurrentColorScheme, SchemeColor.Walls);
        this.gameObject.layer = SludgeUtil.OutlinedLayerNumber;
    }

    public override void EngineTick()
    {
        if (!Active)
            return;

        if (doorCollider.enabled && GameManager.Instance.Keys == DisableAtKeyCount)
        {
            StopAllCoroutines();
            StartCoroutine(DisableMe());
        }

        if (!doorCollider.enabled && GameManager.Instance.Keys == EnableAtKeyCount)
        {
            StopAllCoroutines();
            StartCoroutine(EnableMe());
        }
    }

    IEnumerator DisableMe()
    {
        const float AnimTime = 0.5f;
        double startTime = GameManager.Instance.EngineTime;
        Color color = spriteRenderer.color = ColorScheme.GetColor(GameManager.Instance.CurrentColorScheme, SchemeColor.Walls);

        doorCollider.enabled = false;
        this.gameObject.layer = SludgeUtil.ObjectsLayerNumber;

        while (true)
        {
            float t = (float)(GameManager.Instance.EngineTime - startTime) / AnimTime;
            color.a = 1.0f - t;
            spriteRenderer.color = color;

            if (t >= 1.0f)
                break;

            yield return null;
        }

        spriteRenderer.enabled = false;
    }

    IEnumerator EnableMe()
    {
        const float AnimTime = 0.5f;
        double startTime = GameManager.Instance.EngineTime;
        Color color = spriteRenderer.color = ColorScheme.GetColor(GameManager.Instance.CurrentColorScheme, SchemeColor.Walls);

        spriteRenderer.enabled = true;
        doorCollider.enabled = true;

        while (true)
        {
            float t = (float)(GameManager.Instance.EngineTime - startTime) / AnimTime;
            color.a = t;
            spriteRenderer.color = color;

            if (t >= 1.0f)
                break;

            yield return null;
        }
    }
}
