using Sludge.Modifiers;
using Sludge.Tiles;
using UnityEngine;

public class ModCellFollower : SludgeModifier
{
    public AntAnimScriptableObject Anim;

    static float AnimOffset = 0;

    SpriteRenderer spriteRenderer;
    Vector2Int myCell;
    Transform trans;
    double timeMoveOneCell = 0.5;
    double timeMoveThisCell;
    double startX;
    double startY;
    double targetX;
    double targetY;
    double moveTimeLeft;
    double homeX;
    double homeY;
    float targetRotZ;
    float currentRotZ;
    float animOffset;

    private void Awake()
    {
        homeX = transform.position.x;
        homeY = transform.position.y;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animOffset = AnimOffset;
        AnimOffset += 0.371f;
    }

    public override void OnLoaded()
    {
        Awake();
    }

    public override void Reset()
    {
        currentRotZ = 0;
        targetRotZ = 0;
        trans = transform;
        trans.position = new Vector2((float)homeX, (float)homeY);
        trans.rotation = Quaternion.identity;

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

    public void Die()
    {
        LevelCells.Instance.ReleaseCell(myCell);
        CellAntManager.Instance.Release(this);
    }

    public override void EngineTick()
    {
        if (GameManager.I.FrameCounter == 0) // Oopsie, GameManager calls Reset, EngineTick, then Reset again. Avoid nasty cell claiming.
            return;

        //double animSpeed = 2;
        //float fAnimIdx = (float)(GameManager.I.EngineTime * animSpeed * Anim.Sprites.Length);
        //fAnimIdx += animOffset;
        //int animIdx = ((int) Mathf.Abs(fAnimIdx)) % Anim.Sprites.Length;
        //spriteRenderer.sprite = Anim.Sprites[animIdx];

        double t = Mathf.Clamp01((float)((timeMoveThisCell - moveTimeLeft) / timeMoveThisCell));
        moveTimeLeft -= GameManager.TickSize;
        double x = startX + (targetX - startX) * t;
        double y = startY + (targetY - startY) * t;

        targetRotZ = Mathf.Atan2((float)(targetX - startX), (float)(startY - targetY)) * Mathf.Rad2Deg;
        if (Mathf.Abs(targetRotZ - currentRotZ) > 90)
        {
            currentRotZ = targetRotZ;
        }
        else
        {
            currentRotZ += Mathf.DeltaAngle(currentRotZ, targetRotZ) > 0 ? Time.deltaTime * 1000 : Time.deltaTime * -1000;
        }

        //trans.rotation = Quaternion.Euler(0, 0, currentRotZ);

        float scaleY = Mathf.Sin(Time.time * 20.0f + trans.position.x + trans.position.y);
        scaleY = (scaleY + 1) * 0.5f * 0.15f;

        float scaleX = Mathf.Sin(Mathf.PI + Time.time * 20.0f + trans.position.x + trans.position.y);
        scaleX = (scaleX + 1) * 0.5f * 0.075f;

        trans.localScale = new Vector3(1.0f + scaleX, 1.0f + scaleY, 1);

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

            bool playerIsDangerous = GameManager.I.Player.Size == Player.PlayerSize.Large;
            if (playerIsDangerous)
            {
                diffX *= -1;
                diffY *= -1;
            }

            const double Mid = 0.5;
            if (diffX > Mid)
                desiredDir.x = 1;
            else if (diffX < -Mid)
                desiredDir.x = -1;

            if (diffY > Mid)
                desiredDir.y = 1;
            else if (diffY < -Mid)
                desiredDir.y = -1;

            if (LevelCells.Instance.TryClaimMovement(myCell, desiredDir, prioritizeX: true, out var newCell))
            {
                SetTarget(newCell);
                myCell = newCell;
                timeMoveThisCell = timeMoveOneCell * desiredDir.magnitude;
                moveTimeLeft = timeMoveThisCell;
            }
        }
    }
}
