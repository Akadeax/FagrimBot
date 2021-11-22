using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FagrimBot.Core
{
    public static class Extensions
    {
        private static readonly Random rnd = new();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T val = list[k];
                list[k] = list[n];
                list[n] = val;
            }
        }

        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            return values.All(value => source.Contains(value));
        }
    }
}
