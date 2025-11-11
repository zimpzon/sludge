using Sludge.Colors;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    public string LevelName;
    public ColorSchemeScriptableObject ColorScheme; // Change this then save level. Note that only the name is saved, not this object.
    public string ColorSchemeName;
    public float TargetTime = 0.0f;
    private void OnValidate()
    {
        ColorSchemeName = ColorScheme.name;
    }
}
