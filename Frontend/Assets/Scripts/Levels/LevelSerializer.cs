using Assets.Scripts.Levels;
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
        data.LevelName = levelSettings.LevelName;
        data.TargetTime = levelSettings.TargetTime;

#if UNITY_EDITOR
        //if (levelSettings.ColorScheme != null)
        //{
        //    // Just save name of colorscheme.
        //    data.ColorSchemeName = levelSettings.ColorScheme.name;
        //}
        //else
        //{
        //    // No colorscheme object set in LevelSettings. Maybe level was loaded in editor and saved again - without running it (scheme is located and set in GameManager LoadLevel()).
        //    // Just save the name that was loaded.
        //    data.ColorSchemeName = levelSettings.ColorSchemeName;
        //}

        // Player
        data.PlayerTransform = LevelDataTransform.Get(elements.Player.transform);

        // Tilemaps
        data.WallTilemap = SerializeTilemap(elements.WallTilemap, elements.TileList);
        data.PillTilemap = SerializeTilemap(elements.PillTilemap, elements.TileList);
        data.KillerTilemap = SerializeTilemap(elements.KillerTilemap, elements.TileList);
        data.GlassTilemap = SerializeTilemap(elements.GlassTilemap, elements.TileList);

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
                CustomData = obj.GetComponent<ICustomSerialized>()?.SerializeCustomData(),
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

                Matrix4x4 matrix = map.GetTransformMatrix(tilePos);
                int tileRotation = Mathf.RoundToInt(matrix.rotation.eulerAngles.z);

                // Check for flips by examining the scale
                bool flipX = matrix.m00 < 0; // Scale X component
                bool flipY = matrix.m11 < 0; // Scale Y component

                // Encode: rotation (0-360) + flipX*1000 + flipY*2000
                int transformData = tileRotation;
                if (flipX) transformData += 1000;
                if (flipY) transformData += 2000;

                result.TileIndices.Add(tileIdx + transformData * 10000);
            }
        }

        return result;
    }
}
