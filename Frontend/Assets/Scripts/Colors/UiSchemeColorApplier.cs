using Sludge.Colors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiSchemeColorApplier : MonoBehaviour
{
    public SchemeColor SchemeColor;
    public float BrightnessOffset = 0.0f;

    ColorSchemeScriptableObject myColorScheme;

    public void SetBrightnessOffset(float brightnessOffset)
    {
        BrightnessOffset = brightnessOffset;
        ApplyColor(myColorScheme);
    }

    Color GetColor(Color baseColor, ColorSchemeScriptableObject scheme)
    {
        var schemeColor = ColorScheme.GetColor(scheme, SchemeColor);
        schemeColor.a = baseColor.a;

        if (BrightnessOffset != 0.0f)
        {
            float offset = BrightnessOffset;
            Color.RGBToHSV(schemeColor, out float h, out float s, out float v);
            schemeColor = Color.HSVToRGB(h, s, v + offset);
        }
        return schemeColor;
    }

    public void ApplyColor(ColorSchemeScriptableObject scheme)
    {
        myColorScheme = scheme;

        if (scheme == null)
            return;

        var image = GetComponent<Image>();
        if (image != null)
        {
            image.color = GetColor(image.color, scheme);
            return;
        }

        var rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            rawImage.color = GetColor(rawImage.color, scheme);
            return;
        }

        var text = GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = GetColor(text.color, scheme);
            text.faceColor = Color.white;
            text.ForceMeshUpdate();
        }
    }
}
