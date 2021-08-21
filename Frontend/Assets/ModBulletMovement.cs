using Sludge.Colors;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModBulletMovement : SludgeModifier
{
    public double DX;
    public double DY;
    public double X;
    public double Y;
    public SchemeColor SchemeColor1;
    public SchemeColor SchemeColor2;

    Color color1;
    Color color2;
    SpriteRenderer spriteRenderer;

    Transform trans;

    private void Awake()
    {
        trans = transform;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Reset();
    }

    public override void Reset()
    {
        if (GameManager.Instance == null)
            return;

        color1 = ColorScheme.GetColor(GameManager.Instance.CurrentColorScheme, SchemeColor1);
        color2 = ColorScheme.GetColor(GameManager.Instance.CurrentColorScheme, SchemeColor2);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
        {
            GameManager.Instance.Player.Kill();
            return;
        }

        if (entity == EntityType.StaticLevel || entity == EntityType.FakeWall)
        {
            GameManager.Instance.DustParticles.transform.position = trans.position;
            GameManager.Instance.DustParticles.Emit(5);
            BulletManager.Instance.Release(this);
        }
    }

    public override void EngineTick()
    {
        X = SludgeUtil.Stabilize(X + DX * GameManager.TickSize);
        Y = SludgeUtil.Stabilize(Y + DY * GameManager.TickSize);
        trans.position = new Vector3((float)X, (float)Y);

        // Offset flash by position so bullets don't flash in sync
        int offset = (int)(X * 30) + (int)(Y * 30);
        var color = ((GameManager.Instance.EngineTimeMs + offset) % 200) > 100 ? color1 : color2;
        spriteRenderer.color = color;
    }
}
