using System.Collections.Generic;
namespace Assets.Scripts
{
    public enum PlayerDeathType
    {
        None = 0,
        Squished,
        Bullet,
        Mine,
        Laser,
        Ghost,
        Chaser,
        PoisonCloud,
        Tumbler,
        Follower,
        Bouncer,
        Sticker,
    }

    public static class PlayerDeathTypeExtensions
    {
        public static readonly Dictionary<PlayerDeathType, string> Names = new Dictionary<PlayerDeathType, string>()
        {
            { PlayerDeathType.None, "None" },
            { PlayerDeathType.Squished, "Squished" },
            { PlayerDeathType.Bullet, "Bullet" },
            { PlayerDeathType.Mine, "Mine" },
            { PlayerDeathType.Laser, "Laser" },
            { PlayerDeathType.Ghost, "Ghost" },
            { PlayerDeathType.Chaser, "Chaser" },
            { PlayerDeathType.PoisonCloud, "Poison Cloud" },
            { PlayerDeathType.Tumbler, "Tumbler" },
            { PlayerDeathType.Follower, "Follower" },
            { PlayerDeathType.Bouncer, "Bouncer" },
            { PlayerDeathType.Sticker, "Sticker" },
        };
    }
}