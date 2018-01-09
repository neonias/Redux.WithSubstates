using System;
using System.Collections.Generic;

namespace Redux.WithSubstates.Extensions
{
    internal static class LinqForEachExtension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var element in enumerable)
                action(element);
        }
    }
}
