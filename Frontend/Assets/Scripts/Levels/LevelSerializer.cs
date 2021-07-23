using Sludge.Shared;
using Sludge.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class LevelSerializer
{
    public static LevelData Run(LevelElements elements, LevelSettings levelSettings)
    {
        var data = new LevelData();

        data.Id = levelSettings.LevelId;
        data.Name = levelSettings.LevelName;
        data.StartTimeSeconds = levelSettings.StartTimeSeconds;
        data.StartTimeSeconds = levelSettings.EliteCompletionTimeSeconds;

        // Player
        data.PlayerX = SludgeUtil.Stabilize(elements.Player.transform.position.x);
        data.PlayerY = SludgeUtil.Stabilize(elements.Player.transform.position.y);
        data.PlayerAngle = SludgeUtil.Stabilize(elements.Player.transform.eulerAngles.z);

        // Tilemap
        Tilemap(data, elements.Tilemap);

        // Objects
        return data;
    }

    static void Tilemap(LevelData data, Tilemap map)
    {
        var bounds = map.cellBounds;
        TileBase[] allTiles = map.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
                }
                else
                {
                    Debug.Log("x:" + x + " y:" + y + " tile: (null)");
                }
            }
        }
    }
}
