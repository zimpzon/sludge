using DG.Tweening;
using Sludge.Modifiers;
using Sludge.Utility;
using TMPro;
using UnityEngine;

public class ModSlimeBombLogic : SludgeModifier
{
    public double Countdown = 3;

    public Transform InnerSprite;
    public TMP_Text CountdownText;

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
        slimeRenderer = GetComponent<SpriteRenderer>();
        activationCollider = GetComponent<BoxCollider2D>();
        cloudCollider = GetComponent<CircleCollider2D>();
    }

    public override void Reset()
    {
        trans = transform;
        countingDown = false;
        expanding = false;
        CountdownText.enabled = false;
        CountdownText.text = "";
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
            CountdownText.enabled = true;
        }
    }

    public override void EngineTick()
    {
        if (!expanding)
            InnerSprite.transform.localScale = Vector3.one * ((Mathf.Sin((float)GameManager.Instance.EngineTime * 4) + 1) * 0.2f + 0.35f);

        if (countingDown)
        {
            // Counting down
            int second = Mathf.CeilToInt((float)timeLeft);
            if (second != currentSecond)
            {
                CountdownText.text = second.ToString();
                CountdownText.transform.DORewind();
                CountdownText.transform.localScale = Vector3.one * 3.0f;
                CountdownText.transform.DOScale(0.0f, 1.0f);

                currentSecond = second;
            }

            timeLeft -= GameManager.TickSize;

            if (timeLeft <= 0)
            {
                // Explode now
                countingDown = false;
                expanding = true;
                CountdownText.enabled = false;
                slimeRenderer.enabled = true;
                GameManager.Instance.DeathParticles.transform.position = trans.position;
                GameManager.Instance.DeathParticles.Emit(50);
                GameManager.Instance.CameraRoot.DORewind();
                GameManager.Instance.CameraRoot.DOShakePosition(0.2f, 2.0f);
                InnerSprite.transform.localScale = Vector3.one * 0.5f;
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