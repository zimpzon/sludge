using Sludge.Modifiers;
using Sludge.Utility;
using TMPro;
using UnityEngine;

public class ModSnifferLogic : SludgeModifier
{
    static double FollowDelay = 3;
    const double followDelayIncrease = 0.2;
    double myFollowDelay;
    double speed = 0.80;
    double activationTime = -1;
    AnimatedAnt ant;
    CircleCollider2D triggerCollider;
    CircleCollider2D antCollider;
    GameObject textExclamation;
    bool isFollowing;
    Transform trans;
    double baseX;
    double baseY;
    double posX;
    double posY;
    double triggerX;
    double triggerY;
    int frameAtTriggerTime;
    double currentFrame;
    float baseTriggerRadius;

    private void Awake()
    {
        ant = GetComponentInChildren<AnimatedAnt>();
        antCollider = ant.GetComponent<CircleCollider2D>();
        triggerCollider = GetComponent<CircleCollider2D>();
        textExclamation = SludgeUtil.FindByName(transform, "TextExclamation").gameObject;
        trans = transform;
    }

    public override void OnLoaded()
    {
        trans = transform;
        baseX = SludgeUtil.Stabilize(trans.position.x);
        baseY = SludgeUtil.Stabilize(trans.position.y);
        baseTriggerRadius = triggerCollider.radius;
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
        textExclamation.SetActive(false);
        posX = baseX;
        posY = baseY;
        transform.rotation = Quaternion.Euler(0, 0, -90);
        UpdateTransform();
    }

    void UpdateTransform()
    {
        trans.position = new Vector2((float)posX, (float)posY);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (entity != EntityType.Player)
            return;

        if (activationTime < 0)
        {
            SoundManager.Play(FxList.Instance.SnifferActivate);
            activationTime = GameManager.I.EngineTime;
            triggerCollider.radius = 0.25f;
            frameAtTriggerTime = Player.PositionSampleIdx;
            currentFrame = frameAtTriggerTime;
            myFollowDelay = FollowDelay;
            FollowDelay += followDelayIncrease; // We need a further delay or they will all end up overlapping

            triggerX = SludgeUtil.Stabilize(GameManager.PlayerSamples[frameAtTriggerTime].Pos.x);
            triggerY = SludgeUtil.Stabilize(GameManager.PlayerSamples[frameAtTriggerTime].Pos.y);

            textExclamation.SetActive(true);
            transform.rotation = Quaternion.Euler(0, 0, 0);
            SoundManager.Play(FxList.Instance.GhostAwake);
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
            GameManager.I.DustParticles.transform.position = new Vector2((float)posX, (float)posY);
            GameManager.I.DustParticles.Emit(2);
            GameManager.I.DustParticles.transform.position = new Vector2((float)newX, (float)newY);
            GameManager.I.DustParticles.Emit(2);
        }

        posX = newX;
        posY = newY;
    }

    public override void EngineTick()
    {
        if (activationTime < 0)
            return;

        if (!isFollowing)
        {
            double t = (GameManager.I.EngineTime - activationTime) / myFollowDelay;
            posX = Mathf.Lerp((float)baseX, (float)triggerX, (float)t);
            posY = Mathf.Lerp((float)baseY, (float)triggerY, (float)t);
            UpdateTransform();

            if (t >= 1)
            {
                antCollider.offset = Vector2.zero;
                isFollowing = true;
                textExclamation.SetActive(false);
            }

            return;
        }

        // Following
        SetPosFromPlayerFrame((int)currentFrame);
        currentFrame += speed;
        UpdateTransform();
    }
}
