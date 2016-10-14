using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using Polyfacing.Core;

namespace Polyfacing.Domain.Graphing.Tree
{
    /// <summary>
    /// container of a method call's invoking data and return data
    /// </summary>
    public class UnitofWorkInfo
    {
        #region Ctor
        private UnitofWorkInfo(string name, object actor, List<object> args, object rv, Exception error)
        {
            this.Name = name;
            this.Actor = actor;
            this.Args = args;
            this.ReturnValue = rv;
            this.Error = error;
        }
        public static UnitofWorkInfo New(string name, object actor, List<object> args, object rv, Exception error)
        {
            return new UnitofWorkInfo(name, actor, args, rv, error);
        }
        #endregion

        #region Properties
        public string Name { get; private set; }
        public object Actor { get; private set; }
        public List<object> Args { get; private set; }
        public object ReturnValue { get; private set; }
        public Exception Error { get; private set; }
        #endregion
    }

    /// <summary>
    /// decorates with tracing
    /// </summary>
    public class GraphTracer : DecorationBase<Graph>
    {

        #region Ctor
        public GraphTracer(Graph decorated)
            : base(decorated)
        {
            this.Calls = new List<UnitofWorkInfo>();

            List<string> interceptedMethods = new List<string>(
                new string[]{
               "AddChild", "RemoveCurrent", "AddNext", "AddPrevious", "MoveToRoot",
               "MoveUp", "MoveDown", "MoveNext", "MovePrevious"});

            //add interception logic and then intercept the root
            this.TypedPolyface.WithHasInterception(InterceptionStrategy.New((proxy, target, msg) =>
            {
                return msg;
            }, (proxy, target, call, msg) =>
            {
                //we intercept the return side to trace
                if (!(msg is ReturnMessage))
                    throw new InvalidOperationException();

                ReturnMessage rm = msg as ReturnMessage;

                Debug.WriteLine("intercepting " + rm.MethodName);

                if (!interceptedMethods.Contains(rm.MethodName))
                {
                    Debug.WriteLine("ignoring " + rm.MethodName);

                    return msg;
                }

                this.Calls.Add(UnitofWorkInfo.New(rm.MethodName, target, rm.Args.ToList(), rm.ReturnValue, rm.Exception));

                //if the return value is the same instance as the target, we assume it's a fluent call and return the proxy instead
                if (object.ReferenceEquals(rm.ReturnValue, target))
                {
                    var newMsg = new ReturnMessage(proxy, rm.OutArgs, rm.OutArgCount, rm.LogicalCallContext, call);
                    return newMsg;
                }
                else
                    return msg;
            }));

            this.TypedPolyface.AsHasInterception().InterceptRoot();

        }

        public static GraphTracer New(Graph decorated)
        {
            return new GraphTracer(decorated);
        }
        #endregion

        #region Properties
        public List<UnitofWorkInfo> Calls { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// generates a new graph replaying all of the calls
        /// </summary>
        /// <returns></returns>
        public Graph ReplayCalls()
        {
            var rv = Graph.New();

            foreach (var each in this.Calls)
            {
                Debug.WriteLine(string.Format("calling {0} {1}", each.Name, each.Args.AppendFormatEach((x) => x.ToString(), ",")));

                //find the method to invoke
                var mi = typeof(Graph).GetMethod(each.Name);
                if(mi == null)
                    continue;

                //get all the arguments
                mi.Invoke(rv, each.Args.ToArray());
            }

            return rv;
        }
        #endregion
    }

    public static class GraphTracerExtensions
    {
        #region Declarations
        public static string NAME = "GraphTracer";
        #endregion

        public static GraphTracer WithTracer(this Graph decorated)
        {
            return GraphTracer.New(decorated);
        }
        public static Polyface<Graph> WithTracer(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            var face = polyface.As<GraphTracer>(NAME);
            if (face == null)
            {
                var root = polyface.Root;
                var dec = GraphTracerExtensions.WithTracer(root);
                polyface.Has(NAME, dec);
            }

            return polyface;
        }
        public static Polyface<Graph> UndecorateTracer(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            polyface.RemoveFace(NAME);
            return polyface;
        }
        public static GraphTracer AsTracer(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            return polyface.As<GraphTracer>(NAME);
        }
    }
}
