using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.Tools
{
    public static class UnixTime
    {
        private static readonly long unixEpochTime
            = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;


        public static long FromDateTime(DateTimeOffset time)
#if NET45
            => FromTicks(time.UtcDateTime.Ticks);
#else
            => time.ToUnixTimeSeconds();
#endif

        public static long FromDateTime(DateTime time)
            => FromTicks(time.ToUniversalTime().Ticks);

        private static long FromTicks(long utcTicks)
            => (utcTicks - unixEpochTime) / 10000000L;

        public static DateTime ToDateTime(long time)
            => DateTime.FromBinary((time * 10000000L + unixEpochTime) | 0x4000000000000000L);


        public static DateTimeOffset ToLocalDateTime(long time)
#if NET45
            => new DateTimeOffset(ToDateTime(time)).ToLocalTime();
#else
            => DateTimeOffset.FromUnixTimeSeconds(time).ToLocalTime();
#endif

        public static DateTime DefaultDateTimeUtc { get; } = ToDateTime(0);
        public static DateTimeOffset DefaultDateTimeOffsetLocal { get; } = ToLocalDateTime(0);

    }
}
