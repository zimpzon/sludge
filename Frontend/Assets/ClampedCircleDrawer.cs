using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ClampedCircleDrawer : MonoBehaviour
{
    public string sortingLayerName = string.Empty;
    public int orderInLayer = 0;

    public float maxRadius = 5.0f;
    public float expandSpeed = 0.05f;
    public float breathingSpeed = 4f;
    public float breathingMagnitude= 0.02f;
    public float dotScoreThreshold = 0.5f;
    public int rayCount = 32;
    public LayerMask obstacleLayer;

    float contactScoreAll;
    float contactScoreUp;
    float contactScoreDown;
    float contactScoreLeft;
    float contactScoreRight;

    [NonSerialized] public bool disableCollisions;

    [NonSerialized] public bool hasAnyContact;
    [NonSerialized] public bool hasLeftContact;
    [NonSerialized] public bool hasRightContact;
    [NonSerialized] public bool hasHeadContact;
    [NonSerialized] public bool hasGroundContact;

    private Transform trans;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private float[] lengths;

    Vector3 prevPos;

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

    void SetSortingLayer()
    {
        if (sortingLayerName != string.Empty)
        {
            var renderer = GetComponent<Renderer>();
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = orderInLayer;
        }
    }

    private void Start()
    {
        SetSortingLayer();
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
        DrawCircle();
        prevPos = trans.position;
    }

    void DrawCircle()
    {
        contactScoreAll = 0;
        contactScoreUp = 0;
        contactScoreDown = 0;
        contactScoreLeft = 0;
        contactScoreRight = 0;

        float angleStep = 360.0f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            if (disableCollisions)
            {
                vertices[i + 1] = vertex * lengths[i];
                continue;
            }

            // Raycast to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(trans.position, vertex, maxRadius, obstacleLayer);
            float maxPossibleLength = hit ? hit.distance : maxRadius;

            // dot: // 1 same direction, -1 opposite direction
            float dotUp = Vector2.Dot(Vector2.up, vertex);
            float dotDown = Vector2.Dot(Vector2.down, vertex);
            float dotLeft = Vector2.Dot(Vector2.left, vertex);
            float dotRight = Vector2.Dot(Vector2.right, vertex);

            if (hit)
            {
                contactScoreUp += dotUp > dotScoreThreshold ? dotUp : 0.0f;
                contactScoreDown += dotDown > dotScoreThreshold ? dotDown : 0.0f;
                contactScoreLeft += dotLeft > dotScoreThreshold ? dotLeft : 0.0f;
                contactScoreRight += dotRight > dotScoreThreshold ? dotRight : 0.0f;
            }

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

        contactScoreAll = (contactScoreUp + contactScoreDown + contactScoreLeft + contactScoreRight) / 4;

        hasAnyContact = contactScoreAll > 0;
        hasHeadContact = contactScoreUp > 0;
        // 6.47 is max on vertical surface
        hasLeftContact = contactScoreLeft > 6.4f;
        hasRightContact = contactScoreRight > 6.4f;
        hasGroundContact = contactScoreDown > 0;

        DebugLinesScript.Show("contactScoreLeft", contactScoreLeft);
        DebugLinesScript.Show("contactScoreRight", contactScoreRight);

        //if (contactScoreUp > 0)
        //    Debug.DrawLine(trans.position, trans.position + Vector3.up * 2, Color.yellow, 0.05f);

        //if (contactScoreDown > 0)
        //    Debug.DrawLine(trans.position, trans.position + Vector3.down * 2, Color.yellow, 0.05f);

        //if (contactScoreLeft > 0)
        //    Debug.DrawLine(trans.position, trans.position + Vector3.left * 2, Color.yellow, 0.05f);

        //if (contactScoreRight > 0)
        //    Debug.DrawLine(trans.position, trans.position + Vector3.right * 2, Color.yellow, 0.05f);

        mesh.SetVertices(vertices);
    }
}
