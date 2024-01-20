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
        if (GameManager.I == null)
            return;

        color1 = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor1);
        color2 = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor2);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        bool destroyBullet = false;
        if (entity == EntityType.Player)
        {
            GameManager.I.Player.Kill();
            destroyBullet = true;
        }
        else if (entity == EntityType.Enemy)
        {
            GameManager.I.KillEnemy(collision.gameObject);
            destroyBullet = true;
        }
        else if (entity == EntityType.Friend)
        {
            collision.gameObject.GetComponent<KidLogicMod>().Kill();
            destroyBullet = true;
        }

        if (entity == EntityType.StaticLevel || entity == EntityType.FakeWall)
        {
            destroyBullet = true;
        }

        if (destroyBullet)
        {
            GameManager.I.DustParticles.transform.position = trans.position;
            GameManager.I.DustParticles.Emit(5);
            BulletManager.Instance.Release(this);
        }
    }

    public override void EngineTick()
    {
        X = SludgeUtil.Stabilize(X + DX * GameManager.TickSize);
        Y = SludgeUtil.Stabilize(Y + DY * GameManager.TickSize);
        trans.position = new Vector3((float)X, (float)Y);

        // Offset flash by position so bullets don't flash in sync
        int offset = (int)(X * 20) + (int)(Y * 20);
        var color = ((GameManager.I.EngineTimeMs + offset) % 200) > 100 ? color1 : color2;
        spriteRenderer.color = color;
    }
}
