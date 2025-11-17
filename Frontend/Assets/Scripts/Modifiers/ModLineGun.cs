using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;
using DG.Tweening;

public class ModLineGun : SludgeModifier
{
    public double Delay = 1;
    public double DelayBeforeFirstBullet = 0;
    public double BulletSpeed = 1;

    Transform trans;
    int nextShotTimeMs;
    int firstBulletEndTimeMs;
    Tween childTransTween;
    bool isFirstTick;

    private void Awake()
    {
        trans = transform;
    }

    public override void Reset()
    {
        isFirstTick = true;
        nextShotTimeMs = 0;
        firstBulletEndTimeMs = GameManager.I.EngineTimeMs + (int)(DelayBeforeFirstBullet * 1000);
    }

    public override void EngineTick()
    {
        if (GameManager.I.EngineTimeMs < firstBulletEndTimeMs)
        {
            return;
        }

        if (GameManager.I.EngineTimeMs >= nextShotTimeMs && !isFirstTick)
        {
            nextShotTimeMs = GameManager.I.EngineTimeMs + (int)(Delay * 1000);
            var bullet = BulletManager.Instance.Get();
            if (bullet != null)
            {
                bullet.transform.position = trans.position;

                var look = SludgeUtil.LookAngle(trans.rotation.eulerAngles.z);
                bullet.DX = SludgeUtil.Stabilize(look.x * BulletSpeed);
                bullet.DY = SludgeUtil.Stabilize(look.y * BulletSpeed);
                bullet.X = SludgeUtil.Stabilize(trans.position.x + look.x * 0.5);
                bullet.Y = SludgeUtil.Stabilize(trans.position.y + look.y * 0.5);

                var childTrans = trans.GetChild(0);
                if (childTransTween == null)
                    childTransTween = childTrans.DOPunchScale(Vector3.one * 0.25f, 0.2f);
                else
                    childTransTween.Restart();

                SoundManager.Play(FxList.Instance.EnemyShoot);
            }
        }

        isFirstTick = false;
    }
}
