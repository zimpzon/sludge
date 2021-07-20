using SludgeColors;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UiSchemeColorApplier : MonoBehaviour
{
    public SchemeColor SchemeColor;
    public float BrightnessOffset = 0.0f;

    void Start()
    {
        ApplyColor();
    }

    void OnValidate()
    {
        ApplyColor();
    }

    Color GetColor(Color baseColor)
    {
        var schemeColor = ColorScheme.GetColor(GameManager.Instance.UiColorScheme, SchemeColor);
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

    void ApplyColor()
    {
        if (GameManager.Instance?.ColorScheme == null)
            return;

        var image = GetComponent<Image>();
        if (image != null)
        {
            image.color = GetColor(image.color);
            return;
        }

        var rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            rawImage.color = GetColor(rawImage.color);
            return;
        }

        var text = GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = GetColor(text.color);
            return;
        }
    }
}
