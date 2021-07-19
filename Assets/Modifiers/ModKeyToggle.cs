using Sludge.Modifiers;
using System.Collections;
using UnityEngine;

public class ModKeyToggle : SludgeModifier
{
    public int DisableAtKeyCount = 1;
    public int EnableAtKeyCount = 1;
    public bool StartEnabled = true;

    Color baseColor;
    Vector3 baseScale;
    Collider2D doorCollider;
    SpriteRenderer spriteRenderer;
    Transform trans;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trans = transform;

        baseColor = spriteRenderer.color;
        baseScale = trans.localScale;
    }

    public override void Reset()
    {
        StopAllCoroutines();

        doorCollider.enabled = StartEnabled;
        spriteRenderer.enabled = StartEnabled;
        spriteRenderer.color = baseColor;
        trans.localScale = baseScale;
    }

    public override void EngineTick()
    {
        if (doorCollider.enabled && LevelManager.Instance.Keys == DisableAtKeyCount)
        {
            StopAllCoroutines();
            StartCoroutine(DisableMe());
        }

        if (!doorCollider.enabled && LevelManager.Instance.Keys == EnableAtKeyCount)
        {
            StopAllCoroutines();
            StartCoroutine(EnableMe());
        }
    }

    IEnumerator DisableMe()
    {
        const float AnimTime = 0.25f;
        double startTime = LevelManager.Instance.EngineTime;
        Color color = baseColor;
        Vector3 scale = baseScale;

        doorCollider.enabled = false;

        while (true)
        {
            float t = (float)(LevelManager.Instance.EngineTime - startTime) / AnimTime;
            color.a = 1.0f - t;
            spriteRenderer.color = color;
            trans.localScale = scale;

            if (t >= 1.0f)
                break;

            yield return null;
        }

        spriteRenderer.enabled = false;
    }

    IEnumerator EnableMe()
    {
        const float AnimTime = 0.25f;
        double startTime = LevelManager.Instance.EngineTime;
        Color color = baseColor;
        Vector3 scale = baseScale;

        spriteRenderer.enabled = true;
        doorCollider.enabled = true;

        while (true)
        {
            float t = (float)(LevelManager.Instance.EngineTime - startTime) / AnimTime;
            color.a = t;
            spriteRenderer.color = color;
            trans.localScale = scale;

            if (t >= 1.0f)
                break;

            yield return null;
        }
    }
}
