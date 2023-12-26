using Sludge.Modifiers;
using UnityEngine;

public class ModCellAntSpawner : SludgeModifier
{
    double Cooldown = 0.25f;
    const int MaxSpawns = 1000;
    int spawnsLeft;
    double timeNextSpawn;

    Transform trans;
    Vector2Int myCell;
    Vector2Int myCellRight;
    Vector2Int myCellLeft;
    Vector2Int myCellUp;
    Vector2Int myCellDown;

    public override void Reset()
    {
        spawnsLeft = MaxSpawns;
        trans = transform;
        myCell = LevelCells.Instance.ClaimCell(trans.position);
        myCellRight = myCell + Vector2Int.right;
        myCellLeft = myCell + Vector2Int.left;
        myCellUp = myCell + Vector2Int.up;
        myCellDown = myCell + Vector2Int.down;

        timeNextSpawn = GameManager.I.EngineTime + Cooldown;
    }

    void SpawnAt(Vector2Int cell)
    {
        var ant = CellAntManager.Instance.Get();
        if (ant == null)
            return;

        ant.transform.position = LevelCells.Instance.CellToWorld(cell);
        ant.gameObject.SetActive(true);
        ant.OnLoaded();
        ant.Reset();
        GameManager.I.DustParticles.transform.position = ant.transform.position;
        GameManager.I.DustParticles.Emit(3);

        timeNextSpawn = GameManager.I.EngineTime + Cooldown;
        spawnsLeft--;
    }

    public override void EngineTick()
    {
        if (spawnsLeft > 0 && GameManager.I.EngineTime >= timeNextSpawn)
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
