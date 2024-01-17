using Assets.Scripts.Game;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PillCollectorScript : MonoBehaviour
{
    float radius = 1.2f;

    private void Update()
    {
        Vector3Int centerTile = GameManager.I.PillTilemap.WorldToCell(transform.position);

        int radiusInTiles = Mathf.CeilToInt(radius / GameManager.I.PillTilemap.cellSize.x);
        for (int x = -radiusInTiles; x <= radiusInTiles; x++)
        {
            for (int y = -radiusInTiles; y <= radiusInTiles; y++)
            {
                Vector3Int tilePos = new Vector3Int(centerTile.x + x, centerTile.y + y, centerTile.z);

                if (Vector3.Distance(GameManager.I.PillTilemap.CellToWorld(tilePos), transform.position) <= radius)
                {
                    TileBase tile = GameManager.I.PillTilemap.GetTile<TileBase>(tilePos);
                    if (tile != null)
                    {
                        PillManager.EatPill(tilePos);
                    }
                }
            }
        }
    }
}
