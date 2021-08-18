using DG.Tweening;
using Sludge.Easing;
using Sludge.Utility;
using System.Collections;
using UnityEngine;
using Ease = Sludge.Easing.Ease;

public class AnimatedAnt : MonoBehaviour
{
    public enum AntType { Static, Laser, PlainGun };

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
                headTrans.DOPunchPosition(new Vector3(0.03f, 0.03f, 0), 0.1f);
                yield return new WaitForSeconds(0.1f);
            }

            //if (AntennaShake != 0)
            //    antennaLeftTrans.DOPunchRotation(new Vector3(0, 0, (float)AntennaShake), 1.0f);
            //mandibleLeftTrans.DOPunchRotation(new Vector3(0, 0, -25), 0.5f);
            //pupilLeftTrans.DOPunchPosition(new Vector3(0.1f, 0.1f, 0), 0.7f);
            //pupilRightTrans.DOPunchPosition(new Vector3(0.1f, 0.1f, 0), 0.7f);
            yield return null;
        }
    }

    private void Update()
    {
        GnawingMandibles = false;

        if (Type == AntType.Laser)
        {
            double t = SludgeUtil.TimeMod(Time.time * 2.0f);
            double t01 = Ease.PingPong(Ease.Apply(Easings.Linear, t));
            const double Range = 15;
            double rotZ = t01 * Range - Range * 0.5;
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
