using DG.Tweening;

namespace Sludge.Utility
{
    public static class Startup
    {
        public static void StaticInit()
        {
            Strings.Init();
            LevelList.LoadLevels();
            DOTween.Init();
        }
    }
}
