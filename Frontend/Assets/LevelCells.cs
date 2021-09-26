using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelCells : MonoBehaviour
{
    public static LevelCells Instance;

    public Tile FloorTile;

    readonly Vector2Int OutOfBounds = new Vector2Int(int.MaxValue, int.MaxValue);

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

    public bool TryClaimCell(Vector2Int cell)
    {
        if (GetCellValue(cell) != Free)
            return false;

        SetCellValue(cell, Taken);
        return true;
    }

    public Vector2Int ClaimCell(Vector2 worldPos)
    {
        var cellPos = WorldToCell(worldPos);
        SetCellValue(cellPos, Taken);
        return cellPos;
    }

    public void ReleaseCell(Vector2Int cellPos)
    {
        if (GetCellValue(cellPos) == Taken)
            SetCellValue(cellPos, Free);
    }

    public bool TryClaimMovement(Vector2Int currentCellPos, Vector2Int dir, bool prioritizeX, out Vector2Int newPos)
    {
        var backup = dir;
        newPos = currentCellPos + dir;

        // Try both x and y
        bool canMove = GetCellValue(newPos) == Free;
        bool isDiagonal = dir.x != 0 && dir.y != 0;
        if (canMove && isDiagonal)
        {
            // When moving in a diagonal make sure left and right cells are not blocked.
            var justY = new Vector2Int(0, dir.y);
            var justX = new Vector2Int(dir.x, 0);
            canMove = GetCellValue(currentCellPos + justY) == Free && GetCellValue(currentCellPos + justX) == Free;
        }

        if (prioritizeX)
        {
            if (!canMove)
            {
                // Try just x
                dir = backup;
                dir.y = 0;
                newPos = currentCellPos + dir;
                canMove = GetCellValue(newPos) == Free;
            }

            if (!canMove)
            {
                // Try just y
                dir.x = 0;
                newPos = currentCellPos + dir;
                canMove = GetCellValue(newPos) == Free;
            }
        }
        else
        {
            if (!canMove)
            {
                // Try just y
                dir.x = 0;
                newPos = currentCellPos + dir;
                canMove = GetCellValue(newPos) == Free;
            }

            if (!canMove)
            {
                // Try just x
                dir = backup;
                dir.y = 0;
                newPos = currentCellPos + dir;
                canMove = GetCellValue(newPos) == Free;
            }
        }

        if (canMove)
        {
            SetCellValue(currentCellPos, Free);
            SetCellValue(newPos, Taken);
        }
        return canMove;
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
                if (cellIdx < 0 || cellIdx > cells.Length)
                    continue;

                cells[cellIdx] = value;
            }
        }
    }

    void SetCellValue(Vector2Int cellPos, byte value)
    {
        int idx = cellPos.y * w + cellPos.x;
        if (idx < 0 || idx >= cells.Length)
            return;

        cells[cellPos.y * w + cellPos.x] = value;
    }

    byte GetCellValue(Vector2Int cellPos)
    {
        int idx = cellPos.y * w + cellPos.x;
        if (idx < 0 || idx >= cells.Length)
            return StaticWall;

        return cells[idx];
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
