using System.Collections;
using UnityEngine;

public class QuadDistort : MonoBehaviour
{
    public float RippleTime = 1.0f;
    public float MaxSize = 20;

    Material mat;
    Renderer _renderer;
    Transform trans;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        mat = _renderer.material;
        trans = transform;

        Reset();
    }

    public void Reset()
    {
        StopAllCoroutines();
        _renderer.enabled = false;
    }

    public void DoRipples()
    {
        StartCoroutine(DoRippleLoop());
    }

    IEnumerator DoRippleLoop()
    {
        _renderer.enabled = true;

        float startTime = Time.time;
        float endTime = startTime + RippleTime;

        while (Time.time < endTime)
        {
            float t = 1.0f - ((endTime - Time.time) / RippleTime);

            float size = t * MaxSize;
            trans.localScale = Vector2.one * size;

            float amount = (1.0f - t) * 0.03f;
            mat.SetFloat("_amount", amount);

            yield return null;
        }
    }
}
