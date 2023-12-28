using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PillSnapshot : MonoBehaviour
{
    List<TileBase> tiles = new List<TileBase>();
    List<Matrix4x4> transforms = new List<Matrix4x4>();

    public void Push()
    {
        tiles.Clear();
        var tilemap = GetComponent<Tilemap>();
        var bounds = tilemap.cellBounds;
        foreach(var pos in bounds.allPositionsWithin)
        {
            tiles.Add(tilemap.GetTile(pos));
            transforms.Add(tilemap.GetTransformMatrix(pos));
        }
    }

    public void Pop()
    {
        var tilemap = GetComponent<Tilemap>();
        var bounds = tilemap.cellBounds;
        int i = 0;
        foreach (var pos in bounds.allPositionsWithin)
        {
            tilemap.SetTile(pos, tiles[i]);
            tilemap.SetTransformMatrix(pos, transforms[i]);
            i++;
        }
    }
}
