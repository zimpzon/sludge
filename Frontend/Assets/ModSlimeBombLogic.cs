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

    SpriteRenderer slimeRenderer;
    Transform trans;
    bool activated;
    bool exploding;
    double timeLeft;
    int currentSecond = -1;
    const double slimeSpeed = 2;

    double slimeScale = 1;

    private void Awake()
    {
        slimeRenderer = GetComponent<SpriteRenderer>();
    }

    public override void Reset()
    {
        trans = transform;
        activated = false;
        exploding = false;
        CountdownText.enabled = false;
        CountdownText.text = "";
        currentSecond = -1;
        slimeRenderer.enabled = false;
        slimeScale = 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated)
            return;

        var entity = SludgeUtil.GetEntityType(collision.gameObject);
        if (exploding)
        {
            // Kill stuff
            Debug.Log($"{collision.gameObject.name} = {entity}");
        }
        else
        {
            if (entity == EntityType.Player)
            {
                GameManager.Instance.DustParticles.transform.position = trans.position;
                GameManager.Instance.DustParticles.Emit(5);

                activated = true;
                GameManager.Instance.OnActivatingBomb();
                timeLeft = Countdown;
                CountdownText.enabled = true;
            }
        }
    }

    public override void EngineTick()
    {
        if (!exploding)
            InnerSprite.transform.localScale = Vector3.one * ((Mathf.Sin((float)GameManager.Instance.EngineTime * 4) + 1) * 0.2f + 0.35f);

        if (activated)
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
                activated = false;
                exploding = true;
                CountdownText.enabled = false;
                slimeRenderer.enabled = true;
                GameManager.Instance.DeathParticles.transform.position = trans.position;
                GameManager.Instance.DeathParticles.Emit(50);
                GameManager.Instance.CameraRoot.DORewind();
                GameManager.Instance.CameraRoot.DOShakePosition(0.2f, 1.0f);
                InnerSprite.transform.localScale = Vector3.one * 0.5f;
                trans.localScale = Vector3.one * (float)slimeScale;
            }
        }
        else if (exploding)
        {
            slimeScale = SludgeUtil.Stabilize(slimeScale + GameManager.TickSize * slimeSpeed);
            trans.localScale = Vector3.one * (float)slimeScale;
        }
    }
}