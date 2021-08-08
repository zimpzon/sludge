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
            float tInv = ((endTime - Time.time) / RippleTime);

            float size = (1 - (tInv * tInv)) * MaxSize;
            trans.localScale = Vector2.one * size;

            float rippleAmount = tInv * tInv;
            mat.SetFloat("_amount", rippleAmount);

            yield return null;
        }
    }
}
