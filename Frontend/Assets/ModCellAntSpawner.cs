using Sludge.Modifiers;
using UnityEngine;

public class ModCellAntSpawner : SludgeModifier
{
    double Cooldown = 0.5f;
    int Spawns = 50;
    double timeNextSpawn;

    Transform trans;
    Vector2Int myCell;
    Vector2Int myCellRight;

    public override void Reset()
    {
        Spawns = 50;
        trans = transform;
        myCell = LevelCells.Instance.ClaimCell(trans.position);
        myCellRight = myCell + Vector2Int.right;

        timeNextSpawn = GameManager.Instance.EngineTime + Cooldown;
    }

    public override void EngineTick()
    {
        if (Spawns > 0 && GameManager.Instance.EngineTime >= timeNextSpawn)
        {
            if (LevelCells.Instance.TryClaimCell(myCellRight))
            {
                var ant = CellAntManager.Instance.Get();
                ant.transform.position = LevelCells.Instance.CellToWorld(myCellRight);
                ant.OnLoaded();
                ant.Reset();

                timeNextSpawn = GameManager.Instance.EngineTime + Cooldown;
                Spawns--;
            }
        }
    }
}
