using UnityEngine;

namespace Sludge.Easing
{
    public enum Easings
    {
        Linear,
        Flip,
        BounceStart,
        BounceEnd,
        Sin,
        Cos,
        SmoothStart2,
        SmoothStart3,
        SmoothStart4,
        SmoothStart5,
        SmoothStop2,
        SmoothStop3,
        SmoothStop4,
        SmoothStop5,
        SmoothStep2,
        SmoothStep3,
        SmoothStep4,
        SmoothStep5,
    }

    public static class Ease
    {
        public static double Linear(double t) => t;
        public static double Flip(double t) => 1.0 - t;
        public static double Mix(double a, double b, double t) => (a * (1.0 - t)) + (b * t);
        public static double Scale(double a, double t) => a * t;
        public static double PingPong(double t) => t <= 0.5 ? t * 2 : 1 - ((t - 0.5) * 2);
        public static double BounceStart(double t) => Mathf.Abs(((float)t * 2.0f) - 1.0f);
        public static double BounceEnd(double t) => 1.0 - BounceStart(t);

        public static double Sin(double t) => Mathf.Sin((float)t);
        public static double Cos(double t) => Mathf.Cos((float)t);

        public static double SmoothStart2(double t) => t * t;
        public static double SmoothStart3(double t) => t * t * t;
        public static double SmoothStart4(double t) => t * t * t * t;
        public static double SmoothStart5(double t) => t * t * t * t * t;

        public static double SmoothStop2(double t) => Flip(SmoothStart2(Flip(t)));
        public static double SmoothStop3(double t) => Flip(SmoothStart3(Flip(t)));
        public static double SmoothStop4(double t) => Flip(SmoothStart4(Flip(t)));
        public static double SmoothStop5(double t) => Flip(SmoothStart5(Flip(t)));

        public static double SmoothStep2(double t) => Mix(SmoothStart2(t), SmoothStop2(t), t);
        public static double SmoothStep3(double t) => Mix(SmoothStart2(t), SmoothStop2(t), t);
        public static double SmoothStep4(double t) => Mix(SmoothStart2(t), SmoothStop2(t), t);
        public static double SmoothStep5(double t) => Mix(SmoothStart2(t), SmoothStop2(t), t);

        public static double Arch2(double t) => Scale(Flip(t), t);

        public static double Apply(Easings[] easings, double t)
        {
            if (easings == null)
                return t;

            for (int i = 0; i < easings.Length; ++i)
                t = Apply(easings[i], t);

            return t;
        }

        public static double Apply(Easings easing, double t)
        {
            return easing switch
            {
                Easings.Linear => Linear(t),
                Easings.Flip => Flip(t),
                Easings.BounceStart => BounceStart(t),
                Easings.BounceEnd => BounceEnd(t),

                Easings.Sin => Sin(t),
                Easings.Cos => Cos(t),

                Easings.SmoothStart2 => SmoothStart2(t),
                Easings.SmoothStart3 => SmoothStart3(t),
                Easings.SmoothStart4 => SmoothStart4(t),
                Easings.SmoothStart5 => SmoothStart5(t),

                Easings.SmoothStop2 => SmoothStop2(t),
                Easings.SmoothStop3 => SmoothStop3(t),
                Easings.SmoothStop4 => SmoothStop4(t),
                Easings.SmoothStop5 => SmoothStop5(t),

                Easings.SmoothStep2 => SmoothStep2(t),
                Easings.SmoothStep3 => SmoothStep3(t),
                Easings.SmoothStep4 => SmoothStep4(t),
                Easings.SmoothStep5 => SmoothStep5(t),

                _ => Linear(t),
            };
        }
    }
}
