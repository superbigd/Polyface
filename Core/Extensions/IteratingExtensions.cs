using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{
    public static class IteratingExtensions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            var r = new Random();
            var count = enumerable.Count();

            if (count == 0)
                return default(T);

            var rv = enumerable.ToList().ElementAt(r.Next(0, count));
            return rv;
        }
        public static bool TrueForSet<T>(this IEnumerable<T> enumerable, Func<T, bool> fn, bool falseOnEmpty)
        {
            var count = enumerable.Count();

            if (count == 0)
                if (falseOnEmpty)
                    return false;
                else
                    return true;

            var list = enumerable.ToList();
            foreach (var each in list)
                if (!fn(each))
                    return false;

            return true;
        }
        public static IEnumerable<T> Each<T>(this IEnumerable<T> list, Func<T, bool> itemFn)
        {
            if (list == null)
                return null;

            if (itemFn == null)
                return list;

            foreach (var each in list)
                if (itemFn(each))
                    break;

            return list;
        }

        public static string AppendFormatEach<T>(this IEnumerable<T> list, Func<T, string> formatFn, string delim)
        {
            var sb = new StringBuilder();

            foreach (var each in list)
            {
                var line = formatFn(each);
                sb.Append(line);
            }

            return sb.ToString();
        }

        #region To List
        /// <summary>
        /// creates a list from this string, with the first item being the original string 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<T> AddToList<T>(this T item)
        {
            var list = new List<T>();
            list.Add(item);
            return list;
        }
        public static List<T> AddToList<T>(this List<T> list, T item)
        {
            if (list == null)
            {
                return item.AddToList();
            }

            list.Add(item);
            return list;
        }
        #endregion
    }
}
