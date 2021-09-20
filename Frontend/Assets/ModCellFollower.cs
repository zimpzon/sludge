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
    double timeMoveOneCell = 0.25;
    double timeMoveThisCell = 0.25;
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
        AnimOffset += 0.1f;
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

    void Die()
    {
        GameManager.Instance.DustParticles.transform.position = trans.position;
        GameManager.Instance.DustParticles.Emit(5);
        CellAntManager.Instance.Release(this);
    }

    public override void EngineTick()
    {
        if (GameManager.Instance.FrameCounter == 0) // Oopsie, GameManager calls Reset, EngineTick, then Reset again. Avoid nasty cell claiming.
            return;

        double animSpeed = 1;
        float fAnimIdx = (float)(GameManager.Instance.EngineTime * animSpeed * Anim.Sprites.Length);
        fAnimIdx += animOffset;
        int animIdx = ((int) Mathf.Abs(fAnimIdx)) % Anim.Sprites.Length;
        spriteRenderer.sprite = Anim.Sprites[animIdx];

        double t = Mathf.Clamp01((float)((timeMoveThisCell - moveTimeLeft) / timeMoveThisCell));
        moveTimeLeft -= GameManager.TickSize;
        double x = startX + (targetX - startX) * t;
        double y = startY + (targetY - startY) * t;

        if (moveTimeLeft > 0)
        {
            targetRotZ = Mathf.Atan2((float)(targetX - startX), (float)(startY - targetY)) * Mathf.Rad2Deg;
            if (Mathf.Abs(targetRotZ - currentRotZ) > 90)
            {
                currentRotZ = targetRotZ;
            }
            else
            {
                currentRotZ += Mathf.DeltaAngle(currentRotZ, targetRotZ) > 0 ? Time.deltaTime * 1000 : Time.deltaTime * -1000;
            }

            trans.rotation = Quaternion.Euler(0, 0, currentRotZ);

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
                timeMoveThisCell = timeMoveOneCell * desiredDir.magnitude;
                moveTimeLeft = timeMoveThisCell;
            }
        }
    }
}
