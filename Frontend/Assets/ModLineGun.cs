using Sludge.Modifiers;
using Sludge.Utility;
using UnityEngine;
using DG.Tweening;

public class ModLineGun : SludgeModifier
{
    public double Delay = 1;
    public double DelayBeforeFirstBullet = 0;
    public double BulletSpeed = 1;

    AnimatedAnt ant;
    Transform trans;
    double countdown;
    double firstBulletCountdown;
    Tween childTransTween;

    private void Awake()
    {
        trans = transform;
        ant = GetComponentInChildren<AnimatedAnt>();
    }

    public override void Reset()
    {
        countdown = Delay;
        firstBulletCountdown = DelayBeforeFirstBullet;
    }

    public override void EngineTick()
    {
        if (firstBulletCountdown > 0)
        {
            firstBulletCountdown = SludgeUtil.Stabilize(firstBulletCountdown - GameManager.TickSize);
            return;
        }

        countdown -= GameManager.TickSize;
        if (countdown <= 0)
        {
            countdown = Delay;
            var bullet = BulletManager.Instance.Get();
            if (bullet != null)
            {

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

                ant.ShotFired();

                SoundManager.Play(FxList.Instance.EnemyShoot);
            }
        }
    }
}
