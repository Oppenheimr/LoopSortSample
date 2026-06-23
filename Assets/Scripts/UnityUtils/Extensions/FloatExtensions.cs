namespace UnityUtils.Extensions
{
    public static class FloatExtensions
    {
        // Numarayı her üç basamakta bir nokta koyarak formatla
        public static string FormatWithDots(this float number) => $"{number:N0}".Replace(",", ".");
        public static string FormatWithDots(this float? number) => $"{number:N0}".Replace(",", ".");
    }
}