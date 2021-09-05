using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using TMPro;
using UnityEngine;

public class ModSlimeBombLogic : SludgeModifier
{
    public double Countdown = 3;

    Transform innerSprite;
    TMP_Text countdownText;
    BoxCollider2D activationCollider;
    CircleCollider2D cloudCollider;
    SpriteRenderer slimeRenderer;
    Transform trans;
    bool countingDown;
    bool expanding;
    double timeLeft;
    int currentSecond = -1;
    const double slimeSpeedStart = 80;
    const double slimeSpeedMin = 4;
    const double slimeSpeedDampen = 0.93;
    double slimeSpeed;
    double slimeScale = 1;

    private void Awake()
    {
        GetComponents();
    }

    void GetComponents()
    {
        innerSprite ??= transform.parent.Find("InnerSprite");
        countdownText ??= transform.parent.Find("TextCountdown").GetComponent<TMP_Text>();
        slimeRenderer ??= GetComponent<SpriteRenderer>();
        activationCollider ??= GetComponent<BoxCollider2D>();
        cloudCollider ??= GetComponent<CircleCollider2D>();
    }

    public override void Reset()
    {
        GetComponents();
        trans = transform;
        countingDown = false;
        expanding = false;
        countdownText.enabled = false;
        countdownText.text = "";
        currentSecond = -1;
        slimeRenderer.enabled = false;
        slimeScale = 1;
        slimeSpeed = slimeSpeedStart;
        activationCollider.enabled = true;
        cloudCollider.enabled = false;
        SetColliderScale();
    }

    void SetColliderScale()
    {
        trans.localScale = Vector3.one * (float)slimeScale;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!expanding)
            return;

        var go = collision.gameObject;
        var entity = SludgeUtil.GetEntityType(go);
        if (entity == EntityType.Player)
            GameManager.Instance.Player.ExitSlimeCloud();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!expanding)
            return;

        var go = collision.gameObject;
        var entity = SludgeUtil.GetEntityType(go);
        // Kill stuff
        if (entity == EntityType.Player)
        {
            GameManager.Instance.Player.InSlimeCloud();
        }
        else if (entity == EntityType.Enemy)
        {
            GameManager.Instance.KillEnemy(go);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (countingDown || expanding)
            return;

        var go = collision.gameObject;
        var entity = SludgeUtil.GetEntityType(go);
        if (entity == EntityType.Player)
        {
            // Player activated the bomb
            activationCollider.enabled = false;
            cloudCollider.enabled = true;

            countingDown = true;
            GameManager.Instance.OnActivatingBomb();
            timeLeft = Countdown;
            countdownText.enabled = true;
        }
    }

    public override void EngineTick()
    {
        if (!expanding)
            innerSprite.transform.localScale = Vector3.one * ((Mathf.Sin((float)GameManager.Instance.EngineTime * 4) + 1) * 0.2f + 0.35f);

        if (countingDown)
        {
            // Counting down
            int second = Mathf.CeilToInt((float)timeLeft);
            if (second != currentSecond)
            {
                var audioClip = second switch
                {
                    5 => FxList.Instance.Countdown5,
                    4 => FxList.Instance.Countdown4,
                    3 => FxList.Instance.Countdown3,
                    2 => FxList.Instance.Countdown2,
                    1 => FxList.Instance.Countdown1,
                    _ => null,
                };
                if (audioClip != null)
                    SoundManager.Play(audioClip);

                SoundManager.Play(FxList.Instance.ClockTick);

                countdownText.text = second.ToString();
                countdownText.transform.DORewind();
                countdownText.transform.localScale = Vector3.one * 3.0f;
                countdownText.transform.DOScale(0.0f, 1.0f);

                currentSecond = second;
            }

            timeLeft -= GameManager.TickSize;

            if (timeLeft <= 0)
            {
                // Explode now
                SoundManager.Play(FxList.Instance.SlimeBombExplode);
                countingDown = false;
                expanding = true;
                countdownText.enabled = false;
                slimeRenderer.enabled = true;
                GameManager.Instance.DeathParticles.transform.position = trans.position;
                GameManager.Instance.DeathParticles.Emit(50);
                GameManager.Instance.CameraRoot.DORewind();
                GameManager.Instance.CameraRoot.DOShakePosition(0.2f, 2.0f);
                innerSprite.transform.localScale = Vector3.one * 0.5f;
                SetColliderScale();
            }
        }
        else if (expanding)
        {
            slimeScale = SludgeUtil.Stabilize(slimeScale + GameManager.TickSize * slimeSpeed);
            slimeSpeed = SludgeUtil.Stabilize(slimeSpeed * slimeSpeedDampen);
            if (slimeSpeed < slimeSpeedMin)
                slimeSpeed = slimeSpeedMin;

            SetColliderScale();
        }
    }
}