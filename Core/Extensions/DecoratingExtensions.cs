using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Extensions
{
    public static class DecoratingExtensions
    {
        /// <summary>
        /// walks a decoration chain applying the visitor at each link.  if the visitor returns true
        /// stops iteration and returns current link.  
        /// </summary>
        /// <param name="decoration"></param>
        /// <param name="visitorFn"></param>
        /// <returns></returns>
        public static object WalkDecorated(this IDecorating decoration, Func<object, bool> visitorFn)
        {
            object match = null;

            Walker.DepthFirstWalk(decoration, (item) =>
            {
                if (item is IDecorating)
                {
                    return new List<object>() { ((IDecorating)item).Decorated };
                }

                return null;
            }, (item) =>
            {
                //don't process if we've already found a match
                if (match != null)
                    return;

                if (visitorFn != null && visitorFn(item))
                    match = item;
            });

            return match;
        }
        /// <summary>
        /// returns the last non-null decorated value
        /// </summary>
        /// <param name="decoration"></param>
        /// <returns></returns>
        public static object GetLastNonNullDecorated(this IDecorating decoration)
        {
            return decoration.WalkDecorated((item) =>
            {
                if (!(item is IDecorating))
                    return true;

                if (item is IDecorating && ((IDecorating)item).Decorated == null)
                    return true;

                return false;
            });
        }
        /// <summary>
        /// walks the decorated chain for the first type of the provided generic arg
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="decoration"></param>
        /// <returns></returns>
        public static T As<T>(this IDecorating decoration)
        {
            var match = decoration.WalkDecorated((item) =>
            {
                if (item.GetType().Equals(typeof(T)))
                    return true;

                return false;
            });

            T rv = (T)match;
            return rv;
        }
    }
}
