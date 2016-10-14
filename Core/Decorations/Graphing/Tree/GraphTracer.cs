using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;
using System.Runtime.Remoting.Messaging;

namespace Polyfacing.Core.Decorations.Graphing.Tree
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
            this.TypedPolyface.WithHasInterception(InterceptionStrategy.New((target, msg) =>
            {
                return msg;
            }, (target, msg) =>
            {
                //we intercept the return side to trace
                if (!(msg is ReturnMessage))
                    throw new InvalidOperationException();


                ReturnMessage rm = msg as ReturnMessage;
                if (!interceptedMethods.Contains(rm.MethodName))
                    return msg;

                this.Calls.Add(UnitofWorkInfo.New(rm.MethodName, target, rm.Args.ToList(), rm.ReturnValue, rm.Exception));

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
