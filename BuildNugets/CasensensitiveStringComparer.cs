using System;
using System.Collections;
using System.Collections.Generic;

namespace BuildNugets
{
    public class CasensensitiveStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }
    }
}