using Sludge;
using System.Threading;

namespace SludgeFunctions
{
    public static class Counters
    {
        static long totalAttempts = -1;
        static object _lock = new object();
        static Timer _timer;
        static bool hasChanges;

        static string CounterBlobPath = $"{Statics.VersionName}/total-attempts.txt";

        public static long GetAttempts()
        {
            if (totalAttempts == -1)
            {
                if (_timer == null)
                    _timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);

                // Initialize from blob
                totalAttempts = 0;
                string strCount = AsyncHelper.RunSync(() => Statics.BlobStorage.GetTextBlobAsync(CounterBlobPath, throwIfNotFound: false));
                if (!string.IsNullOrWhiteSpace(strCount))
                {
                    totalAttempts = long.Parse(strCount);
                }
            }
            return totalAttempts;
        }

        public static void AddAttemptForLevel()
        {
            totalAttempts++;
            hasChanges = true;
        }

        static private void TimerCallback(object state)
        {
            //AsyncHelper.RunSync(() => Statics.BlobStorage.StoreTextBlobAsync(CounterBlobPath + ".timer-callback", "", overwriteExisting: true));
            PersistChanges();
            _timer.Change(1000 * 30, Timeout.Infinite);
        }

        static void PersistChanges()
        {
            if (!hasChanges)
                return;

            //AsyncHelper.RunSync(() => Statics.BlobStorage.StoreTextBlobAsync(CounterBlobPath + ".hasChanges", "", overwriteExisting: true));

            lock (_lock)
            {
                AsyncHelper.RunSync(() => Statics.BlobStorage.StoreTextBlobAsync(CounterBlobPath, totalAttempts.ToString(), overwriteExisting: true));
                hasChanges = false;
            }
        }
    }
}
