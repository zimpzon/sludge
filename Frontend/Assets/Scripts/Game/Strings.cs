namespace Sludge.Utility
{
    public static class Strings
    {
        public static string[] TimeStrings = new string[60000]; // 0.000 to 59.999

        public static void Init()
        {
            for (int i = 0; i < TimeStrings.Length; ++i)
                TimeStrings[i] = (i / 1000.0).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
