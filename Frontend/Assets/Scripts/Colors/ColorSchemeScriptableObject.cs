using UnityEngine;

namespace Sludge.Colors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorScheme", order = 1)]
    public class ColorSchemeScriptableObject : ScriptableObject
    {
        public string schemeName;
        public Color[] Palette;
        public Color Background;
        public Color Walls;
        public Color Edges;
        public Color Player;
        public Color Exit1;
        public Color Exit2;
        public Color Key1;
        public Color Key2;
        public Color TimePill1;
        public Color TimePill2;
        public Color Conveyor1;
        public Color Conveyor2;
        public Color LaserColor;
        public Color BulletFlash1;
        public Color BulletFlash2;
        public Color Enemy1;

        public Color UiTimerBarFront;
        public Color UiTimerBarBack;
        public Color UiTextDefault;
        public Color UiTextHighlighted;
        public Color UiTextDimmed;
        public Color UiLevelLocked;
        public Color UiLevelUnlocked;
        public Color UiLevelCompleted;
        public Color UiLevelMastered;
        public Color UiSelectionMarker;
        public Color UiButtonFace;
        public Color UiButtonOutline;
        public Color UiBackground;

        public void OnValidate()
        {
            ColorScheme.ApplyColors(this);
            ColorScheme.ApplyUiColors(this);
        }
    }

    public enum SchemeColor
    {
        // ---> NB: New members must be added at bottom since enum as serialized as strings. Adding in the middle will skew all values coming after
        Background,
        Walls,
        Edges,
        Player,
        Exit1,
        Exit2,
        Key1,
        Key2,
        TimePill1,
        TimePill2,
        Conveyor1,
        Conveyor2,
        UiTimerBarFront,
        UiTimerBarBack,
        UiTextDefault,
        UiTextHighlighted,
        UiTextDimmed,
        UiLevelLocked,
        UiLevelCompleted,
        UiLevelMastered,
        UiSelectionMarker,
        UiButtonFace,
        UiButtonOutline,
        UiBackground,
        LaserColor,
        BulletFlash1,
        BulletFlash2,
        UiLevelUnlocked,
        Enemy1,
        // <--- NB: New members must be added at bottom since enum as serialized as strings. Adding in the middle will skew all values coming after.
    }

    public static class ColorScheme
    {
        public static void ApplyColors(ColorSchemeScriptableObject scheme)
        {
            var allColorAppliers = GameObject.FindObjectsOfType<SchemeColorApplier>(includeInactive: true);
            foreach (var applier in allColorAppliers)
                applier.ApplyColor(scheme);

            Shader.SetGlobalColor("_EdgeColor", scheme.Edges);
            Shader.SetGlobalColor("_WallColor", scheme.Walls);
        }

        public static void ApplyUiColors(ColorSchemeScriptableObject scheme)
        {
            var allUiColorAppliers = GameObject.FindObjectsOfType<UiSchemeColorApplier>(includeInactive: true);
            foreach (var applier in allUiColorAppliers)
                applier.ApplyColor(scheme);
        }

        public static Color GetColor(ColorSchemeScriptableObject scheme, SchemeColor name)
        {
            var color = name switch
            {
                SchemeColor.Background => scheme.Background,
                SchemeColor.Walls => scheme.Walls,
                SchemeColor.Edges => scheme.Edges,
                SchemeColor.Player => scheme.Player,
                SchemeColor.Exit1 => scheme.Exit1,
                SchemeColor.Exit2 => scheme.Exit2,
                SchemeColor.Key1 => scheme.Key1,
                SchemeColor.Key2 => scheme.Key2,
                SchemeColor.TimePill1 => scheme.TimePill1,
                SchemeColor.TimePill2 => scheme.TimePill2,
                SchemeColor.Conveyor1 => scheme.Conveyor2,
                SchemeColor.LaserColor => scheme.LaserColor,
                SchemeColor.BulletFlash1 => scheme.BulletFlash1,
                SchemeColor.BulletFlash2 => scheme.BulletFlash2,
                SchemeColor.Enemy1 => scheme.Enemy1,

                SchemeColor.UiTimerBarFront => scheme.UiTimerBarFront,
                SchemeColor.UiTimerBarBack => scheme.UiTimerBarBack,
                SchemeColor.UiTextDefault => scheme.UiTextDefault,
                SchemeColor.UiTextHighlighted => scheme.UiTextHighlighted,
                SchemeColor.UiTextDimmed => scheme.UiTextDimmed,
                SchemeColor.UiLevelLocked => scheme.UiLevelLocked,
                SchemeColor.UiLevelUnlocked => scheme.UiLevelUnlocked,
                SchemeColor.UiLevelCompleted => scheme.UiLevelCompleted,
                SchemeColor.UiLevelMastered => scheme.UiLevelMastered,
                SchemeColor.UiSelectionMarker => scheme.UiSelectionMarker,
                SchemeColor.UiButtonFace => scheme.UiButtonFace,
                SchemeColor.UiButtonOutline => scheme.UiButtonOutline,
                SchemeColor.UiBackground => scheme.UiBackground,
                _ => Color.red,
            };

            color.a = 1;
            return color;
        }
    }
}