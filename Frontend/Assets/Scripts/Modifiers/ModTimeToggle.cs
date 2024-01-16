using Sludge.Utility;
using UnityEngine;

namespace Sludge.Modifiers
{
    public class ModTimeToggle : SludgeModifier
    {
        public bool Active = true;
        public double OnFrom = 0.5;
        public double OnTo = 1.0;
        public double TimeMultiplier = 1.0;
        public bool UseUnityTime = true;

        public bool IsOn()
        {
            double time = UseUnityTime ? Time.time : GameManager.I.EngineTime;
            double t = SludgeUtil.TimeMod(time * TimeMultiplier, pingPong: false);
            return t >= OnFrom && t <= OnTo;
        }

        public override void EngineTick()
        {
        }
    }
}
