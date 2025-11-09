using Sludge.Colors;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModBulletMovement : SludgeModifier
{
    public bool Static = false;
    public double DX;
    public double DY;
    public double X;
    public double Y;
    public SchemeColor SchemeColor1;
    public SchemeColor SchemeColor2;

    float startTime;

    SpriteRenderer spriteRenderer;

    Transform trans;

    private void Awake()
    {
        trans = transform;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        startTime = Time.time;
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

        if (Static)
            return;

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

    private void Update()
    {
        var color1 = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor1);
        var color2 = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor2);

        // Offset flash by position so bullets don't flash in sync
        float flashX = trans.position.x;
        float flashY = trans.position.y;

        if (Static)
        {
            int offset = (int)(flashX * 10) + (int)(flashY * 10);
            var color = (Mathf.Abs(Time.time * 100 + offset) % 50) > 25 ? color1 : color2;
            spriteRenderer.color = color;
        }
        else
        {
            var color = (Mathf.Abs(Time.time * 100 + startTime) % 20) > 10 ? color1 : color2;
            spriteRenderer.color = color;
        }
    }

    public override void EngineTick()
    {
        if (Static)
            return;

        X = SludgeUtil.Stabilize(X + DX * GameManager.TickSize);
        Y = SludgeUtil.Stabilize(Y + DY * GameManager.TickSize);
        trans.position = new Vector3((float)X, (float)Y);
    }
}
