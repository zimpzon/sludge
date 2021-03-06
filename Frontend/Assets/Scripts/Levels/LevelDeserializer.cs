using Sludge.Modifiers;
using Sludge.Shared;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class LevelDeserializer
{
    public static void Run(LevelData data, LevelElements elements, LevelSettings levelSettings)
    {
        // Level settings
        levelSettings.LevelName = data.Name;
        levelSettings.Difficulty = data.Difficulty;
        levelSettings.StartTimeSeconds = data.TimeSeconds;
        levelSettings.EliteCompletionTimeSeconds = data.EliteCompletionTimeSeconds;
        levelSettings.UniqueId = data.UniqueId;
        levelSettings.SortKey = data.SortKey;
        levelSettings.ColorSchemeName = data.ColorSchemeName;

        // Clear existing bullets
        if (BulletManager.Instance != null)
            BulletManager.Instance.Reset();

        for (int i = elements.ObjectsRoot.transform.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(elements.ObjectsRoot.transform.GetChild(i).gameObject);
        }

        // Clear existing tiles
        var tilemapCollider = elements.Tilemap.gameObject.GetComponent<CompositeCollider2D>();
        tilemapCollider.generationType = CompositeCollider2D.GenerationType.Manual;
        elements.Tilemap.ClearAllTiles();
        elements.Tilemap.CompressBounds();
        tilemapCollider.GenerateGeometry();

        // Place player
        data.PlayerTransform.Set(elements.Player.transform);

        // Place new tiles
        var tilePos = new Vector3Int();

        for (int y = 0; y < data.TilesH; ++y)
        {
            for (int x = 0; x < data.TilesW; ++x)
            {
                tilePos.x = x + data.TilesX;
                tilePos.y = y + data.TilesY;
                int tileIdx = data.TileIndices[y * data.TilesW + x];
                // Tile rotation is stored as rot * 1000
                int tileRotation = tileIdx / 1000;
                tileIdx %= 1000;
                var tile = elements.TileList.Tiles[tileIdx];
                elements.Tilemap.SetTile(tilePos, tile);

                var tileTransform = new Matrix4x4();
                tileTransform.SetTRS(Vector3.zero, Quaternion.Euler(0, 0, tileRotation), Vector3.one);
                elements.Tilemap.SetTransformMatrix(tilePos, tileTransform);
            }
        }

        tilemapCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
        tilemapCollider.GenerateGeometry();
        elements.Tilemap.CompressBounds();

        // Place new Objects
        for (int i = 0; i < data.Objects.Count; ++i)
        {
            var storedObj = data.Objects[i];
            var prefab = elements.ObjectPrefabList.ObjectPrefabs[storedObj.ObjectIdx];
#if UNITY_EDITOR
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#else
            var instance = GameObject.Instantiate(prefab);
#endif
            storedObj.Transform.Set(instance.transform);

            var modifiers = instance.GetComponentsInChildren<SludgeModifier>();
            for (int j = 0; j < modifiers.Count(); ++j)
            {
                if (j >= storedObj.Modifiers.Count)
                {
                    Debug.Log($"Object ({instance.name}) stored in json has fewer modifiers than in the current code. Maybe new modifiers were added in the editor after level was saved?");
                    continue;
                }
                JsonUtility.FromJsonOverwrite(storedObj.Modifiers[j], modifiers[j]);
                modifiers[j].OnLoaded();
            }

            instance.transform.SetParent(elements.ObjectsRoot.transform);
        }
    }
}
