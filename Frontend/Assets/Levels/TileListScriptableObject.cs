using UnityEngine;
using UnityEngine.Tilemaps;

namespace SludgeColors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileListScriptableObject", order = 2)]
    public class TileListScriptableObject : ScriptableObject
    {
        public Tile[] Tiles;
    }
}