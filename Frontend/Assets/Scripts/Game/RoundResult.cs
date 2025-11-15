using Sludge.Utility;

namespace Sludge
{
    public class RoundResult
    {
        public PlayerProgress.LevelNamespace LevelNamespace;
        public int LevelId;
        public bool Cancelled;
        public bool Completed;
        public bool GotTarget;
        public bool GotFirstTarget;
        public bool GotPersonalBest;
        public float Time = -1;
        public float TimeToPersonalBest = 1;
        public float TimeToTarget = -1;
    }
}
