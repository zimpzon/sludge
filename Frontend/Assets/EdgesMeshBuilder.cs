using Sludge.Util;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EdgesMeshBuilder : MonoBehaviour
{
    TilemapCollider2D tilemapCollider;
    MeshFilter mesh;

    private void Start()
    {
        tilemapCollider = GetComponentInParent<TilemapCollider2D>();
        mesh = GetComponent<MeshFilter>();
        
        var fullMesh = tilemapCollider.CreateMesh(useBodyPosition: true, useBodyRotation: true);
        var edges = EdgeHelpers.GetEdges(fullMesh.triangles);

        mesh.mesh = tilemapCollider.CreateMesh(useBodyPosition: true, useBodyRotation: true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
