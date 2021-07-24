using UnityEngine;
using UnityEngine.Tilemaps;

namespace Sludge.Tiles
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileListScriptableObject", order = 2)]
    public class TileListScriptableObject : ScriptableObject
    {
        public Tile[] Tiles;

        public int GetTileIndex(TileBase tile)
        {
            for (int i = 0; i < Tiles.Length; ++i)
            {
                if (Tiles[i] == tile)
                    return i;
            }

            Debug.LogError($"Tile not found in TileListScriptableObject.Tiles: {tile.name}");
            return 0;
        }
    }
}