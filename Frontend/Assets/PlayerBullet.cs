using Sludge.SludgeObjects;
using Sludge.Utility;
using UnityEngine;

public class PlayerBullet : SludgeObject
{
    public override EntityType EntityType => EntityType.PlayerBullet;

    public bool Alive;

    public double DX;
    public double DY;
    public double X;
    public double Y;

    double distance;

    Transform trans;

    private void Awake()
    {
        trans = transform;
    }

    public override void Reset()
    {
        trans = transform;
        trans.position = Vector2.one * -2132;
        distance = 0;
        Alive = false;
    }

    public void Fire()
    {
        if (Alive)
            return;

        Alive = true;
        distance = 0;

        double angle = SludgeUtil.Stabilize(Mathf.Atan2((float)-DX, (float)DY) * Mathf.Rad2Deg);
        trans.rotation = Quaternion.Euler(0, 0, (float)angle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
            return;

        if (entity == EntityType.Enemy)
        {
            GameManager.Instance.KillEnemy(collision.gameObject);
            Die();
        }
        else if (entity == EntityType.StaticLevel || entity == EntityType.FakeWall)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager.Instance.DustParticles.transform.position = trans.position;
        GameManager.Instance.DustParticles.Emit(2);
        Alive = false;
        distance = 0;
    }

    public void Tick()
    {
        if (!Alive)
        {
            trans.position = Vector2.one * 12221;
            return;
        }

        X = SludgeUtil.Stabilize(X + DX * GameManager.TickSize);
        Y = SludgeUtil.Stabilize(Y + DY * GameManager.TickSize);
        trans.position = new Vector3((float)X, (float)Y);

        double stepLength = Mathf.Sqrt((float)((DX * DX) + (DY * DY))) * GameManager.TickSize;

        distance += stepLength;
        if (distance > 10)
            Die();
    }
}
