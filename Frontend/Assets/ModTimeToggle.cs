using Sludge.Utility;

namespace Sludge.Modifiers
{
    public class ModTimeToggle : SludgeModifier
    {
        public bool Active = true;
        public double OnFrom = 0.5;
        public double OnTo = 1.0;
        public double TimeMultiplier = 1.0;

        public bool IsOn()
        {
            double t = SludgeUtil.TimeMod(GameManager.I.EngineTime * TimeMultiplier, pingPong: false);
            return t >= OnFrom && t <= OnTo;
        }

        public override void EngineTick()
        {
        }
    }
}
