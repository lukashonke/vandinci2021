using System;
using System.Collections.Generic;

namespace _Chi.Scripts.Utilities
{
    public class DuplicateKeyComparer : IComparer<FloatWithAction>
    {
        public DuplicateKeyComparer()
        {
            
        }
        
        public int Compare(FloatWithAction x, FloatWithAction y)
        {
            int result = x.time.CompareTo(y.time);

            if (result == 0)
                return 1; // Handle equality as being greater. Note: this will break Remove(key) or
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result;
        }
    }

    public struct FloatWithAction
    {
        public float time;
        public Action action;
    }
}