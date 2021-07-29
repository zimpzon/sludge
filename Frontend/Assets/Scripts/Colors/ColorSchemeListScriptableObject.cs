using UnityEngine;

namespace Sludge.Colors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorSchemeList", order = 1)]
    public class ColorSchemeListScriptableObject : ScriptableObject
    {
        public ColorSchemeScriptableObject[] ColorSchemes;

        public static int IdxSelected;

        public ColorSchemeScriptableObject GetNext()
        {
            if (++IdxSelected >= ColorSchemes.Length)
                IdxSelected = 0;

            return ColorSchemes[IdxSelected];
        }

        public ColorSchemeScriptableObject GetPrev()
        {
            if (--IdxSelected < 0)
                IdxSelected = ColorSchemes.Length - 1;

            return ColorSchemes[IdxSelected];
        }
    }
}