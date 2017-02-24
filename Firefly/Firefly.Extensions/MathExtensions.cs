using System;

namespace Firefly.Extensions
{
    public static class MathExtensions
    {
        public static TNumber Clamp<TNumber>(this TNumber val, TNumber min, TNumber max) where TNumber : IComparable
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static TNumber ClampMin<TNumber>(this TNumber val, TNumber min) where TNumber : IComparable
        {
            if (val.CompareTo(min) < 0) return min;
            return val;
        }

        public static TNumber ClampMax<TNumber>(this TNumber val, TNumber max) where TNumber : IComparable
        {
            if (val.CompareTo(max) > 0) return max;
            return val;
        }
    }
}