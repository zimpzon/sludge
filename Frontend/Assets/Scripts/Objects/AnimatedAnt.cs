using DG.Tweening;
using Sludge.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedAnt : MonoBehaviour
{
    public enum AntType { Static, Laser, PlainGun, Stalker, Sniffer };

    public AntType Type = AntType.Static;
    [NonSerialized] public double animationSpeedScale = 1;
    [NonSerialized] public double animationOffset = 0;

    public Transform headTrans;

    List<Tweener> tweeners = new List<Tweener>();
    Collider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        StartCoroutine(Loop());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var entity = SludgeUtil.GetEntityType(collision.gameObject);

        if (entity == EntityType.Player)
        {
            // Chaser uses AnimatedAnt even if it is a ghost
            GameManager.I.Player.Kill(Assets.Scripts.PlayerDeathType.Chaser);
        }
    }

    IEnumerator Loop()
    {
        while (true)
        {
            if (Type == AntType.Laser)
            {
                if (tweeners.Count == 0)
                {
                    tweeners.Add(headTrans.DOPunchPosition(new Vector3(0.03f, 0.03f, 0), 0.2f));
                }
                else
                {
                    for (int i = 0; i < tweeners.Count; ++i)
                        tweeners[i].Restart();
                }

                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }
    }

    public void EnableCollider(bool enable)
        => myCollider.enabled = enable;

    public void ShotFired()
    {
        if (tweeners.Count == 0)
        {
        }
        else
        {
            for (int i = 0; i < tweeners.Count; ++i)
                tweeners[i].Restart();
        }
    }

    private void Update()
    {
        int posX = Mathf.RoundToInt((transform.position.x - 0.5f) / 1.5f);
        int posY = -Mathf.RoundToInt((transform.position.y + 6.0f) / 1.5f);
        int idx = posY * 6 + posX;
        float offset = idx / 30.0f;

        animationOffset = offset;
    }
}
