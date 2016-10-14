using Jil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Polyfacing.Core.Services.Serialization
{


    public class MSJSONSerializer : ISerializer
    {
        private JavaScriptSerializer _srlzr = new JavaScriptSerializer();

        #region Ctor
        private MSJSONSerializer()
        {

        }
        public static MSJSONSerializer New()
        {
            return new MSJSONSerializer();
        }
        #endregion

        #region IHasId
        public string Id
        {
            get { return typeof(MSJSONSerializer).Name; }
        }
        #endregion

        #region ISerializer
        public bool CanHandle(object obj)
        {
            return true;
        }

        public string Serialize(object obj)
        {
            var rv =  _srlzr.Serialize(obj);
            return rv;
        }

        public object Deserialize(string text, Type type)
        {
            var rv = _srlzr.Deserialize(text, type);
            return rv;
        }
        #endregion


    }
}
