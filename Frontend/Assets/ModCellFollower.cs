using Sludge.Modifiers;
using UnityEngine;

public class ModCellFollower : SludgeModifier
{
    Vector2Int myCell;
    Transform trans;
    double timeMoveOneCell = 0.25;
    double startX;
    double startY;
    double targetX;
    double targetY;
    double moveTimeLeft;
    double homeX;
    double homeY;

    public override void OnLoaded()
    {
        homeX = transform.position.x;
        homeY = transform.position.y;
    }

    public override void Reset()
    {
        trans = transform;
        trans.position = new Vector2((float)homeX, (float)homeY);
        myCell = LevelCells.Instance.ClaimCell(trans.position);
        SetTarget(myCell);
        moveTimeLeft = 0;
    }

    void SetTarget(Vector2Int targetCell)
    {
        var currentWorld = LevelCells.Instance.CellToWorld(myCell);
        var targetWorld = LevelCells.Instance.CellToWorld(targetCell);

        startX = currentWorld.x;
        startY = currentWorld.y;
        targetX = targetWorld.x;
        targetY = targetWorld.y;
    }

    public override void EngineTick()
    {
        if (GameManager.Instance.FrameCounter == 0) // Oopsie, GameManager calls Reset, EngineTick, then Reset again. Avoid nasty cell claiming.
            return;

        double t = Mathf.Clamp01((float)((timeMoveOneCell - moveTimeLeft) / timeMoveOneCell));
        moveTimeLeft -= GameManager.TickSize;
        double x = startX + (targetX - startX) * t;
        double y = startY + (targetY - startY) * t;

        if (moveTimeLeft > 0)
        {
            // Move from one cell to another. We only occupy the target cell.
            trans.position = new Vector2((float)x, (float)y);
        }
        else
        {
            // Pick new target, if possible
            Vector2Int desiredDir = Vector2Int.zero;
            double diffX = Player.Position.x - x;
            double diffY = Player.Position.y - y;
            if (diffX > 0.5)
                desiredDir.x = 1;
            else if (diffX < -0.5)
                desiredDir.x = -1;

            if (diffY > 0.5)
                desiredDir.y = 1;
            else if (diffY < -0.5)
                desiredDir.y = -1;

            if (LevelCells.Instance.TryClaimMovement(myCell, desiredDir, out var newCell))
            {
                SetTarget(newCell);
                myCell = newCell;
                moveTimeLeft = timeMoveOneCell;
            }
        }

        DebugLinesScript.Show("myCell", myCell);
        DebugLinesScript.Show("timeLeft", moveTimeLeft);
    }
}
