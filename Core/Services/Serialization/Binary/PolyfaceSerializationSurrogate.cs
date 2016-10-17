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

    /// <summary>
    /// serializes polyface
    /// </summary>
    public class PolyfaceSerializationSurrogate : ISerializationSurrogate
    {
        #region Ctor
        public PolyfaceSerializationSurrogate()
        {
        }
        public static PolyfaceSerializationSurrogate New()
        {
            return new PolyfaceSerializationSurrogate();
        }
        #endregion

        #region Implementation
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if(!(obj is IPolyface))
                throw new ArgumentOutOfRangeException("obj");

            var polyface = (obj as IPolyface);
            var faceNames = polyface.GetFaceNames();

            var root = polyface.GetRoot();
            SerializeLayer("__root", root , info, context);
            info.AddValue("__faces", faceNames);

            foreach (var each in faceNames)
            {
                var face = polyface.As(each);
                SerializeLayer(each, face, info, context);
            }
        }
        private void SerializeLayer(string name, object val, SerializationInfo info, StreamingContext context)
        {
            info.AddValue(name, val);
            info.AddValue(name + "RuntimeType", val.GetType().AssemblyQualifiedName);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            if (!(obj is IPolyface || (obj is IKnowsPolyface)))
                throw new ArgumentOutOfRangeException("obj");

            Polyface polyface = obj as Polyface;

            var root = this.DeserializeLayer("__root", info, context, selector);
            polyface.SetRoot(root);

            var faceNames = info.GetValue("__faces", typeof(List<string>)) as List<string>;
            
            foreach (var each in faceNames)
            {
                var val = this.DeserializeLayer(each, info, context, selector);
                IDecorating dec = val as IDecorating;
                polyface.Has(each, dec);
            }


            return obj;
        }
        private object DeserializeLayer(string name, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var typeName = info.GetString(name + "RuntimeType");
            Type type = Type.GetType(typeName);

            var rv = info.GetValue(name, type);
            return rv;
        }
        #endregion
    }
}
