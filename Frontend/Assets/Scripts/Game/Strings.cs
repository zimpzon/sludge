namespace Sludge.Utility
{
    public static class Strings
    {
        public static string[] TimeStrings = new string[6250]; // 0.00 to 99.99 (100 / 0.016)

        public static void Init()
        {
            for (int i = 0; i < TimeStrings.Length; ++i)
                TimeStrings[i] = ((i * GameManager.TickSizeMs) / 1000.0f).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
