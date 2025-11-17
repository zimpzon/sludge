using System.Collections.Generic;
namespace Assets.Scripts
{
    public enum PlayerDeathType
    {
        None = 0,
        Bullet,
        Mine,
        Saw,
        Laser,
        Stalker,
        Chaser,
        Tumbler,
        Squished,
        PoisonCloud,
    }

    public static class PlayerDeathTypeExtensions
    {
        public static readonly Dictionary<PlayerDeathType, string> Names = new Dictionary<PlayerDeathType, string>()
        {
            { PlayerDeathType.None, "None" },
            { PlayerDeathType.Mine, "Mine" },
            { PlayerDeathType.Saw, "Saw" },
            { PlayerDeathType.Bullet, "Bullet" },
            { PlayerDeathType.Laser, "Laser" },
            { PlayerDeathType.Stalker, "Stalker" },
            { PlayerDeathType.Chaser, "Chaser" },
            { PlayerDeathType.Tumbler, "Tumbler" },
            { PlayerDeathType.Squished, "Squished" },
            { PlayerDeathType.PoisonCloud, "Poison" },
        };
    }
}