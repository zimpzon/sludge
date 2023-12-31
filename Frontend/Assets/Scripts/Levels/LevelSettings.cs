using Sludge.Colors;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    public string LevelName;
    public ColorSchemeScriptableObject ColorScheme; // Change this then save level. Note that only the name is saved, not this object.
    public string ColorSchemeName;

    private void OnValidate()
    {
        GameManager.ApplyColorScheme(ColorScheme);
        ColorSchemeName = ColorScheme.name;
    }
}
