using UnityEngine;
using UnityEngine.Tilemaps;

public class EnergyPillDetectorScript : MonoBehaviour
{
    float radius = 0.90f;

    public TileBase UnarmedTile;
    public TileBase ArmedTile;

    private void Update()
    {
        Vector3Int centerTile = GameManager.I.EnergyTilemap.WorldToCell(transform.position);

        int radiusInTiles = Mathf.CeilToInt(radius / GameManager.I.EnergyTilemap.cellSize.x);
        for (int x = -radiusInTiles; x <= radiusInTiles; x++)
        {
            for (int y = -radiusInTiles; y <= radiusInTiles; y++)
            {
                Vector3Int tilePos = new Vector3Int(centerTile.x + x, centerTile.y + y, centerTile.z);

                if (Vector3.Distance(GameManager.I.EnergyTilemap.CellToWorld(tilePos), transform.position) <= radius)
                {
                    TileBase tile = GameManager.I.EnergyTilemap.GetTile<TileBase>(tilePos);
                    if (tile == null)
                        continue;

                    if (tile == UnarmedTile)
                    {
                        GameManager.I.EnergyTilemap.SetTile(tilePos, ArmedTile);
                        GameManager.I.EnergyTilemap.RefreshTile(tilePos);
                    }
                    else if (tile == ArmedTile)
                    {
                    //    GameManager.I.EnergyTilemap.SetTile(tilePos, UnarmedTile);
                    //    GameManager.I.EnergyTilemap.RefreshTile(tilePos);
                    }
                    else
                    {
                        Debug.Log("Unknown tile: " + tile.name);
                    }
                }
            }
        }
    }
}
