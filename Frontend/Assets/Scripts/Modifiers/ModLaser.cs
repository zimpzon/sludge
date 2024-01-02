using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ModLaser : SludgeModifier
{
    LineRenderer line;
    ParticleSystem particles;
    ParticleSystem particlesFixed;
    Transform trans;
    ModTimeToggle timeToggle;
    
    private void Awake()
    {
        trans = transform;
        line = GetComponentInChildren<LineRenderer>();
        line.positionCount = 2;
        particles = transform.Find("Particles").GetComponentInChildren<ParticleSystem>();
        particlesFixed = transform.Find("ParticlesFixed").GetComponentInChildren<ParticleSystem>();
        timeToggle = GetComponent<ModTimeToggle>();
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
        if (timeToggle.Active && !timeToggle.IsOn())
        {
            line.enabled = false;
            particles.gameObject.SetActive(false);
            particlesFixed.gameObject.SetActive(false);
            return;
        }

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

        line.SetPosition(0, Vector2.zero);
        line.SetPosition(1, new Vector2(distance, 0));

        particles.transform.position = hit.point;
        particlesFixed.transform.position = hit.point;

        // length of laser is determined, check if it hits something killable
        RaycastHit2D killableTarget = Physics2D.Raycast(trans.position, direction, distance, SludgeUtil.KillableLayerMask);
        if (killableTarget.collider != null)
        {
            DebugLinesScript.Show("HIT", Time.time);

            var entity = SludgeUtil.GetEntityType(killableTarget.transform.gameObject);
            if (entity == EntityType.Player)
            {
                GameManager.I.Player.Kill();
            }
            else if (entity == EntityType.Enemy)
            {
                DebugLinesScript.Show("ENEMY", Time.time);
                GameManager.I.KillEnemy(killableTarget.transform.gameObject);
            }
        }
    }
}
