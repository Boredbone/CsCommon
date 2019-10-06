using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset ToDate(this DateTimeOffset source)
        {
            return new DateTimeOffset(source.Year, source.Month, source.Day, 0, 0, 0, source.Offset);
        }
    }
}
