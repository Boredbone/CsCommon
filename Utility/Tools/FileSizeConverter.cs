using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility
{
    public static class FileSizeConverter
    {
        private const double threshold = 1024;
        

        public static string ConvertFrom(double size)
        {
            return ConvertFrom(size, "#,##0.##");
        }

        public static string ConvertFrom(double size, string specifier)
        {
            string[] suffix = { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
            int index = 0;

            while (size >= threshold)
            {
                size /= threshold;
                index++;
            }

            return string.Format(
                "{0} {1}B",
                size.ToString(specifier),
                index < suffix.Length ? suffix[index] : "-");
        }

        public static string ConvertAuto(double size)
        {
            string[] suffix = { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
            int index = 0;
            

            while (size >= 1000)
            {
                size /= threshold;
                index++;
            }

            if (size < 10)
            {
                return string.Format(
                "{0} {1}B",
                size.ToString("#,##0.00"),
                index < suffix.Length ? suffix[index] : "-");
            }
            if (size < 100)
            {
                return string.Format(
                "{0} {1}B",
                size.ToString("#,##0.0"),
                index < suffix.Length ? suffix[index] : "-");
            }

            return string.Format(
                "{0} {1}B",
                size.ToString("#,##0"),
                index < suffix.Length ? suffix[index] : "-");
        }
    }
}
