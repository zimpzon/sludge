using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ClampedCircleDrawer : MonoBehaviour
{
    public float maxRadius = 5.0f;
    public float expandSpeed = 0.05f;
    public float breathingSpeed = 4f;
    public float breathingMagnitude= 0.02f;
    public int rayCount = 50;
    public LayerMask obstacleLayer;

    [NonSerialized] public float groundedScore;
    [NonSerialized] public float contactScore;
    [NonSerialized] public Vector2 contactVector;

    private Transform trans;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private float[] lengths;

    Vector3 prevPos;
    Vector3 movementVelocity;
    Vector3 movementDirection;

    private void Awake()
    {
        trans = transform;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CheckSizes();
        Reset();

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
        DebugLinesScript.Show("groundedScore", groundedScore);
        DebugLinesScript.Show("contactScore", contactScore);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + movementDirection);

        float angleStep = 360.0f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            bool hit = Physics2D.Raycast(transform.position, vertex, maxRadius, obstacleLayer).collider != null;
            if (hit)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + vertex * maxRadius);
            }

            vertices[i + 1] = vertex * maxRadius;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < rayCount ;i++)
        {
            lengths[i] = maxRadius;
        }
    }

    void CheckSizes()
    {
        int vertexCount = rayCount + 1;

        if (vertices?.Length != vertexCount)
        {
            lengths = new float[rayCount];

            vertices = new Vector3[vertexCount];
            vertices[0] = Vector3.zero; // center of the circle

            triangles = new int[rayCount * 3];

            mesh?.SetVertices(vertices);
            mesh?.SetIndices(triangles, MeshTopology.Triangles, 0);
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
        movementVelocity = trans.position - prevPos;
        movementDirection = movementVelocity.normalized;
        DrawCircle();

        prevPos = trans.position;
    }

    void DrawCircle()
    {
        groundedScore = 0;
        contactScore = 0;

        float angleStep = 360.0f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            // Raycast to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(trans.position, vertex, maxRadius, obstacleLayer);
            float maxPossibleLength = hit ? hit.distance : maxRadius;

            float dotDown = Vector2.Dot(Vector2.down, vertex); // 1 same direction, -1 opposite direction
            contactScore += dotDown;

            if (hit && dotDown > 0)
                groundedScore += dotDown;

            bool notEnoughRoom = hit && lengths[i] > maxPossibleLength;
            if (notEnoughRoom)
            {
                // immediately clamp length when there is no room
                lengths[i] = hit.distance;
            }
            else
            {
                // there is room, expand
                float target = maxRadius + Mathf.Sin(Time.time * breathingSpeed) * breathingMagnitude * Mathf.Clamp01(-dotDown + 0.5f);
                lengths[i] = Mathf.MoveTowards(lengths[i], target, (float)GameManager.TickSize * expandSpeed);
            }

            vertices[i + 1] = vertex * lengths[i];
        }

        mesh.SetVertices(vertices);
    }
}
