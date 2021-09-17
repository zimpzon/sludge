using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelCells : MonoBehaviour
{
    public static LevelCells Instance;

    public Tile FloorTile;

    const byte Free = 0;
    const byte StaticWall = 1;
    const byte DynamicWall = 2;
    const byte Taken = 50;

    Tilemap currentTilemap;
    static byte[] cells = new byte[0];
    int bottomOffset;
    int leftOffset;
    int w;
    int h;

    private void Awake()
    {
        Instance = this;
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
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(worldPos, Vector3.one * 0.25f);
                }
            }
        }
    }

    public byte GetCellValue(Vector2 pos)
    {
        int x = (int)(pos.x - leftOffset);
        int y = (int)(pos.y - bottomOffset);
        return cells[y * w + x];
    }

    public void SetDynamicWallRectangle(Vector2 center, float rectW, float rectH, bool blocked)
    {
        // World to cell
        int rectLeft = (int)(center.x - rectW / 2) - leftOffset;
        int rectBottom = (int)(center.y - rectH / 2) - bottomOffset;

        int intRectW = (int)rectW;
        int intRectH = (int)rectH;

        byte value = blocked ? DynamicWall : Free;
        for (int y = rectBottom; y < rectBottom + intRectH ; ++y)
        {
            for (int x = rectLeft; x < rectLeft + intRectW; ++x)
            {
                int cellIdx = (y * w) + x;
                cells[cellIdx] = value;
            }
        }
    }

    public void ResetToTilemap(Tilemap tilemap)
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
                cells[cellIdx] = tile == FloorTile ? Free : StaticWall;
            }
        }
    }
}
