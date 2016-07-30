using System;
using System.Collections.Generic;
using System.Text;

namespace Boredbone.Utility.Tools
{
    public static class PathUtility
    {
        private static string separator = System.IO.Path.DirectorySeparatorChar.ToString();

        public static string WithPostSeparator(string str)
        {
            //var separator = System.IO.Path.DirectorySeparatorChar.ToString();
            return str.EndsWith(separator) ? str : (str + separator);
        }

        public static string WithoutPostSeparator(string str)
        {
            //var separator = System.IO.Path.DirectorySeparatorChar.ToString();
            return str.EndsWith(separator) ? str.TrimEnd(System.IO.Path.DirectorySeparatorChar) : str;
        }
    }
}
