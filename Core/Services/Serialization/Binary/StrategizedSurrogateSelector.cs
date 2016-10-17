using Polyfacing.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Services.Serialization.Binary
{
    //see: http://jamesryangray.blogspot.ca/2010_09_01_archive.html

    /// <summary>
    /// pluggable serialization strategy 
    /// </summary>
    public class StrategizedSurrogateSelector : ISurrogateSelector
    {
        #region Declarations
        private readonly SurrogateSelector innerSelector = new SurrogateSelector();
        private readonly Type iFormatter = typeof(IFormatter);
        #endregion

        #region Ctor
        public StrategizedSurrogateSelector(Func<Type, ISerializationSurrogate> strategy)
        {
            this.Strategy = strategy;
        }
        public static StrategizedSurrogateSelector New(Func<Type, ISerializationSurrogate> strategy)
        {
            return new StrategizedSurrogateSelector(strategy);
        }

        #endregion

        #region Properties
        public Func<Type, ISerializationSurrogate> Strategy { get; set; }
        #endregion

        #region Overrides
        public void ChainSelector(ISurrogateSelector selector)
        {
            innerSelector.ChainSelector(selector);
        }

        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            //if we have no strategy, fall back to default
            if(this.Strategy == null)
                return innerSelector.GetSurrogate(type, context, out selector);

            var rv = this.Strategy(type);
            if(rv == null)//if the strategy we do have produces nothing, fall back to default
                rv = innerSelector.GetSurrogate(type, context, out selector);

            selector = this;
            return rv;
        }

        public ISurrogateSelector GetNextSelector()
        {
            return innerSelector.GetNextSelector();
        }
        #endregion
    }
}
