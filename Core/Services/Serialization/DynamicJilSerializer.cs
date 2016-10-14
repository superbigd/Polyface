using Jil;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Extensions;

namespace Polyfacing.Core.Services.Serialization
{
    //https://github.com/kevin-montrose/Jil

    public class DynamicJilSerializer : ISerializer
    {
        #region Ctor
        private DynamicJilSerializer()
        {

        }
        public static DynamicJilSerializer New()
        {
            return new DynamicJilSerializer();
        }
        #endregion

        #region IHasId
        public string Id
        {
            get { return typeof(DynamicJilSerializer).Name; }
        }
        #endregion

        #region ISerializer
        public bool CanHandle(object obj)
        {
            return true;
        }

        public string Serialize(object obj)
        {
            var rv = JSON.SerializeDynamic(obj);
            return rv;
        }

        public object Deserialize(string text, Type type)
        {
            var rv = JSON.DeserializeDynamic(text);
            
            //some tweaks
            if (type.Equals(typeof(ExpandoObject)))
            {
                return BuildExpando(rv);
            }

            return rv;
        }
        private ExpandoObject BuildExpando(dynamic d)
        {
            
            ExpandoObject rv = new ExpandoObject();
            var dict = rv as IDictionary<string, Object>;

            foreach (var keyValue in d)
            {
                var key = keyValue.Key;
                var val = keyValue.Value;

                string valData = val.ToString();
                if(valData.StartsWith("{") && valData.EndsWith("}"))
                {
                    dict.Add(key, BuildExpando(val));
                }
                else
                {
                    dict.Add(key, val);
                }
            }

            return rv;
        }
        #endregion



    }
}
