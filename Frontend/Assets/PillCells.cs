using UnityEngine;
using UnityEngine.Tilemaps;

public class PillCells : MonoBehaviour
{
    public static PillCells I;

    readonly Vector2Int OutOfBounds = new Vector2Int(int.MaxValue, int.MaxValue);

    const byte Free = 0;
    const byte HasPill = 1;

    Tilemap currentTilemap;
    static byte[] cells = new byte[0];
    int bottomOffset;
    int leftOffset;
    int w;
    int h;

    private void Awake()
    {
        I = this;
    }

    private void OnDrawGizmos()
    {
        if (currentTilemap == null)
            return;

        for (int y = 0; y < h; ++y)
        {
            for (int x = 0; x < w; ++x)
            {
                int cellIdx = (y * w) + x;
                byte value = cells[cellIdx];
                if (value != Free)
                {
                    Vector2 worldPos = new Vector2(leftOffset + x + 0.5f, bottomOffset + y + 0.5f);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawCube(worldPos, Vector3.one * 0.25f);
                }
            }
        }
    }

    public Vector2 CellToWorld(Vector2Int cellPos)
        => new Vector2(cellPos.x + leftOffset + 0.5f, cellPos.y + bottomOffset + 0.5f);

    public Vector2Int WorldToCell(Vector2 worldPos)
    {
        int x = (int)(worldPos.x - leftOffset);
        int y = (int)(worldPos.y - bottomOffset);

        if (x < 0 || x >= w || y < 0 || y >= h)
            return OutOfBounds;

        return new Vector2Int(x, y);
    }

    public void UpdateFrom(Tilemap tilemap)
    {
        currentTilemap = tilemap;
        bottomOffset = tilemap.cellBounds.yMin;
        leftOffset = tilemap.cellBounds.xMin;
        w = tilemap.cellBounds.size.x;
        h = tilemap.cellBounds.size.y;

        if (cells.Length < w * h)
            cells = new byte[w * h];

        Vector3Int pos = Vector3Int.zero;
        for (int y = 0; y < h; ++y)
        {
            for (int x = 0; x < w; ++x)
            {
                pos.x = leftOffset + x;
                pos.y = bottomOffset + y;

                // Null = no tile
                var tile = tilemap.GetTile(pos);

                int cellIdx = (y * w) + x;
                cells[cellIdx] = tile == null ? Free : HasPill;
            }
        }
    }
}
