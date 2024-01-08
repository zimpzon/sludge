using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;

[RequireComponent(typeof(SpriteSkin))]
public class BoneClamper : MonoBehaviour
{
    public Transform[] bones; // Assign the bones of the sprite in the inspector
    public float maxRadius = 5.0f;
    public LayerMask obstacleLayer;

    private Transform trans;
    private List<Vector3> boneBasePositions = new List<Vector3>();

    private void Awake()
    {
        trans = transform;
        boneBasePositions = bones.Select(bones => bones.position).ToList();
        Reset();
    }

    private void Reset()
    {
        for (int i = 0; i < boneBasePositions.Count; i++)
        {
            bones[i].position = boneBasePositions[i];
        }
    }

    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.right * maxRadius, Color.cyan, 0.03f);
        //Reset();
        //AdjustCircleBones();
    }

    void AdjustCircleBones()
    {
        for (int i = 0; i < boneBasePositions.Count; i++)
        {
            Vector3 direction = (boneBasePositions[i] - trans.position).normalized;
            float distance = maxRadius;

            RaycastHit2D hit = Physics2D.Raycast(trans.position, direction, maxRadius, obstacleLayer);
            if (hit.collider != null)
            {
                distance = hit.distance; // Clamp distance to hit point
            }
            Debug.DrawLine(trans.position, trans.position + direction * maxRadius, hit ? Color.red : Color.green, 0.03f);

            //bones[i].position = trans.position + direction * distance;
        }
    }
}
