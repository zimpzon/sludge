using Assets.Scripts.Game;
using Sludge.Utility;
using UnityEngine;

public class PillCollectorScript : MonoBehaviour
{
    Transform _trans;
    CircleCollider2D _collider;

    private void Awake()
    {
        _trans = transform;
        _collider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        int hits = Physics2D.OverlapCircleNonAlloc(_trans.position, _collider.radius, SludgeUtil.colliderHits, SludgeUtil.PillsLayerMask);
        DebugLinesScript.Show("huts", hits);
        for (int i = 0; i < hits; ++i)
        {
            PillManager.EatPill(SludgeUtil.colliderHits[i].transform.position, PillEater.Player);
        }
    }
}
