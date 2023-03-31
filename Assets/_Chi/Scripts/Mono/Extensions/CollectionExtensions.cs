using System.Collections.Generic;
using System.Linq;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class CollectionExtensions
    {
        public static bool HasValues<T>(this IEnumerable<T> val)
        {
            return val != null && val.Any();
        }
    }
}