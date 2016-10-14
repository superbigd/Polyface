using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using Polyfacing.Core;
using Polyfacing.Domain.Graphing.Tree;

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
            this.CallsToIntercept = new List<string>();
            this.CallGraph = Graph.New();

            //add interception logic and then intercept the root
            this.TypedPolyface.WithHasInterception(InterceptionStrategy.New((proxy, target, msg) =>
            {
                IMethodCallMessage call = msg as IMethodCallMessage;

                //filter
                Debug.WriteLine("begin interception " + call.MethodName);
                if (!this.CallsToIntercept.Contains(call.MethodName))
                {
                    Debug.WriteLine("ignoring " + call.MethodName);

                    return msg;
                }

                //mark the beginning of a call
                this.CallGraph.AddChild(CallNarrativeNode.New().Begin(call.MethodName, target, call.Args.ToList()));

                return msg;
            }, (proxy, target, call, msg) =>
            {
                //we intercept the return side to trace
                if (!(msg is ReturnMessage))
                    throw new InvalidOperationException();

                ReturnMessage rm = msg as ReturnMessage;

                //filter
                Debug.WriteLine("end interception " + rm.MethodName);
                if (!this.CallsToIntercept.Contains(rm.MethodName))
                {
                    Debug.WriteLine("ignoring " + rm.MethodName);

                    return msg;
                }

                CallNarrativeNode nodeValue = this.CallGraph.Current.Value as CallNarrativeNode;
                nodeValue.End(rm.ReturnValue, rm.Exception);
                this.CallGraph.MoveUp();

                //if the return value is the same instance as the target, we assume it's a fluent call and return the proxy instead
                if (object.ReferenceEquals(rm.ReturnValue, target))
                {
                    var newMsg = new ReturnMessage(proxy, rm.OutArgs, rm.OutArgCount, rm.LogicalCallContext, call);
                    return newMsg;
                }
                else
                    return msg;
            }));
        }

        public static HasCallNarration<T> New(T decorated)
        {
            return new HasCallNarration<T>(decorated);
        }
        #endregion

        #region Properties
        public List<string> CallsToIntercept { get; set; }
        public Graph CallGraph { get; private set; }
        #endregion

        #region Interception Configuration
        public HasCallNarration<T> InterceptRoot()
        {
            this.TypedPolyface.AsHasInterception().InterceptRoot();
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
