using Assets.Scripts.Game;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModMutatorJumpType : SludgeModifier
{
    public string DisplayText;

    Vector3 basePos;

    public MutatorJumpType JumpType = MutatorJumpType.DoubleJump;

    private void Awake()
    {
        basePos = transform.position;
    }

    public override void Reset()
    {
        transform.position = basePos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        MutatorUtil.OnPickupJumpType(this);

        transform.position = new Vector3(1000, Random.value * 1000, 0);
    }

    public override void EngineTick()
    {
    }
}
