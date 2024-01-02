using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

// TODO: Maybe flicker faster and faster (with increasing alpha) until firing a very fast elongated projectile
// (with elongated we won't miss edge colliders when moving fast).
public class ModLaser : SludgeModifier
{
    LineRenderer line;
    ParticleSystem particles;
    ParticleSystem particlesFixed;
    Transform trans;

    private void Awake()
    {
        trans = transform;
        line = GetComponentInChildren<LineRenderer>();
        line.positionCount = 2;
        particles = transform.Find("Particles").GetComponentInChildren<ParticleSystem>();
        particlesFixed = transform.Find("ParticlesFixed").GetComponentInChildren<ParticleSystem>();
    }

    public override void Reset()
    {
        line.enabled = false;
        particles.gameObject.SetActive(false);
        particlesFixed.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
            GameManager.I.Player.Kill();
    }

    public override void EngineTick()
    {
        if (!line.enabled)
        {
            line.enabled = true;
            particles.gameObject.SetActive(true);
            particlesFixed.gameObject.SetActive(true);
        }

        var direction = (line.transform.rotation * Vector2.right).normalized;

        RaycastHit2D hit = Physics2D.Raycast(trans.position, direction, 1000, SludgeUtil.ScanForWallsLayerMask);
        if (hit.collider == null)
            return;

        float distance = (hit.point - (Vector2)transform.position).magnitude;
        distance = (float)SludgeUtil.Stabilize(distance);

        // length of laser is determined, check if it hits player
        RaycastHit2D playerHit = Physics2D.Raycast(trans.position, direction, distance, SludgeUtil.PlayerLayerMask);
        if (playerHit.collider != null)
        {
            GameManager.I.Player.Kill();
        }

        line.SetPosition(0, Vector2.zero);
        line.SetPosition(1, new Vector2(distance, 0));

        particles.transform.position = hit.point;
        particlesFixed.transform.position = hit.point;
    }
}
