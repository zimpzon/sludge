using UnityEngine;

namespace Assets.Scripts.Game
{
    public enum PillEater { Player, Ball, }

    public static class PillManager
    {
        public static int TotalPills;
        public static int PillsLeft;

        public static void Reset(int totalPills)
        {
            TotalPills = totalPills;
            PillsLeft = TotalPills;
        }

        public static void EatPill(Vector3Int tilePos)
        {
            var tile = GameManager.I.PillTilemap.GetTile(tilePos);
            if (tile == null)
                return;

            GameManager.I.PillTilemap.SetTile(tilePos, null);

            Vector3 pillCenter = GameManager.I.PillTilemap.CellToWorld(tilePos);
            SoundManager.Play(FxList.Instance.TimePillPickup);
            ParticleEmitter.I.EmitPills(pillCenter, 1);

            PillsLeft--;
            GameManager.I.OnPillEaten();
        }
    }
}
