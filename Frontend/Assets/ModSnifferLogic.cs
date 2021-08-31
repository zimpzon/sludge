using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;

public class ModSnifferLogic : SludgeModifier
{
    static double FollowDelay = 2;
    const double followDelayIncrease = 0.25;
    double myFollowDelay;
    double speed = 1.0;
    SpriteRenderer deadAntRenderer;
    double activationTime = -1;
    AnimatedAnt ant;
    Collider2D triggerCollider;
    bool isFollowing;
    Transform trans;
    double baseX;
    double baseY;
    double posX;
    double posY;
    double angle;
    double triggerX;
    double triggerY;
    double triggerAngle;
    int frameAtTriggerTime;
    double currentFrame;

    private void Awake()
    {
        ant = GetComponentInChildren<AnimatedAnt>();
        deadAntRenderer = transform.Find("DeadAnt").GetComponent<SpriteRenderer>();
        triggerCollider = GetComponent<Collider2D>();
        trans = transform;
    }

    public override void OnLoaded()
    {
        baseX = SludgeUtil.Stabilize(trans.position.x);
        baseY = SludgeUtil.Stabilize(trans.position.y);
    }

    public override void Reset()
    {
        FollowDelay = 2; // Global static to separate ants (fixed delay will make them all move x seconds after player = exactly on top of each other)
        trans = transform;
        isFollowing = false;
        activationTime = -1;
        triggerCollider.enabled = true;
        ant.EnableCollider(false);
        ant.animationOffset = Mathf.Clamp01((float)(baseX * 0.117 + baseY * 0.3311));
        ant.animationSpeedScale = 2;

        var col = deadAntRenderer.color;
        col.a = 1;
        deadAntRenderer.color = col;

        angle = 180;
        posX = baseX;
        posY = baseY;
        UpdateTransform();
    }

    void UpdateTransform()
    {
        trans.position = new Vector2((float)posX, (float)posY);
        trans.rotation = Quaternion.Euler(0, 0, (float)angle + 180);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        if (activationTime < 0)
        {
            activationTime = GameManager.Instance.EngineTime;
            triggerCollider.enabled = false;
            frameAtTriggerTime = Player.PositionSampleIdx;
            currentFrame = frameAtTriggerTime;
            myFollowDelay = FollowDelay;
            FollowDelay += followDelayIncrease;

            triggerX = SludgeUtil.Stabilize(Player.PlayerSamples[frameAtTriggerTime].Pos.x);
            triggerY = SludgeUtil.Stabilize(Player.PlayerSamples[frameAtTriggerTime].Pos.y);
            triggerAngle = SludgeUtil.Stabilize(Player.PlayerSamples[frameAtTriggerTime].Angle);
        }
    }

    void SetPosFromPlayerFrame(int frame)
    {
        if (frame < 0)
            frame = 0;

        double newX = SludgeUtil.Stabilize(Player.PlayerSamples[frame].Pos.x);
        double newY = SludgeUtil.Stabilize(Player.PlayerSamples[frame].Pos.y);
        bool largeJump = Mathf.Abs((float)(newX - posX)) + Mathf.Abs((float)(newX - posX)) > 1;
        if (largeJump)
        {
            GameManager.Instance.DustParticles.transform.position = new Vector2((float)posX, (float)posY);
            GameManager.Instance.DustParticles.Emit(2);
            GameManager.Instance.DustParticles.transform.position = new Vector2((float)newX, (float)newY);
            GameManager.Instance.DustParticles.Emit(2);
        }

        posX = newX;
        posY = newY;
        angle = SludgeUtil.Stabilize(Player.PlayerSamples[frame].Angle);
    }

    public override void EngineTick()
    {
        if (activationTime < 0)
            return;

        if (!isFollowing)
        {
            double t = (GameManager.Instance.EngineTime - activationTime) / myFollowDelay;
            var col = deadAntRenderer.color;
            col.a = 1 - Mathf.Clamp01((float)t);
            deadAntRenderer.color = col;
            angle = Mathf.Lerp((float)angle, (float)triggerAngle, (float)t);
            posX = Mathf.Lerp((float)baseX, (float)triggerX, (float)t);
            posY = Mathf.Lerp((float)baseY, (float)triggerY, (float)t);
            UpdateTransform();

            if (t >= 1)
            {
                ant.EnableCollider(true);
                isFollowing = true;
            }

            return;
        }

        // Following
        SetPosFromPlayerFrame((int)currentFrame);
        currentFrame += speed;
        UpdateTransform();
    }
}
