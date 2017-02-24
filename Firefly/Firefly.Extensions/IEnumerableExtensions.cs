using System.Collections.Generic;
using System.Linq;

namespace Firefly.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns string joined by separator, null or empty values are ignored.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="separator"></param>
        /// <returns>String</returns>
        public static string JoinIgnoreEmpty(this IEnumerable<string> me, string separator)
        {
            return string.Join(separator, me.Where(s => string.IsNullOrEmpty(s) == false));
        }
    }
}