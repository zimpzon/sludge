using UnityEngine;

namespace Sludge.Colors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorScheme", order = 1)]
    public class ColorSchemeScriptableObject : ScriptableObject
    {
        public string schemeName;
        public bool IsDefault;

        public Color[] Palette;

        public Color Black;
        public Color Background;
        public Color Walls;
        public Color PlayerTint;
        public Color Exit;
        public Color Key;
        public Color Conveyor;

        public Color PlayerScheme_A1;
        public Color PlayerScheme_A2;
        public Color PlayerScheme_B1;
        public Color PlayerScheme_B2;

        public Color TimePill;
        public Color EnemyScheme_A1;
        public Color EnemyScheme_A2;
        public Color EnemyScheme_B1;
        public Color EnemyScheme_B2;

        public Color UiTextTitle;
        public Color UiTextHighlighted;
        public Color UiTextDefault;

        public Color UiSelectionMarker;
        public Color UiButtonFaceDark;
        public Color UiButtonFaceLight;
        public Color UiMenuBackground;
        public Color UiTitleBackground;
        public Color Mines;

        public void OnValidate()
        {
            ColorScheme.ApplyColors(this);
#if UNITY_EDITOR
            // Force Unity to repaint the editor (Scene/Game view UI)
            UnityEditor.SceneView.RepaintAll();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
        }
    }

    public enum SchemeColor
    {
        // ---> NB: New members must be added at bottom since enum as serialized as strings. Adding in the middle will skew all values coming after
        Black,
        Background,
        Walls,
        PlayerTint,
        Exit,
        Key,
        Conveyor,
        PlayerScheme_A1,
        PlayerScheme_A2,
        PlayerScheme_B1,
        PlayerScheme_B2,
        TimePill,

        EnemyScheme_A1,
        EnemyScheme_A2,
        EnemyScheme_B1,
        EnemyScheme_B2,

        UiTextTitle,
        UiTextHighlighted,
        UiTextDefault,

        UiSelectionMarker,
        UiButtonFaceDark,
        UiButtonFaceLight,
        UiMenuBackground,
        UiTitleBackground,

        Mines,
        // <--- NB: New members must be added at bottom since enum as serialized as strings. Adding in the middle will skew all values coming after.
    }

    public static class ColorScheme
    {
        public static void ApplyColors(ColorSchemeScriptableObject scheme)
        {
            if (scheme == null)
            {
                Debug.LogError("Setting NULL color scheme");
                return;
            }

            if (GameManager.I != null)
            {
                GameManager.I.CurrentColorScheme = scheme;
                Debug.Log($"Applying color scheme (GameManager is initialized): [{scheme.name}]");
            }
            else
            {
                Debug.Log($"Applying color scheme with GameManager not initialized: [{scheme.name}]");
            }

            var allColorAppliers = GameObject.FindObjectsByType<SchemeColorApplier>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var applier in allColorAppliers)
                applier.ApplyColor(scheme);

            Shader.SetGlobalColor("_EdgeColor", scheme.Walls);
            Shader.SetGlobalColor("_WallColor", scheme.Walls);

            ApplyUiColors(scheme);
        }

        private static void ApplyUiColors(ColorSchemeScriptableObject scheme)
        {
            var allUiColorAppliers = GameObject.FindObjectsByType<UiSchemeColorApplier>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var applier in allUiColorAppliers)
                applier.ApplyColor(scheme);
        }

        public static Color GetColor(ColorSchemeScriptableObject scheme, SchemeColor name)
        {
            if (scheme == null)
                return Color.magenta;

            var color = name switch
            {
                SchemeColor.Black => scheme.Black,
                SchemeColor.Background => scheme.Background,
                SchemeColor.Walls => scheme.Walls,
                SchemeColor.PlayerTint => scheme.PlayerTint,
                SchemeColor.Exit => scheme.Exit,
                SchemeColor.Key => scheme.Key,
                SchemeColor.Conveyor => scheme.Conveyor,
                SchemeColor.PlayerScheme_A1 => scheme.PlayerScheme_A1,
                SchemeColor.PlayerScheme_A2 => scheme.PlayerScheme_A2,
                SchemeColor.PlayerScheme_B1 => scheme.PlayerScheme_B1,
                SchemeColor.PlayerScheme_B2 => scheme.PlayerScheme_B2,
                SchemeColor.TimePill => scheme.TimePill,

                SchemeColor.EnemyScheme_A1 => scheme.EnemyScheme_A1,
                SchemeColor.EnemyScheme_A2 => scheme.EnemyScheme_A2,
                SchemeColor.EnemyScheme_B1 => scheme.EnemyScheme_B1,
                SchemeColor.EnemyScheme_B2 => scheme.EnemyScheme_B2,

                SchemeColor.UiTextTitle => scheme.UiTextTitle,
                SchemeColor.UiTextHighlighted => scheme.UiTextHighlighted,
                SchemeColor.UiTextDefault => scheme.UiTextDefault,
                SchemeColor.UiSelectionMarker => scheme.UiSelectionMarker,
                SchemeColor.UiButtonFaceDark => scheme.UiButtonFaceDark,
                SchemeColor.UiButtonFaceLight => scheme.UiButtonFaceLight,
                SchemeColor.UiMenuBackground => scheme.UiMenuBackground,
                SchemeColor.UiTitleBackground => scheme.UiTitleBackground,

                SchemeColor.Mines => scheme.Mines,
                _ => Color.red,
            };

            color.a = 1;
            return color;
        }
    }
}
