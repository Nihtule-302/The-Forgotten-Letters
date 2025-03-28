using System;
using System.Collections.Generic;

namespace _Project.Scripts.Core.Utilities
{
    public static class Extentions
    {
        public static IList<T> Shuffle<T>(this IList<T> list) {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);

                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static string ReverseText(this string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
