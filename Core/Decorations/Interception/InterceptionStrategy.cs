using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Polyfacing.Core.Decorations.Interception
{
    /// <summary>
    /// container of interception logic strategies
    /// </summary>
    public class InterceptionStrategy
    {
        #region Ctor
        private InterceptionStrategy(Func<object, IMessage, IMessage> beforeInterceptionFn, Func<object, IMessage, IMessage> afterInterceptionFn)
        {

            this.BeforeInterceptionFn = beforeInterceptionFn;
            this.AfterInterceptionFn = afterInterceptionFn;
        }
        public static InterceptionStrategy New(Func<object, IMessage, IMessage> beforeInterceptionFn, Func<object, IMessage, IMessage> afterInterceptionFn)
        {
            return new InterceptionStrategy(beforeInterceptionFn, afterInterceptionFn);
        }
        #endregion

        #region Properties
        public Func<object, IMessage, IMessage> BeforeInterceptionFn { get; set; }
        public Func<object, IMessage, IMessage> AfterInterceptionFn { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// wires all of the interception behaviour together
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public IMessage Invoke(object actor, IMessage msg)
        {
            //do the before interception
            var scrubbedMsg = this.BeforeInterceptionFn(actor, msg);

            //do the actual call
            IMethodCallMessage call = msg as IMethodCallMessage;
            var args = call.Args;

            try
            {
                object returnValue = actor.GetType().InvokeMember(call.MethodName, 
                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                    null, actor, args);

                var rawResponseMsg = new ReturnMessage(returnValue, args, args.Length, call.LogicalCallContext, call);

                //do the after scrub
                var afterScrub = this.AfterInterceptionFn(actor, rawResponseMsg);
                return afterScrub;
            }
            catch (Exception ex)
            {
                var rawResponseMsg = new ReturnMessage(ex, call);

                //do the after scrub
                var afterScrub = this.AfterInterceptionFn(actor, rawResponseMsg);
                return afterScrub;

            }
        }
        #endregion
    }
}
