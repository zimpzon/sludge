using UnityEngine;

namespace Sludge.Colors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorSchemeScriptableObject", order = 1)]
    public class ColorSchemeScriptableObject : ScriptableObject
    {
        public string schemeName;

        public Color Background;
        public Color Walls;
        public Color Text;
        public Color Edges;
        public Color Player;
        public Color Laser;
        public Color Exit1;
        public Color Exit2;
        public Color EnemyA1;
        public Color EnemyA2;
        public Color EnemyB1;
        public Color EnemyB2;
        public Color Mine1;
        public Color Mine2;
        public Color Buff1;
        public Color Buff2;
        public Color Key1;
        public Color Key2;
        public Color Time1;
        public Color Time2;
        public Color GhostTrigger1;
        public Color GhostTrigger2;
    }

    public enum SchemeColor
    {
        Background,
        Walls,
        Text,
        Edges,
        Player,
        Laser,
        Exit1,
        Exit2,
        EnemyA1,
        EnemyA2,
        EnemyB1,
        EnemyB2,
        Mine1,
        Mine2,
        Buff1,
        Buff2,
        Key1,
        Key2,
        Time1,
        Time2,
        GhostTrigger1,
        GhostTrigger2,
    }

    public static class ColorScheme
    {
        public static Color GetColor(ColorSchemeScriptableObject scheme, SchemeColor name)
        {
            return name switch
            {
                SchemeColor.Background => scheme.Background,
                SchemeColor.Walls => scheme.Walls,
                SchemeColor.Text => scheme.Text,
                SchemeColor.Edges => scheme.Edges,
                SchemeColor.Player => scheme.Player,
                SchemeColor.Laser => scheme.Laser,
                SchemeColor.Exit1 => scheme.Exit1,
                SchemeColor.Exit2 => scheme.Exit2,
                SchemeColor.EnemyA1 => scheme.EnemyA1,
                SchemeColor.EnemyA2 => scheme.EnemyA2,
                SchemeColor.EnemyB1 => scheme.EnemyB1,
                SchemeColor.EnemyB2 => scheme.EnemyB2,
                SchemeColor.Mine1 => scheme.Mine1,
                SchemeColor.Mine2 => scheme.Mine2,
                SchemeColor.Buff1 => scheme.Buff1,
                SchemeColor.Buff2 => scheme.Buff2,
                SchemeColor.Key1 => scheme.Key1,
                SchemeColor.Key2 => scheme.Key2,
                SchemeColor.Time1 => scheme.Time1,
                SchemeColor.Time2 => scheme.Time2,
                SchemeColor.GhostTrigger1 => scheme.GhostTrigger1,
                SchemeColor.GhostTrigger2 => scheme.GhostTrigger2,
                _ => Color.black,
            };
        }
    }
}