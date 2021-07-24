using Sludge.Modifiers;
using Sludge.Shared;
using System.Linq;
using UnityEngine;

public static class LevelDeserializer
{
    public static void Run(LevelData data, LevelElements elements, LevelSettings levelSettings)
    {
        // Level settings
        levelSettings.LevelId = data.Id;
        levelSettings.LevelName = data.Name;
        levelSettings.StartTimeSeconds = data.StartTimeSeconds;
        levelSettings.EliteCompletionTimeSeconds = data.EliteCompletionTimeSeconds;

        // Player
        data.PlayerTransform.Set(elements.Player.transform);

        // Tiles
        elements.Tilemap.ClearAllTiles();
        var tilePos = new Vector3Int();
        var tilemapCollider = elements.Tilemap.gameObject.GetComponent<CompositeCollider2D>();
        tilemapCollider.generationType = CompositeCollider2D.GenerationType.Manual;

        for (int y = 0; y < data.TilesH; ++y)
        {
            for (int x = 0; x < data.TilesW; ++x)
            {
                tilePos.x = x + data.TilesX;
                tilePos.y = y + data.TilesY;
                int tileIdx = data.TileIndices[y * data.TilesW + x];
                var tile = elements.TileList.Tiles[tileIdx];
                elements.Tilemap.SetTile(tilePos, tile);
            }
        }
        tilemapCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;

        // Objects
        for (int i = elements.ObjectsRoot.transform.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(elements.ObjectsRoot.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < data.Objects.Count; ++i)
        {
            var storedObj = data.Objects[i];
            var prefab = elements.ObjectPrefabList.ObjectPrefabs[storedObj.ObjectIdx];
            var instance = GameObject.Instantiate(prefab);
            storedObj.Transform.Set(instance.transform);

            var modifiers = instance.GetComponentsInChildren<SludgeModifier>();
            for (int j = 0; j < modifiers.Count(); ++j)
            {
                JsonUtility.FromJsonOverwrite(storedObj.Modifiers[j], modifiers[j]);
            }

            instance.transform.SetParent(elements.ObjectsRoot.transform);
        }
    }
}
