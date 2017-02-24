using System;
using System.Collections.Generic;
using System.Linq;

namespace Firefly.Extensions
{
    public static class StringExtensions
    {
        private static readonly Random Random = new Random();

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static T AsEnum<T>(this string value)
        {
            if (value == null)
            {
                return Enum.GetValues(typeof(T)).Cast<T>().First();
            }
            return (T) Enum.Parse(typeof(T), value, true);
        }

        public static string RandomWeakLegible(this string str, int length)
        {
            const string chars = "abcdefghjkmnpqrstuvwxyz23456789";
            str = _random(chars, length);
            return str;
        }

        public static string RandomWeak(this string str, int length)
        {
            return RandomWeak(length);
        }

        public static string RandomAlpha(this string str, int length)
        {
            return RandomAlpha(length);
        }

        public static string RandomWeak(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz1234567890";
            return _random(chars, length);
        }

        public static string RandomAlpha(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return _random(chars, length);
        }

        public static bool Contains(this string str, ICollection<string> strings)
        {
            return strings.Any(str.Contains);
        }

        public static bool Contains(this string str, ICollection<string> strings, out string match)
        {
            foreach (var s in strings)
            {
                if (!str.Contains(s)) continue;
                match = s;
                return true;
            }
            match = null;
            return false;
        }

        private static string _random(string set, int length)
        {
            var rand = new string(Enumerable.Repeat(set, length)
                .Select(s => s[Random.Next(s.Length)])
                .ToArray());

            return rand;
        }
    }
}