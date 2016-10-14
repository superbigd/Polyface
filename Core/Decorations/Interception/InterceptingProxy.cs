using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Decorations.Interception
{
    /// <summary>
    /// marker interface
    /// </summary>
    public interface IInterceptingProxy : IDecorating
    {

    }

    /// <summary>
    /// Is a proxy to a thing, which intercepts according to the supplied interception strategy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InterceptingProxy<T> : RealProxy, IDecorating<T>, IInterceptingProxy
    {
        #region Declarations

        private readonly InterceptionStrategy _strategy = null;
        #endregion

        #region Ctor
        public InterceptingProxy(T decorated, InterceptionStrategy strategy)
        : base(typeof(T))
        {
            if (strategy == null)
                throw new ArgumentNullException("strategy");

            this.Decorated = decorated;
            this._strategy = strategy;
        }
        public static InterceptingProxy<T> New(T decorated, InterceptionStrategy strategy)
        {
            return new InterceptingProxy<T>(decorated, strategy);
        }
        #endregion

        #region Overrides
        public override IMessage Invoke(IMessage msg)
        {
            //pass the proxy implementation back to the strategy
            return this._strategy.Invoke(this.Decorated, msg);
        }
        #endregion

        #region IDecorating
        public T Decorated
        {
            get; private set;
        }

        object IDecorating.Decorated
        {
            get
            {
                return this.Decorated;
            }
        }
        #endregion
    }
    public static class InterceptingProxyExtensions
    {
        public static T GetInterceptingProxy<T>(this T decorated, InterceptionStrategy strategy)
        {
            var proxy = InterceptingProxy<T>.New(decorated, strategy);

            var tproxy = proxy.GetTransparentProxy();

            return (T)tproxy;
        }
    }
}