using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public class RopeNode
    {
        public Vector3 Position;
        public Vector3 PreviousPosition;
    }

    LineRenderer LineRenderer;
    Vector3[] LinePositions;

    private List<RopeNode> RopeNodes = new List<RopeNode>();
    private float NodeDistance = 0.2f;
    private int TotalNodes = 20;
    private float RopeWidth = 0.1f;

    public float Dampen = 0.1f;
    public Transform Node1Lock;
    public Transform Node2Lock;

    void Awake()
    {
        LineRenderer = this.GetComponent<LineRenderer>();

        // Generate some rope nodes based on properties
        Vector3 startPosition = Vector2.zero;
        for (int i = 0; i < TotalNodes; i++)
        {
            RopeNode node = new RopeNode();
            node.Position = startPosition;
            node.PreviousPosition = startPosition;
            RopeNodes.Add(node);

            startPosition.y -= NodeDistance;
        }

        // for line renderer data
        LinePositions = new Vector3[TotalNodes];
    }


    void Update()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        Simulate();

        // Higher iteration results in stiffer ropes and stable simulation
        for (int i = 0; i < 20; i++)
        {
            ApplyConstraint();
        }
    }

    private void Simulate()
    {
        // step each node in rope
        for (int i = 0; i < TotalNodes; i++)
        {
            // derive the velocity from previous frame
            Vector3 velocity = (RopeNodes[i].Position - RopeNodes[i].PreviousPosition) * Dampen;
            RopeNodes[i].PreviousPosition = RopeNodes[i].Position;

            // calculate new position
            Vector3 newPos = RopeNodes[i].Position + velocity;
            RopeNodes[i].Position = newPos;
        }
    }

    private void ApplyConstraint()
    {
        RopeNodes[0].Position = Node1Lock.position;
        RopeNodes[TotalNodes - 1].Position = Node2Lock.position;

        for (int i = 0; i < TotalNodes - 1; i++)
        {
            RopeNode node1 = this.RopeNodes[i];
            RopeNode node2 = this.RopeNodes[i + 1];

            // Get the current distance between rope nodes
            float currentDistance = (node1.Position - node2.Position).magnitude;
            float difference = Mathf.Abs(currentDistance - NodeDistance);
            Vector2 direction = Vector2.zero;

            // determine what direction we need to adjust our nodes
            if (currentDistance > NodeDistance)
            {
                direction = (node1.Position - node2.Position).normalized;
            }
            else if (currentDistance < NodeDistance)
            {
                direction = (node2.Position - node1.Position).normalized;
            }

            // calculate the movement vector
            Vector3 movement = direction * difference;

            // apply correction
            node1.Position -= (movement * 0.5f);
            node2.Position += (movement * 0.5f);
        }
    }

    private void DrawRope()
    {
        LineRenderer.startWidth = RopeWidth;
        LineRenderer.endWidth = RopeWidth;

        for (int n = 0; n < TotalNodes; n++)
        {
            LinePositions[n] = new Vector3(RopeNodes[n].Position.x, RopeNodes[n].Position.y, 0);
        }

        LineRenderer.positionCount = LinePositions.Length;
        LineRenderer.SetPositions(LinePositions);
    }
}