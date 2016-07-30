using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.Extensions
{
    public static class MathExtensions
    {
        public static long? ToLong(object source)
        {
            var ui32 = source as uint?;
            var i32 = source as int?;
            var ui64 = source as ulong?;
            var i64 = source as long?;

            return (ui32.HasValue) ? (long)ui32.Value
                : (i32.HasValue) ? (long)i32.Value
                : (ui64.HasValue) ? (long)ui64.Value
                : (i64.HasValue) ? i64
                : null;
        }

        public static bool IsValid(this double num) => !double.IsInfinity(num) && !double.IsNaN(num);

        public static double Limit(this double num, double min, double max)
        {
            if (num < min)
            {
                return min;
            }
            if (num > max)
            {
                return max;
            }
            return num;
        }
        public static int Limit(this int num, int min, int max)
        {
            if (num < min)
            {
                return min;
            }
            if (num > max)
            {
                return max;
            }
            return num;
        }
    }
}
