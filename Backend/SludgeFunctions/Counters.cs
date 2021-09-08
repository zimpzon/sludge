using Sludge;
using System.Threading;

namespace SludgeFunctions
{
    public static class Counters
    {
        public static long totalAttempts;

        static object _lock = new object();
        static Timer _timer;
        static bool hasChanges;

        static string CounterBlobPath = $"{Statics.VersionName}/total-attempts.txt";

        static Counters()
        {
            AsyncHelper.RunSync(() => Statics.BlobStorage.StoreTextBlobAsync(CounterBlobPath + ".started", "", overwriteExisting: true));
            _timer = new Timer(TimerCallback, null, 1000, Timeout.Infinite);

            totalAttempts = 0;
            string strCount = AsyncHelper.RunSync(() => Statics.BlobStorage.GetTextBlobAsync(CounterBlobPath, throwIfNotFound: false));
            if (!string.IsNullOrWhiteSpace(strCount))
            {
                totalAttempts = long.Parse(strCount);
            }
        }

        public static void AddAttemptForLevel(string levelUniqueId)
        {
            lock (_lock)
            {
                totalAttempts++;
                hasChanges = true;
            }
        }

        static private void TimerCallback(object state)
        {
            AsyncHelper.RunSync(() => Statics.BlobStorage.StoreTextBlobAsync(CounterBlobPath + ".timer-callback", "", overwriteExisting: true));
            PersistChanges();
            _timer.Change(1000 * 30, Timeout.Infinite);
        }

        static void PersistChanges()
        {
            if (!hasChanges)
                return;

            AsyncHelper.RunSync(() => Statics.BlobStorage.StoreTextBlobAsync(CounterBlobPath + ".hasChanges", "", overwriteExisting: true));

            lock (_lock)
            {
                AsyncHelper.RunSync(() => Statics.BlobStorage.StoreTextBlobAsync(CounterBlobPath, totalAttempts.ToString(), overwriteExisting: true));
                hasChanges = false;
            }
        }
    }
}
