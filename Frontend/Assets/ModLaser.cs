using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModLaser : SludgeModifier
{
    LineRenderer line;
    ParticleSystem particles;
    ParticleSystem particlesFixed;
    Transform trans;
    Transform bodyTrans;
    const float RotationSpeed = 100;
    float timeOffset;

    private void Awake()
    {
        trans = transform;
        line = GetComponentInChildren<LineRenderer>();
        line.positionCount = 2;
        particles = transform.Find("Particles").GetComponentInChildren<ParticleSystem>();
        particlesFixed = transform.Find("ParticlesFixed").GetComponentInChildren<ParticleSystem>();
        bodyTrans = transform.Find("Body").transform;
    }

    public override void Reset()
    {
        timeOffset = (int)trans.position.x * 10 + (int)trans.position.y * 10;
        line.enabled = false;
        particles.gameObject.SetActive(false);
        particlesFixed.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity == EntityType.Player)
            GameManager.Instance.Player.Kill();
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

        var hit = Physics2D.Raycast(trans.position, direction, 1000);
        if (hit.collider == null)
            return;

        bool isPlayer = 1 << hit.collider.gameObject.layer == SludgeUtil.PlayerLayerMask;
        if (isPlayer)
            GameManager.Instance.Player.Kill();

        float distance = (hit.point - (Vector2)transform.position).magnitude;
        distance = (float)SludgeUtil.Stabilize(distance);

        line.SetPosition(0, Vector2.zero);
        line.SetPosition(1, new Vector2(distance, 0));

        particles.transform.position = hit.point;
        particlesFixed.transform.position = hit.point;

        float rotZ = (float)(GameManager.Instance.EngineTime + timeOffset) * RotationSpeed;
        bodyTrans.rotation = Quaternion.Euler(0, 0, rotZ);

        //float scale = (float)Ease.SmoothStep2(Ease.PingPong(SludgeUtil.TimeMod(GameManager.Instance.EngineTime * 0.17f + timeOffset)));
        //scale = 1 - scale * 0.2f;
        //bodyTrans.localScale = Vector2.one * scale;
    }
}
