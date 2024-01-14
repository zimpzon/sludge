using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModPushPlayer : SludgeModifier
{
    private Collider2D _collider;
    private Vector3 _prevPos;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    public override void EngineTick()
    {
        int hits = Physics2D.OverlapCollider(_collider, SludgeUtil.PlayerOnlyFilter, SludgeUtil.colliderHits);
        bool playerIsInsideCollider = hits > 0;
        if (playerIsInsideCollider)
        {
            Vector3 delta = transform.position - _prevPos;
            GameManager.I.Player.MovingWallForceMove(delta);
        }

        _prevPos = transform.position;
    }
}
