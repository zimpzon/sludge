using Assets.Scripts.Game;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModMutatorAirJump : SludgeModifier
{
    Vector3 basePos;

    public MutatorTypeAirJumpCount AirJump = MutatorTypeAirJumpCount.DoubleJump;

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

        MutatorUtil.OnPickupAirJump(this);

        transform.position = new Vector3(1000, Random.value * 1000, 0);
    }

    public override void EngineTick()
    {
    }
}
