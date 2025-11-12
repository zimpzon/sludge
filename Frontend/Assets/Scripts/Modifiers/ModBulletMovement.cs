using Sludge.Colors;
using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModBulletMovement : SludgeModifier
{
    public bool Static = false;
    public bool StartArmed = true;
    public bool IsArmed = true;
    public double DX;
    public double DY;
    public double X;
    public double Y;
    public SchemeColor SchemeColor1;
    public SchemeColor SchemeColor2;

    const float ArmDelayMs = 1000;
    float armTime = 0;
    bool pendingArm;
    float startTime;

    SpriteRenderer spriteRenderer;
    Transform trans;
    BulletSprites bulletSprites;

    private void Awake()
    {
        trans = transform;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        bulletSprites = GetComponent<BulletSprites>();
        startTime = Time.time;
        Reset();
    }

    private void OnValidate()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        bulletSprites = GetComponent<BulletSprites>();
        SetVisual();
    }

    public override void Reset()
    {
        armTime = 0;
        pendingArm = false;
        IsArmed = StartArmed;
        SetVisual();
    }

    void SetVisual()
    {
        spriteRenderer.sprite = IsArmed ? bulletSprites.ArmedSprite : bulletSprites.NotArmedSprite;
        if (pendingArm)
            spriteRenderer.sprite = bulletSprites.PendingArmedSprite;

        if (GameManager.I?.CurrentColorScheme is not null)
        {
            spriteRenderer.color = IsArmed ? ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor1) : ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor2);
            if (pendingArm)
                spriteRenderer.color = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        bool destroyBullet = false;
        if (entity == EntityType.Player)
        {
            if (IsArmed)
            {
                GameManager.I.Player.Kill();
                destroyBullet = true;
            }
            else if (!pendingArm)
            {
                armTime = GameManager.I.EngineTimeMs + ArmDelayMs;
                pendingArm = true;
                SetVisual();
            }
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
        // Offset flash by position so bullets don't flash in sync
        float flashX = trans.position.x;
        float flashY = trans.position.y;

        if (Static)
        {
            // Hacky solution to changing color scheme but color applier only works with a single color (I think). Should just be an event.
            SetVisual();
        }
        else
        {
            var color1 = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor1);
            var color2 = ColorScheme.GetColor(GameManager.I.CurrentColorScheme, SchemeColor2);
            var color = (Mathf.Abs(Time.time * 100 + startTime) % 20) > 10 ? color1 : color2;
            spriteRenderer.color = color;
        }
    }

    public override void EngineTick()
    {
        if (pendingArm && GameManager.I.EngineTimeMs > armTime)
        {
            IsArmed = true;
            pendingArm = false;
            spriteRenderer.sprite = bulletSprites.ArmedSprite;
            SetVisual();
        }

        if (Static)
            return;

        X = SludgeUtil.Stabilize(X + DX * GameManager.TickSize);
        Y = SludgeUtil.Stabilize(Y + DY * GameManager.TickSize);
        trans.position = new Vector3((float)X, (float)Y);
    }
}
