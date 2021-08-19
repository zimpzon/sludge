using DG.Tweening;
using Sludge.Easing;
using Sludge.Utility;
using System.Collections;
using UnityEngine;
using Ease = Sludge.Easing.Ease;

public class AnimatedAnt : MonoBehaviour
{
    public enum AntType { Static, Laser, PlainGun, Chaser };

    public AntType Type = AntType.Static;
    bool GnawingMandibles;
    double GnawingMandiblesRange = 40;

    public Transform headTrans;
    public Transform mandibleLeftTrans;
    public Transform mandibleRightTrans;
    public Transform antennaLeftTrans;
    public Transform antennaRightTrans;
    public Transform pupilLeftTrans;
    public Transform pupilRightTrans;

    private void Awake()
    {
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            if (Type == AntType.Laser)
            {
                headTrans.DORewind();
                headTrans.DOPunchPosition(new Vector3(0.03f, 0.03f, 0), 0.2f);
                pupilLeftTrans.DORewind();
                pupilLeftTrans.DOPunchPosition(new Vector3(0.02f, 0.01f, 0), 0.1f);
                pupilRightTrans.DORewind();
                pupilRightTrans.DOPunchPosition(new Vector3(0.02f, 0.01f, 0), 0.1f);
                yield return new WaitForSeconds(0.2f);
            }

            //if (AntennaShake != 0)
            //    antennaLeftTrans.DOPunchRotation(new Vector3(0, 0, (float)AntennaShake), 1.0f);
            //pupilLeftTrans.DOPunchPosition(new Vector3(0.1f, 0.1f, 0), 0.7f);
            //pupilRightTrans.DOPunchPosition(new Vector3(0.1f, 0.1f, 0), 0.7f);
            yield return null;
        }
    }

    public void ShotFired()
    {
        mandibleLeftTrans.DORewind();
        mandibleLeftTrans.DOPunchRotation(new Vector3(0, 0, -45), 0.5f);
        antennaLeftTrans.DORewind();
        antennaLeftTrans.DOPunchRotation(new Vector3(0, 0, 30), 0.6f);

        pupilLeftTrans.DORewind();
        pupilLeftTrans.DOPunchPosition(new Vector3(0.05f, 0.02f, 0), 0.1f);
        pupilRightTrans.DORewind();
        pupilRightTrans.DOPunchPosition(new Vector3(0.05f, 0.02f, 0), 0.1f);
    }

    private void Update()
    {
        GnawingMandibles = false;
        double animateAntenna = 0;
        double animateAntennaSpeed = 0;

        if (Type == AntType.Laser)
        {
            animateAntenna = 25;
            animateAntennaSpeed = 2;
        }
        else if (Type == AntType.Chaser)
        {
            GnawingMandibles = true;
            animateAntenna = 35;
            animateAntennaSpeed = 1;
        }

        if (animateAntenna != 0)
        {
            double t = SludgeUtil.TimeMod(Time.time * (float)animateAntennaSpeed);
            double t01 = Ease.PingPong(Ease.Apply(Easings.Linear, t));
            double range = animateAntenna;
            double rotZ = t01 * range - range * 0.5;
            antennaLeftTrans.localRotation = Quaternion.Euler(0, 0, (float)rotZ);
        }

        if (GnawingMandibles)
        {
            double t = SludgeUtil.TimeMod(Time.time);
            double rotZ = Ease.PingPong(Ease.Apply(Easings.Linear, t)) * GnawingMandiblesRange - GnawingMandiblesRange * 0.5;
            mandibleLeftTrans.localRotation = Quaternion.Euler(0, 0, (float)rotZ);
        }

        float antennaZ = antennaLeftTrans.localRotation.eulerAngles.z;
        antennaRightTrans.localRotation = Quaternion.Euler(0, 0, -antennaZ);

        float mandibleZ = mandibleLeftTrans.localRotation.eulerAngles.z;
        mandibleRightTrans.localRotation = Quaternion.Euler(0, 0, -mandibleZ);
    }
}
