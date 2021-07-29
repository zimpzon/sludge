using Sludge.Colors;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SchemeColorApplier : MonoBehaviour
{
    public SchemeColor SchemeColor;
    public float BrightnessOffset = 0.0f;

    void Start()
    {
        ApplyColor(GameManager.Instance?.CurrentColorScheme);
    }

    void OnValidate()
    {
        ApplyColor(GameManager.Instance?.CurrentColorScheme);
    }

    Color GetColor(Color baseColor, ColorSchemeScriptableObject scheme)
    {
        var schemeColor = ColorScheme.GetColor(scheme, SchemeColor);
        schemeColor.a = baseColor.a;

        if (BrightnessOffset != 0.0f)
        {
            float offset = BrightnessOffset;
            Color.RGBToHSV(schemeColor, out float h, out float s, out float v);

            // Adjust brightness up or down according to existing value.
            if (v < 0.5f)
                offset *= -1;

            schemeColor = Color.HSVToRGB(h, s, v + offset);
        }
        return schemeColor;
    }

    public void ApplyColor(ColorSchemeScriptableObject scheme)
    {
        if (scheme == null)
            return;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetColor(spriteRenderer.color, scheme);
            return;
        }

        var camera = GetComponent<Camera>();
        if (camera != null)
        {
            camera.backgroundColor = GetColor(camera.backgroundColor, scheme);
            return;
        }

        var tilemap = GetComponent<Tilemap>();
        if (tilemap != null)
        {
            tilemap.color = GetColor(tilemap.color, scheme);
            return;
        }

        var image = GetComponent<Image>();
        if (image != null)
        {
            image.color = GetColor(image.color, scheme);
            return;
        }

        var text = GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = GetColor(text.color, scheme);
            return;
        }
    }
}
