using System;

namespace Firefly.Extensions
{
    public static class EnumExtensions
    {
        public static bool Equals(this Enum me, string compare)
        {
            return me.ToString() == compare;
        }

        public static bool EqualsInsensitive(this Enum me, string compare)
        {
            return string.Equals(me.ToString(), compare, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}