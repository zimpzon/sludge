namespace Sludge
{
    public class RoundResult
    {
        public string ClientId;
        public string Version;
        public string Platform;
        public long UnixTimestamp;
        public string UniqueId;
        public string Ip; // Set by backend server

        public string LevelId;
        public string LevelName;
        public bool IsReplay;
        public bool Cancelled;
        public bool Completed;
        public bool OutOfTime;
        public double RoundTotalTime;
        public double EndTime;
        public bool IsEliteTime;
        public string ReplayData;
        public double ProgressionAfter;
    }
}
