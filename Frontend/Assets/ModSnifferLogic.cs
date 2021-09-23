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
    CircleCollider2D triggerCollider;
    CircleCollider2D antCollider;
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
    float baseTriggerRadius;

    private void Awake()
    {
        ant = GetComponentInChildren<AnimatedAnt>();
        antCollider = ant.GetComponent<CircleCollider2D>();
        deadAntRenderer = transform.Find("DeadAnt").GetComponent<SpriteRenderer>();
        triggerCollider = GetComponent<CircleCollider2D>();
        baseTriggerRadius = triggerCollider.radius;
        trans = transform;
    }

    public override void OnLoaded()
    {
        trans = transform;
        baseX = SludgeUtil.Stabilize(trans.position.x);
        baseY = SludgeUtil.Stabilize(trans.position.y);
    }

    public override void Reset()
    {
        FollowDelay = 2; // Global static to separate ants (fixed delay will make them all move x seconds after player = exactly on top of each other)
        trans = transform;
        isFollowing = false;
        activationTime = -1;
        triggerCollider.radius = baseTriggerRadius;
        ant.animationOffset = Mathf.Clamp01((float)(baseX * 0.117 + baseY * 0.3311));
        ant.animationSpeedScale = 2;
        antCollider.offset = Vector2.one * 10000; // Hacky: move ant collider so player won't die. If I disabled the collider I couldn't get slimecloud to detect it after reanabling.

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
            SoundManager.Play(FxList.Instance.SnifferActivate);
            activationTime = GameManager.Instance.EngineTime;
            triggerCollider.radius = 0.25f;
            frameAtTriggerTime = Player.PositionSampleIdx;
            currentFrame = frameAtTriggerTime;
            myFollowDelay = FollowDelay;
            FollowDelay += followDelayIncrease;

            triggerX = SludgeUtil.Stabilize(GameManager.PlayerSamples[frameAtTriggerTime].Pos.x);
            triggerY = SludgeUtil.Stabilize(GameManager.PlayerSamples[frameAtTriggerTime].Pos.y);
            triggerAngle = SludgeUtil.Stabilize(GameManager.PlayerSamples[frameAtTriggerTime].Angle);
        }
    }

    void SetPosFromPlayerFrame(int frame)
    {
        if (frame < 0)
            frame = 0;

        double newX = SludgeUtil.Stabilize(GameManager.PlayerSamples[frame].Pos.x);
        double newY = SludgeUtil.Stabilize(GameManager.PlayerSamples[frame].Pos.y);
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
        angle = SludgeUtil.Stabilize(GameManager.PlayerSamples[frame].Angle);
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
                antCollider.offset = Vector2.zero;
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
