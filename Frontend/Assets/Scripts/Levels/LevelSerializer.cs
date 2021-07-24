using Sludge.Modifiers;
using Sludge.Shared;
using Sludge.SludgeObjects;
using Sludge.Tiles;
using System.Linq;
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
        data.EliteCompletionTimeSeconds = levelSettings.EliteCompletionTimeSeconds;

        // Player
        data.PlayerTransform = LevelDataTransform.Get(elements.Player.transform);
            
        // Tilemap
        Tilemap(data, elements.Tilemap, elements.TileList);

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

        return data;
    }

    static void Tilemap(LevelData data, Tilemap map, TileListScriptableObject tileList)
    {
        var bounds = map.cellBounds;
        data.TilesX = bounds.position.x;
        data.TilesY = bounds.position.y;
        data.TilesW = bounds.size.x;
        data.TilesH = bounds.size.y;

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
