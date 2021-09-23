using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class CellAnt : SludgeObject, IEnemy
{
    public override EntityType EntityType => EntityType.Enemy;

    public void Kill()
    {
        GetComponent<ModCellFollower>().Die();
    }
}
