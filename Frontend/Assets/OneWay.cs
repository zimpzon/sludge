using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class OneWay : SludgeObject
{
    public override EntityType EntityType => EntityType.Enemy;

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.7f);

        Vector3 position = transform.position;
        Vector3 scale = transform.localScale;

        float height = 0.5f;
        float width = scale.x;

        Gizmos.DrawWireCube(position, new Vector3(width, height, 1f));

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Gizmos.DrawCube(position, new Vector3(width, height, 1f));
    }
}
