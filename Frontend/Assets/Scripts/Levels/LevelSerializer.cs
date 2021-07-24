using Sludge.Shared;
using Sludge.SludgeObjects;
using Sludge.Tiles;
using Sludge.Utility;
using UnityEngine.Tilemaps;

public static class LevelSerializer
{
    public static LevelData Run(LevelElements elements, LevelSettings levelSettings)
    {
        var data = new LevelData();

        data.Id = levelSettings.LevelId;
        data.Name = levelSettings.LevelName;
        data.StartTimeSeconds = levelSettings.StartTimeSeconds;
        data.EliteCompletionTimeSeconds = levelSettings.EliteCompletionTimeSeconds;

        // Player
        data.PlayerX = SludgeUtil.Stabilize(elements.Player.transform.position.x);
        data.PlayerY = SludgeUtil.Stabilize(elements.Player.transform.position.y);
        data.PlayerAngle = SludgeUtil.Stabilize(elements.Player.transform.eulerAngles.z);

        // Tilemap
        Tilemap(data, elements.Tilemap, elements.TileList);

        // Objects
        var objects = elements.ObjectsRoot.GetComponentsInChildren<SludgeObject>();
        // Each object (de)serializes itself. All have transform. Pos, rot, scale(?)
        return data;
    }

    static void Tilemap(LevelData data, Tilemap map, TileListScriptableObject tileList)
    {
        var bounds = map.cellBounds;
        TileBase[] allTiles = map.GetTilesBlock(bounds);

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                int tileIdx = tileList.GetTileIndex(tile);
                data.TileIndices.Add(tileIdx);
            }
        }
    }
}
