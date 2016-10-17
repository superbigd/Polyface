//using Polyfacing.Core.Utils;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;

//namespace Polyfacing.Core.Services.Serialization.Binary
//{
//    //see: http://jamesryangray.blogspot.ca/2010_09_01_archive.html

//    /// <summary>
//    /// if a type is not marked as serializable (eg. anonymous types), we need to provide a surrogate selector (eg. surrogate 
//    /// factory) to find ISerializationSurrogate's to handle these types.
//    /// </summary>
//    public class StrategizedSurrogateSelector : ISurrogateSelector
//    {
//        private readonly SurrogateSelector innerSelector = new SurrogateSelector();
//        private readonly Type iFormatter = typeof(IFormatter);

//        public void ChainSelector(ISurrogateSelector selector)
//        {
//            innerSelector.ChainSelector(selector);
//        }

//        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
//        {
//            if (!type.IsSerializable)
//            {
//                selector = this;
//                return new UnattributedTypeSerializationSurrogate();
//            }
//            return innerSelector.GetSurrogate(type, context, out selector);
//        }

//        public ISurrogateSelector GetNextSelector()
//        {
//            return innerSelector.GetNextSelector();
//        }
//    }
//}
