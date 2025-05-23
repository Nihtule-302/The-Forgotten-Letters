using System;
using System.Collections.Generic;

namespace _Project.Scripts.Core.Utilities
{
    public static class Extentions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);

                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static string ReverseText(this string str)
        {
            var charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}