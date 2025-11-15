using UnityEngine;

namespace Sludge.Colors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorSchemeList", order = 1)]
    public class ColorSchemeListScriptableObject : ScriptableObject
    {
        public ColorSchemeScriptableObject[] ColorSchemes;
    }
}