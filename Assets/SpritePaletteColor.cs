using SludgeColors;
using UnityEngine;

public class SpritePaletteColor : MonoBehaviour
{
    public SchemeColor SchemeColor;

    void Start()
    {
        var renderer = GetComponent<SpriteRenderer>();
        var schemeColor = ColorScheme.GetColor(LevelManager.Instance.ColorScheme, SchemeColor);
        schemeColor.a = renderer.color.a;
        renderer.color = schemeColor;
    }
}
