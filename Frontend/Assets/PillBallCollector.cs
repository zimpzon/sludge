using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class PillBallCollector : SludgeObject
{
    public BallCollector TrappedBallCollector;

    public override EntityType EntityType => EntityType.PillBallCollector;

    public override void Reset()
    {
        gameObject.SetActive(true);
        TrappedBallCollector.transform.SetParent(transform);

        TrappedBallCollector.Reset();
        TrappedBallCollector.HoldPosition(hold: true);

        base.Reset();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player || entity == EntityType.BallCollector)
        {
            SoundManager.Play(FxList.Instance.BallCollectorSpawn);
            TrappedBallCollector.transform.SetParent(null);
            TrappedBallCollector.HoldPosition(hold: false);
            gameObject.SetActive(false);
        }
    }
}
