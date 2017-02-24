using System;

namespace Firefly.Extensions
{
    public static class TupleExtensions
    {
        public static Tuple<T2, T1> Flip<T1, T2>(this Tuple<T1, T2> me)
        {
            return new Tuple<T2, T1>(me.Item2, me.Item1);
        }
    }
}