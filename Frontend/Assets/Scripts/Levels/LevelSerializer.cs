using Sludge.Modifiers;
using Sludge.Shared;
using Sludge.SludgeObjects;
using Sludge.Tiles;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class LevelSerializer
{
    public static LevelData Run(LevelElements elements, LevelSettings levelSettings)
    {
        var data = new LevelData();

#if UNITY_EDITOR
        data.Name = levelSettings.LevelName;
        data.Difficulty = levelSettings.Difficulty;
        data.TimeSeconds = levelSettings.StartTimeSeconds;
        data.EliteCompletionTimeSeconds = levelSettings.EliteCompletionTimeSeconds;
        data.SortKey = levelSettings.SortKey;

        if (levelSettings.ColorScheme != null)
        {
            // Just save name of colorscheme.
            data.ColorSchemeName = levelSettings.ColorScheme.name;
        }
        else
        {
            // No colorscheme object set in LevelSettings. Maybe level was loaded in editor and saved again - without running it (scheme is located and set in GameManager LoadLevel()).
            // Just save the name that was loaded.
            data.ColorSchemeName = levelSettings.ColorSchemeName;
        }

        if (string.IsNullOrWhiteSpace(levelSettings.UniqueId))
            levelSettings.UniqueId = Guid.NewGuid().ToString().Substring(0, 8);

        data.UniqueId = levelSettings.UniqueId;
        // Player
        data.PlayerTransform = LevelDataTransform.Get(elements.Player.transform);

        // Tilemap
        data.WallTilemap = SerializeTilemap(elements.WallTilemap, elements.TileList);
        data.PillTilemap = SerializeTilemap(elements.PillTilemap, elements.TileList);

        // Objects
        var objects = elements.ObjectsRoot.GetComponentsInChildren<SludgeObject>();
        for (int i = 0; i < objects.Length; ++i)
        {
            var obj = objects[i];
            var modifiers = obj.GetComponentsInChildren<SludgeModifier>();
            var storedObject = new LevelDataObject
            {
                ObjectIdx = elements.ObjectPrefabList.GetObjectIndex(obj.gameObject),
                Transform = LevelDataTransform.Get(obj.transform),
                Modifiers = modifiers.Select(m => JsonUtility.ToJson(m)).ToList(),
            };
            data.Objects.Add(storedObject);
        }
#endif
        return data;
    }

    static LevelTilemapData SerializeTilemap(Tilemap map, TileListScriptableObject tileList)
    {
        var bounds = map.cellBounds;

        var result = new LevelTilemapData
        {
            TilesX = bounds.position.x,
            TilesY = bounds.position.y,
            TilesW = bounds.size.x,
            TilesH = bounds.size.y,
        };

        TileBase[] allTiles = map.GetTilesBlock(bounds);

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                int tileIdx = tileList.GetTileIndex(tile);

                var tilePos = new Vector3Int();
                tilePos.x = x + result.TilesX;
                tilePos.y = y + result.TilesY;
                int tileRotation = (int)map.GetTransformMatrix(tilePos).rotation.eulerAngles.z;

                // Include rotation information in stored tileIdx
                result.TileIndices.Add(tileIdx + tileRotation * 1000);
            }
        }

        return result;
    }
}
