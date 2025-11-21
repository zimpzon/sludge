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

    [Header("Crumbling Wall Settings")]
    [Tooltip("If true, wall will shake and crumble before disappearing")]
    public bool IsCrumblingWall = false;
    [Tooltip("Duration of shake effect in seconds")]
    public float ShakeDuration = 1.0f;
    [Tooltip("Intensity of shake effect")]
    public float ShakeStrength = 0.2f;

    Collider2D doorCollider;
    SpriteRenderer spriteRenderer;
    Material mat;
    bool flipAtLastPillCollectedExecuted;
    bool crumbleTriggered = false;
    Vector3 originalPosition;
    bool isShaking = false;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mat = spriteRenderer.material;
        GameManager.OnColorSchemeChanged += ColorSchemeChanged;
        originalPosition = transform.position;
    }

    private void OnDestroy()
    {
        GameManager.OnColorSchemeChanged -= ColorSchemeChanged;
    }

    public override void OnLoaded()
    {
        originalPosition = transform.position;
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
        crumbleTriggered = false;
        isShaking = false;

        // Reset position to original
        if (originalPosition != Vector3.zero)
            transform.position = originalPosition;

        if (StartEnabled)
        {
            LevelCells.Instance.SetDynamicWallRectangle(transform.position, transform.localScale.x, transform.localScale.y, blocked: true);
        }
    }

    public override void EngineTick()
    {
        if (!Active)
            return;

        // Handle custom shake effect
        if (isShaking)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-ShakeStrength, ShakeStrength),
                Random.Range(-ShakeStrength, ShakeStrength),
                0
            );
            transform.position = originalPosition + shakeOffset;
        }

        if (doorCollider.enabled && GameManager.I.Keys == DisableAtKeyCount && !crumbleTriggered)
        {
            crumbleTriggered = true;
            Debug.Log($"Key trigger detected! Keys: {GameManager.I.Keys}, DisableAtKeyCount: {DisableAtKeyCount}, IsCrumblingWall: {IsCrumblingWall}");
            StopAllCoroutines();
            if (IsCrumblingWall)
            {
                Debug.Log("Starting crumble sequence");
                StartCoroutine(CrumbleAndDisable());
            }
            else
            {
                StartCoroutine(DisableMe());
            }
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
                if (IsCrumblingWall)
                {
                    StartCoroutine(CrumbleAndDisable());
                }
                else
                {
                    StartCoroutine(DisableMe());
                }
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

    IEnumerator CrumbleAndDisable()
    {
        Debug.Log("Starting shake effect");
        // Start custom shaking
        isShaking = true;

        // Wait for shake to complete
        yield return new WaitForSeconds(ShakeDuration);

        // Stop shaking and reset position
        isShaking = false;
        transform.position = originalPosition;

        Debug.Log("Starting normal disable");
        // Now disable normally
        yield return StartCoroutine(DisableMe());
    }
}
