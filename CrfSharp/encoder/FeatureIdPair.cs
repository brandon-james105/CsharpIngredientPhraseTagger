using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRFSharp
{
    public sealed class FeatureIdPair
    {
        public long Key;
        public int Value;

        public FeatureIdPair(long key, int value)
        {
            Key = key;
            Value = value;
        }
    }
}
