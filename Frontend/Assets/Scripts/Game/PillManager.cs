using UnityEngine;

namespace Assets.Scripts.Game
{
    public enum PillEater { Player }

    public static class PillManager
    {
        public static void EatPill(Vector3 pos, PillEater eater)
        {
            Vector3Int tilePos = GameManager.I.PillTilemap.WorldToCell(pos);
            var tile = GameManager.I.PillTilemap.GetTile(tilePos);
            if (tile == null)
                return;

            GameManager.I.PillTilemap.SetTile(tilePos, null);

            Vector3 pillCenter = GameManager.I.PillTilemap.CellToWorld(tilePos);
            pillCenter.x += 0.5f;
            pillCenter.y += 0.5f;

            ParticleEmitter.I.EmitPills(pillCenter, 3);
        }
    }
}
