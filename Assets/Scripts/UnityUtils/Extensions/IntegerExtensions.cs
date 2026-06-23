using System;

namespace UnityUtils.Extensions
{
    public static class IntegerExtensions
    {
        // Numarayı her üç basamakta bir nokta koyarak formatla
        public static string FormatWithDots(this int number) => $"{number:N0}".Replace(",", ".");
        public static string FormatWithDots(this int? number) => $"{number:N0}".Replace(",", ".");
    }
}