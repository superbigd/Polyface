using Polyfacing.Core;
using Polyfacing.Core.Decorations.Interception;
using Polyfacing.Domain.Graphing.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Domain.CallNarration
{

    /// <summary>
    /// decorates with narration type of interception
    /// </summary>
    public class HasCallNarration<T> : DecorationBase<T>
    {
        #region Ctor
        public HasCallNarration(T decorated)
            : base(decorated)
        {
            //this.CallsToIntercept = new List<string>();
            this.Narration = NarrationGraph.New();
            
            //add interception logic and then intercept the root
            this.TypedPolyface.WithHasInterception(this.Narration.InterceptionStrategy);
        }

        public static HasCallNarration<T> New(T decorated)
        {
            return new HasCallNarration<T>(decorated);
        }
        #endregion

        #region Properties
        public NarrationGraph Narration { get; private set; }
        #endregion

        #region Interception Configuration
        public HasCallNarration<T> InterceptRoot()
        {

            this.Narration.Enabled  = false;
            this.TypedPolyface.AsHasInterception().InterceptRoot();
            this.Narration.Enabled = true;
            return this;
        }
        #endregion

        #region Methods
        ///// <summary>
        ///// generates a new graph replaying all of the calls
        ///// </summary>
        ///// <returns></returns>
        //public Graph ReplayCalls()
        //{
        //    var rv = Graph.New();

        //    foreach (var each in this.Calls)
        //    {
        //        Debug.WriteLine(string.Format("calling {0} {1}", each.Name, each.Args.AppendFormatEach((x) => x.ToString(), ",")));

        //        //find the method to invoke
        //        var mi = typeof(Graph).GetMethod(each.Name);
        //        if(mi == null)
        //            continue;

        //        //get all the arguments
        //        mi.Invoke(rv, each.Args.ToArray());
        //    }

        //    return rv;
        //}
        #endregion
    }

    public static class HasCallNarrationExtensions
    {
        #region Declarations
        public static string NAME = "HasCallNarration";
        #endregion

        public static HasCallNarration<T> WithCallNarration<T>(this T decorated)
        {
            return HasCallNarration<T>.New(decorated);
        }
        public static Polyface<T> WithCallNarration<T>(this Polyface<T> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            var face = polyface.As<HasCallNarration<T>>(NAME);
            if (face == null)
            {
                var root = polyface.Root;
                var dec = HasCallNarrationExtensions.WithCallNarration(root);
                polyface.Has(NAME, dec);
            }

            return polyface;
        }
        public static Polyface<T> UndecorateCallNarration<T>(this Polyface<T> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            polyface.RemoveFace(NAME);
            return polyface;
        }
        public static HasCallNarration<T> AsCallNarration<T>(this Polyface<T> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            return polyface.As<HasCallNarration<T>>(NAME);
        }
    }
}
