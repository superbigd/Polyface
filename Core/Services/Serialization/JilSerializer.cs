using Jil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Services.Serialization
{
    //https://github.com/kevin-montrose/Jil

    public class JilSerializer : ISerializer
    {
        #region Ctor
        private JilSerializer()
        {

        }
        public static JilSerializer New()
        {
            return new JilSerializer();
        }
        #endregion

        #region IHasId
        public string Id
        {
            get { return typeof(JilSerializer).Name; }
        }
        #endregion

        #region ISerializer
        public bool CanHandle(object obj)
        {
            return true;
        }

        public string Serialize(object obj)
        {
            var rv = JSON.Serialize(obj);
            return rv;
        }

        public object Deserialize(string text, Type type)
        {
            var rv = JSON.Deserialize(text, type);
            return rv;
        }
        #endregion


    }
}
