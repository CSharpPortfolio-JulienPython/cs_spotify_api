using System;

namespace Utils
{
    static class ExtensionMethods
    {
        public static DateTime StartOfUnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static double GetUnixEpoch(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime() - StartOfUnixEpoch).TotalSeconds;
        }
    }
}