using Assets.Scripts.Game;
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
    public bool FlipWhenLastPillCollected = false;

    Collider2D doorCollider;
    SpriteRenderer spriteRenderer;
    Material mat;
    bool flipAtLastPillCollectedExecuted;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mat = spriteRenderer.material;
        GameManager.OnColorSchemeChanged += ColorSchemeChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnColorSchemeChanged -= ColorSchemeChanged;
    }

    private void Start()
    {
        Reset();
    }

    //private void OnValidate()
    //{
    //    // Update shader visibility in editor when values change
    //    if (!Application.isPlaying && Active)
    //    {
    //        var sr = GetComponent<SpriteRenderer>();
    //        if (sr != null && sr.sharedMaterial != null)
    //        {
    //            sr.sharedMaterial.SetFloat("_Visibility", StartEnabled ? 0.8f : 0.1f);
    //        }
    //    }
    //}

    void ColorSchemeChanged()
    {
        UpdateColor();
    }

    void UpdateColor()
    {
        spriteRenderer.color = ColorScheme.GetColor(GameManager.I?.CurrentColorScheme, SchemeColor.Walls);
    }

    public override void Reset()
    {
        StopAllCoroutines();

        doorCollider.enabled = StartEnabled;

        if (Active)
            mat.SetFloat("_Visibility", StartEnabled ? 0.8f : 0.1f);
        else
            mat.SetFloat("_Visibility", 1.0f);

        UpdateColor();
        this.gameObject.layer = SludgeUtil.OutlinedLayerNumber;

        flipAtLastPillCollectedExecuted = false;

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

        if (FlipWhenLastPillCollected && !flipAtLastPillCollectedExecuted && PillManager.PillsLeft == 0)
        {
            flipAtLastPillCollectedExecuted = true;

            if (doorCollider.enabled)
            {
                StopAllCoroutines();
                StartCoroutine(DisableMe());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(EnableMe());
            }
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
            if (t >= 0.80f)
                break;

            mat.SetFloat("_Visibility", 0.8f - t);

            yield return null;
        }
        mat.SetFloat("_Visibility", 0.1f);
    }

    IEnumerator EnableMe()
    {
        const float AnimTime = 0.5f;
        double startTime = GameManager.I.EngineTime;
        doorCollider.enabled = true;
        SoundManager.Play(FxList.Instance.FakeWallShowUp);

        LevelCells.Instance.SetDynamicWallRectangle(transform.position, transform.localScale.x, transform.localScale.y, blocked: true);

        while (true)
        {
            float t = (float)(GameManager.I.EngineTime - startTime) / AnimTime;
            if (t >= 0.8f)
                break;

            mat.SetFloat("_Visibility", t);
            yield return null;
        }
        mat.SetFloat("_Visibility", 0.8f);
    }
}
