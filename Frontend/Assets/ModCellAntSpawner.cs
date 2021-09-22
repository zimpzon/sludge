using Sludge.Modifiers;
using UnityEngine;

public class ModCellAntSpawner : SludgeModifier
{
    double Cooldown = 0.1f;
    int Spawns;
    double timeNextSpawn;

    Transform trans;
    Vector2Int myCell;
    Vector2Int myCellRight;
    Vector2Int myCellLeft;
    Vector2Int myCellUp;
    Vector2Int myCellDown;

    public override void Reset()
    {
        Spawns = 5000;
        trans = transform;
        myCell = LevelCells.Instance.ClaimCell(trans.position);
        myCellRight = myCell + Vector2Int.right;
        myCellLeft = myCell + Vector2Int.left;
        myCellUp = myCell + Vector2Int.up;
        myCellDown = myCell + Vector2Int.down;

        timeNextSpawn = GameManager.Instance.EngineTime + Cooldown;
    }

    void SpawnAt(Vector2Int cell)
    {
        var ant = CellAntManager.Instance.Get();
        ant.transform.position = LevelCells.Instance.CellToWorld(cell);
        ant.OnLoaded();
        ant.Reset();
        GameManager.Instance.DustParticles.transform.position = ant.transform.position;
        GameManager.Instance.DustParticles.Emit(3);

        timeNextSpawn = GameManager.Instance.EngineTime + Cooldown;
        Spawns--;
    }

    public override void EngineTick()
    {
        if (Spawns > 0 && GameManager.Instance.EngineTime >= timeNextSpawn)
        {
            if (LevelCells.Instance.TryClaimCell(myCellRight))
            {
                SpawnAt(myCellRight);
            }
            else if (LevelCells.Instance.TryClaimCell(myCellLeft))
            {
                SpawnAt(myCellLeft);
            }
            else if (LevelCells.Instance.TryClaimCell(myCellUp))
            {
                SpawnAt(myCellUp);
            }
            else if (LevelCells.Instance.TryClaimCell(myCellDown))
            {
                SpawnAt(myCellDown);
            }
        }
    }
}
