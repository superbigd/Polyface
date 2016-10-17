using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml;
using System.Web.Script.Serialization;

namespace Polyfacing.Core.Utils
{
    public class JSON2XML
    {
        public static XDocument ToXML(string json)
        {
            var xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(
    Encoding.ASCII.GetBytes(json), new XmlDictionaryReaderQuotas()));

            return xml;
        }
        public static string ToJSON(string xml)
        {
            var json = new JavaScriptSerializer().Serialize(GetXmlData(XElement.Parse(xml)));
            return json;
        }



        private static Dictionary<string, object> GetXmlData(XElement xml)
        {
            var attr = xml.Attributes().ToDictionary(d => d.Name.LocalName, d => (object)d.Value);
            if (xml.HasElements) attr.Add("_value", xml.Elements().Select(e => GetXmlData(e)));
            else if (!xml.IsEmpty) attr.Add("_value", xml.Value);

            return new Dictionary<string, object> { { xml.Name.LocalName, attr } };
        }
    }
}
