using System;
using System.Collections.Generic;
using System.Linq;

namespace processJobAndSmsApi.Extensions
{
    public static class ListExtensions
    {
        public static (List<T> Valid, List<T> Invalid) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var valid = new List<T>();
            var invalid = new List<T>();
            var processedItems = new HashSet<T>();
            
            foreach (var item in source)
            {
                if (processedItems.Add(item))  // Only process if item hasn't been seen before
                {
                    if (predicate(item))
                        valid.Add(item);
                    else
                        invalid.Add(item);
                }
            }
            
            return (valid, invalid);
        }
    }
}