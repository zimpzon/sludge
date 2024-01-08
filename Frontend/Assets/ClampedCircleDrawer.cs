using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ClampedCircleDrawer : MonoBehaviour
{
    public float maxRadius = 5.0f;
    public int rayCount = 50;
    public LayerMask obstacleLayer;

    private Transform trans;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private void Awake()
    {
        trans = transform;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CheckSizes();

        // triangle indices never change
        CreateTriangles();

        // calc vertices for normals
        Update();
        
        // normals never change after this
        mesh.RecalculateNormals();
    }

    void OnDrawGizmos()
    {
        CheckSizes();

        float angleStep = 360.0f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            bool hit = Physics2D.Raycast(transform.position, vertex, maxRadius, obstacleLayer).collider != null;
            Gizmos.color = hit ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, transform.position + vertex * maxRadius);

            vertices[i + 1] = vertex * maxRadius;
        }
    }

    void CheckSizes()
    {
        int vertexCount = rayCount + 1;

        if (vertices?.Length != vertexCount)
        {
            vertices = new Vector3[vertexCount];
            triangles = new int[rayCount * 3];

            mesh.SetVertices(vertices);
            mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        }
    }

    void CreateTriangles()
    {
        for (int i = 0; i < rayCount; i++)
        {
            int triangleNo = i * 3;
            triangles[triangleNo + 0] = 0;
            triangles[triangleNo + 1] = ((i + 1) % rayCount) + 1;
            triangles[triangleNo + 2] = (i % rayCount) + 1;
        }

        mesh.SetIndices(triangles,  MeshTopology.Triangles, 0);
    }

    void Update()
    {
        CheckSizes();
        DrawCircle();
    }

    void DrawCircle()
    {
        vertices[0] = Vector3.zero; // center of the circle

        float angleStep = 360.0f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            // Raycast to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(trans.position, vertex, maxRadius, obstacleLayer);

            vertices[i + 1] = hit ? ((Vector3)hit.point - trans.position) : vertex * maxRadius;
        }

        mesh.SetVertices(vertices);
    }
}
