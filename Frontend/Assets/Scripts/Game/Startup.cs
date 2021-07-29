using DG.Tweening;
using UnityEngine;

namespace Sludge.Utility
{
    public static class Startup
    {
        public static void StaticInit()
        {
            Strings.Init();
            LevelList.LoadLevels();
            DOTween.Init();
            Physics2D.simulationMode = SimulationMode2D.Script;
        }
    }
}
