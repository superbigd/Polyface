
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Polyfacing.Core
{
    /// <summary>
    /// describes a string that is convertable to another string, and vice versa
    /// </summary>
    public interface IStringable
    {
        string GetText();
        void ParseText(string text);
        object Value { get; }
    }

    /// <summary>
    /// to facilitate string handling we create a base class upon which to decorate string-y functionality.  
    /// in this way a thing must be converted to stringable, and then the decoration functionality is avail.
    /// </summary>
    public class Stringable : MarshalByRefObject, IKnowsPolyface, IStringable
    {
        #region Declarations
        //private static JavaScriptSerializer _srlzr = new JavaScriptSerializer();
        internal static ITraceWriter _traceWriter = null;
        internal static JsonSerializerSettings _settings = null;

        /// <summary>
        /// helper container to include type information in the serialized data
        /// </summary>
        internal class ObjectEnvelope
        {
            public ObjectEnvelope() { }
            public ObjectEnvelope(object obj)
            {
                if (obj != null)
                {
                    this.Type = obj.GetType().FullName;

                    this.Data = JsonConvert.SerializeObject(obj, _settings);
                    DumpTrace();
                    //this.Data = _srlzr.Serialize(obj);
                }
            }
            public string Type { get; set; }
            public string Data { get; set; }

            public object GetObject()
            {
                if (string.IsNullOrEmpty(this.Type))
                    return null;

                var type = System.Type.GetType(this.Type);

                if (this.Type.Equals("System.Dynamic.ExpandoObject"))
                {
                    //var converter = new ExpandoObjectConverter();
                    dynamic rv = JsonConvert.DeserializeObject<ExpandoObject>(this.Data, _settings);
                    DumpTrace();
                    return rv;
                }
                //else if (type.Equals(typeof(float)))
                //{
                //    var rv = float.Parse(this.Data);
                //    return rv;
                //}
                //else if (type.Equals(typeof(double)))
                //{
                //    var rv = double.Parse(this.Data);
                //    return rv;
                //}
                //else if (type.Equals(typeof(decimal)))
                //{
                //    var rv = decimal.Parse(this.Data);
                //    return rv;
                //}
                else
                {
                    var rv = JsonConvert.DeserializeObject(this.Data, type, _settings);
                    DumpTrace();
                    return rv;
                }

            }
        }
        #endregion

        #region Ctor
        static Stringable()
        {
            InitTrace();
            Trace.AutoFlush = true;
            TracingEnabled = true;
        }
        private Stringable(object obj)
        {
            this.Polyface = Polyface<Stringable>.New(this);
            this.Value = obj;
        }
        public static Stringable New(object obj)
        {
            return new Stringable(obj);
        }
        public static Stringable Parse(string text)
        {
            var rv = Stringable.New(null);
            rv.ParseText(text);
            return rv;
        }
        #endregion

        #region Tracing
        public static bool TracingEnabled { get; set; }
        private static void InitTrace()
        {
            _traceWriter = new MemoryTraceWriter();
            _settings = new JsonSerializerSettings
            {
                TraceWriter = _traceWriter,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Converters = new List<JsonConverter> { new ExpandoObjectConverter(), new StringNumericConverter() }
            };
        }
        private static void DumpTrace()
        {
            if (!TracingEnabled)
                return;

            if(_traceWriter != null)
                Debug.WriteLine(_traceWriter);
            
            //reset the tracer .  there really should be a better way to do this            
            InitTrace();
        }
        #endregion

        #region IStringable
        public string GetText()
        {
            var env = new ObjectEnvelope(this.Value);
            var rv = JsonConvert.SerializeObject(env, _settings);
            DumpTrace();
            return rv;
        }
        public void ParseText(string text)
        {
            var env = JsonConvert.DeserializeObject<ObjectEnvelope>(text, _settings);
            DumpTrace();
            this.Value = env.GetObject();
        }
        public object Value { get; private set; }

        #endregion


        #region IKnowsPolyface - Polyface plumbing
        public Polyface<Stringable> TypedPolyface { get { return this.Polyface as Polyface<Stringable>; } }
        public IPolyface Polyface { get; set; }
        #endregion

    }

    ////http://stackoverflow.com/questions/24051206/handling-decimal-values-in-newtonsoft-json
    //internal class DecimalConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return (objectType == typeof(decimal) || objectType == typeof(decimal?));
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null)
    //            return null;
    //        var value = JToken.Load(reader);
    //        if (value.Type == JTokenType.Float)
    //            return System.ComponentModel.TypeDescriptor.GetConverter(((JValue)value).Value).ConvertToInvariantString(((JValue)value).Value);

    //        return (string)value;
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        Decimal? d = default(Decimal?);
    //        if (value != null)
    //        {
    //            d = value as Decimal?;
    //            if (d.HasValue) // If value was a decimal?, then this is possible
    //            {
    //                d = new Decimal?(new Decimal(Decimal.ToDouble(d.Value))); // The ToDouble-conversion removes all unnessecary precision
    //            }
    //        }
    //        JToken.FromObject(d).WriteTo(writer);
    //    }
    //}

    //http://stackoverflow.com/questions/33421981/newtonsoft-json-deserialize-decimal-numbers-with-more-then-8-decimals

    /*We want to ensure that no data is lost with the types of decimal, float, double as it converts to json with json.net.
     * 
     * During writing we will output a string for these values that has the prefix "__#decimal_" for decimals, "__#double_" for double,
     * "__#float_" for float.  
     * 
     */

    #region JSON.NET specific serialization stuff
    public static class JSONNETScrubber
    {
        #region Declarations
        public const string DEC_PREFIX = "__#decimal_";
        public const string DBL_PREFIX = "__#double_";
        public const string FLT_PREFIX = "__#float_";

        #endregion

        #region Methods
        /// <summary>
        /// applies the data scrub (typically after/during writes/serialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ScrubOn(object value)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal d = (decimal)value;
                    string data = DEC_PREFIX + d.ToString();
                    return data;
                }
                else if (value is float)
                {
                    float f = (float)value;
                    string data = FLT_PREFIX + f.ToString("R");
                    return data;
                }
                else if (value is double)
                {
                    double d = (double)value;
                    string data = DBL_PREFIX + d.ToString("R");
                    return data;
                }
                return value;
            }
            return null;
        }
        /// <summary>
        /// removes the data scrub (typically after/during reads/deserialization)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ScrubOff(object value)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal d = (decimal)value;
                    string data = DEC_PREFIX + d.ToString();
                    return data;
                }
                else if (value is float)
                {
                    float f = (float)value;
                    string data = FLT_PREFIX + f.ToString("R");
                    return data;
                }
                else if (value is double)
                {
                    double d = (double)value;
                    string data = DBL_PREFIX + d.ToString("R");
                    return data;
                }
                return value;
            }
            return null;
        }

        public static void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal d = (decimal)value;
                    string data = DEC_PREFIX + d.ToString();
                    JToken.FromObject(data).WriteTo(writer);
                    return;
                }
                else if (value is float)
                {
                    float f = (float)value;
                    string data = FLT_PREFIX + f.ToString("R");
                    JToken.FromObject(data).WriteTo(writer);
                    return;
                }
                else if (value is double)
                {
                    double d = (double)value;
                    string data = DBL_PREFIX + d.ToString("R");
                    JToken.FromObject(data).WriteTo(writer);
                    return;
                }

            }

            JToken.FromObject(value).WriteTo(writer);
        }
        #endregion
    }

    #endregion

    public class StringNumericConverter : JsonConverter
    {
        #region Declarations
        private const string DEC_PREFIX = "__#decimal_";
        private const string DBL_PREFIX = "__#double_";
        private const string FLT_PREFIX = "__#float_";

        #endregion

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(float) || objectType == typeof(double)
                || objectType == typeof(ExpandoObject) || objectType == typeof(Stringable.ObjectEnvelope)
                ;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            var value = JToken.Load(reader);
            
            if (value.Type == JTokenType.String)
            {
                string token = (string)value;

                //remove leading and trailing quotes
                //token = token.Substring(1, token.Length - 2);

                if (token.StartsWith(DEC_PREFIX))
                {
                    token = token.Replace(DEC_PREFIX, string.Empty);
                    return decimal.Parse(token);
                }
                else if (token.StartsWith(DBL_PREFIX))
                {
                    token = token.Replace(DBL_PREFIX, string.Empty);
                    return double.Parse(token);
                }
                else if (token.StartsWith(FLT_PREFIX))
                {
                    token = token.Replace(FLT_PREFIX, string.Empty);
                    return float.Parse(token);
                }
                return token;
            }
            
            return value;
        }

        public override bool CanWrite { get { return true; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal d = (decimal)value;
                    string data = DEC_PREFIX + d.ToString();
                    JToken.FromObject(data).WriteTo(writer);
                    return;
                }
                else if (value is float)
                {
                    float f = (float)value;
                    string data = FLT_PREFIX +  f.ToString("R");
                    JToken.FromObject(data).WriteTo(writer);
                    return;
                }
                else if (value is double)
                {
                    double d = (double)value;
                    string data = DBL_PREFIX + d.ToString("R");
                    JToken.FromObject(data).WriteTo(writer);
                    return;
                }

            }

            JToken.FromObject(value).WriteTo(writer);
        }
    }
}
