using Polyfacing.Core;
using Polyfacing.Core.Decorations.Interception;
using Polyfacing.Domain.Graphing.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polyfacing.Domain.CallNarration
{
    /*
        The idea of narration is to intercept the calls and to keep a log of who is doing what.  To normalize complicated object
     * data into ids.  
     
     *  NarrationGraph keeps 
     */

    public class NarrationGraph
    {
        #region Declarations
        private readonly object _stateLock = new object();
        private int _nextProxyId = 0;
        private Dictionary<object, ProxyOperand> _proxyOperands = new Dictionary<object, ProxyOperand>();
        private InterceptionStrategy _interceptionStrategy = null;

        #endregion

        #region Ctor
        private NarrationGraph()
        {
            this.Enabled = true;

            //this.CallsToIntercept = new List<string>();
            this.CallGraph = Graph.New();

            //initialize graph with a root node
            this.CallGraph.AddChild("root");

            //define an interception strategy
            this._interceptionStrategy = InterceptionStrategy.New((proxy, target, msg) =>
            {
                if (!this.Enabled)
                    return msg;

                IMethodCallMessage call = msg as IMethodCallMessage;

                ////filter
                //Debug.WriteLine("begin interception " + call.MethodName);
                //if (!this.CallsToIntercept.Contains(call.MethodName))
                //{
                //    Debug.WriteLine("ignoring " + call.MethodName);

                //    return msg;
                //}

                //mark the beginning of a call
                this.Begin(call.MethodName, target, call.Args.ToList());

                return msg;
            }, (proxy, target, call, msg) =>
            {
                if (!this.Enabled)
                    return msg;

                //we intercept the return side to trace
                if (!(msg is ReturnMessage))
                    throw new InvalidOperationException();

                ReturnMessage rm = msg as ReturnMessage;

                ////filter
                //Debug.WriteLine("end interception " + rm.MethodName);
                //if (!this.CallsToIntercept.Contains(rm.MethodName))
                //{
                //    Debug.WriteLine("ignoring " + rm.MethodName);

                //    return msg;
                //}

                this.End(rm.ReturnValue, rm.Exception);
                
                //if the return value is the same instance as the target, we assume it's a fluent call and return the proxy instead
                if (object.ReferenceEquals(rm.ReturnValue, target))
                {
                    var newMsg = new ReturnMessage(proxy, rm.OutArgs, rm.OutArgCount, rm.LogicalCallContext, call);
                    return newMsg;
                }
                else
                    return msg;
            });
        }
        public static NarrationGraph New()
        {
            return new NarrationGraph();
        }
        #endregion

        #region Properties
        public Graph CallGraph { get; private set; }
        public InterceptionStrategy InterceptionStrategy { get { return this._interceptionStrategy; } }
        public bool Enabled { get; set; }
        #endregion

        #region Actor Registry
        /// <summary>
        /// lazy registration/getter
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        private ProxyOperand GetOrRegisterProxy(object actor)
        {
            lock (this._stateLock)
            {
                if (this._proxyOperands.ContainsKey(actor))
                {
                    var rv = this._proxyOperands[actor];
                    return rv;
                }
                else
                {
                    var id = Interlocked.Increment(ref this._nextProxyId);
                    var rv = ProxyOperand.New(id, actor);
                    this._proxyOperands[actor] = rv;
                    return rv;
                }
            }
        }
        /// <summary>
        /// replaces the return value with a proxy operand if it is a valid one
        /// </summary>
        /// <param name="rv"></param>
        /// <returns></returns>
        private object ScrubReturnValue(object rv)
        {
            if (!ProxyOperand.IsCompatible(rv))
                return rv;

            var proxy = this.GetOrRegisterProxy(rv);
            return proxy;
        }
        #endregion

        #region Helper Methods

        private void Begin(string name, object actor, List<object> args)
        {
            var proxy = this.GetOrRegisterProxy(actor);

            //mark the beginning of a call
            this.CallGraph.AddChild(CallNarrativeNode.New().Begin(name, proxy.Id, args));
        }
        private void End(object rv, Exception ex)
        {
            var scrubRv = this.ScrubReturnValue(rv);
            CallNarrativeNode nodeValue = this.CallGraph.Current.Value as CallNarrativeNode;
            nodeValue.End(scrubRv, ex);
            this.CallGraph.MoveUp();
        }
        #endregion

        #region Methods
        public string GetXMLReport()
        {
            var rv = this.CallGraph.WithDump().DumpToXML();
            return rv;
        }
        #endregion

    }
}
