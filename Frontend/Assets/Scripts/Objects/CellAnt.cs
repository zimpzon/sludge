using Sludge;
using Sludge.SludgeObjects;
using Sludge.Utility;

public class CellAnt : SludgeObject, IEnemy
{
    public override EntityType EntityType => EntityType.Enemy;

    public void Kill()
    {
        GetComponent<ModCellFollower>().Die();
    }
}
