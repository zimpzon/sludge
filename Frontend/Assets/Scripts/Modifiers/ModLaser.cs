using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModLaser : SludgeModifier
{
    public float Quiver = 0.1f;

    LineRenderer line;
    ParticleSystem particlesWorldSpace;
    ParticleSystem particlesLocalSpace;
    Transform trans;
    ModTimeToggle timeToggle;
    Transform bodyTrans;
    Transform lineTrans;
    Vector3 bodyBaseScale;
    Vector3 lineBaseScale;

    private void Awake()
    {
        trans = transform;
        bodyTrans = SludgeUtil.FindByName(transform, "Body");
        lineTrans = SludgeUtil.FindByName(transform, "Line");
        bodyBaseScale = bodyTrans.localScale;

        lineBaseScale = lineTrans.localScale;
        line = GetComponentInChildren<LineRenderer>();
        line.positionCount = 2;
        particlesWorldSpace = SludgeUtil.FindByName(transform, "ParticlesWorldSpace").GetComponentInChildren<ParticleSystem>();
        particlesLocalSpace = SludgeUtil.FindByName(transform, "ParticlesLocalSpace").GetComponentInChildren<ParticleSystem>();
        timeToggle = GetComponent<ModTimeToggle>();
    }

    public override void Reset()
    {
        bodyTrans.localScale = bodyBaseScale;
        lineTrans.localScale = lineBaseScale;

        line.enabled = false;
        particlesWorldSpace.gameObject.SetActive(false);
        particlesLocalSpace.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
            GameManager.I.Player.Kill();
    }

    public override void EngineTick()
    {
    }

    private void Update()
    {
        if (timeToggle.Active && !timeToggle.IsOn())
        {
            bodyTrans.localScale = bodyBaseScale;
            lineTrans.localScale = lineBaseScale;

            line.enabled = false;
            particlesWorldSpace.gameObject.SetActive(false);
            particlesLocalSpace.gameObject.SetActive(false);
            return;
        }

        // slight quivering when laser is on
        Vector3 offset = new Vector3(Random.value * Quiver, Random.value * Quiver, 0);
        bodyTrans.localScale = bodyBaseScale + offset;
        //offset *= 0.25f;
        //LineTrans.localScale = lineBaseScale + offset;

        if (!line.enabled)
        {
            line.enabled = true;
            particlesWorldSpace.gameObject.SetActive(true);
            particlesLocalSpace.gameObject.SetActive(true);
        }

        var direction = (line.transform.rotation * Vector2.right).normalized;

        RaycastHit2D hit = Physics2D.Raycast(trans.position, direction, 1000, SludgeUtil.ScanForWallsLayerMask);
        if (hit.collider == null)
            return;

        float distance = (hit.point - (Vector2)transform.position).magnitude;
        distance = (float)SludgeUtil.Stabilize(distance);

        line.SetPosition(0, Vector2.zero);
        line.SetPosition(1, new Vector2(distance, 0));

        particlesWorldSpace.transform.position = hit.point;
        particlesLocalSpace.transform.position = hit.point;

        // length of laser is determined, check if it hits something killable
        RaycastHit2D killableTarget = Physics2D.Raycast(trans.position, direction, distance, SludgeUtil.KillableLayerMask);
        if (killableTarget.collider != null)
        {
            var entity = SludgeUtil.GetEntityType(killableTarget.transform.gameObject);
            if (entity == EntityType.Player)
            {
                GameManager.I.Player.Kill();
            }
            else if (entity == EntityType.Enemy)
            {
                GameManager.I.KillEnemy(killableTarget.transform.gameObject);
            }
        }
    }
}
