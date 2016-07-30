using System;
using System.Collections.Generic;
using System.Text;

namespace Boredbone.XamlTools
{
    public class ResourceString
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public ResourceString(string key, Func<string, string> getString)
        {
            Key = key;
            Value = getString(key);
        }

    }
}
