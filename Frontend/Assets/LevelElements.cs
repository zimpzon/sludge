using Sludge.Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelElements : MonoBehaviour
{
    public Player Player;
    public Tilemap WallTilemap;
    public Tilemap PillTilemap;
    public GameObject ObjectsRoot;
    public TileListScriptableObject TileList;
    public ObjectListScriptableObject ObjectPrefabList;
}
