using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Extensions;

namespace Polyfacing.Core.Services.Serialization
{
    /*
     * Serialization Overview:

     *  We use a 2 step process wherein we serialize data regularly, and then take those results, put them
     *  in a POCO with additional properties of Type and SerializerId, and serialize that as JSON - this is the data that 
     *  comes out of SerializationManager.Serialize.  Deserialization reverses this process.  
     * 
     *  Design Points:
        SerializationManager is the registry of ISerializers, and the single point of entry into the Serialization Services.
     *  In order for a message to be deserialized, the serializer encoded in the json has to be registered with 
     *  SerializationManager.
     
     */


    /// <summary>
    /// manages the serializing process
    /// </summary>
    public class SerializationManager
    {
        #region Inner Classes
        [Serializable]
        /// <summary>
        /// a poco that contains the results of serialization
        /// </summary>
        internal class SerializationEnvelope
        {
            #region Ctor
            public SerializationEnvelope() { }
            #endregion

            #region Properties
            public string Type { get; set; }
            public string Data { get; set; }
            public string SerializerId { get; set; }
            #endregion

        }
        #endregion

        #region Declarations
        private readonly object _stateLock = new object(); //the explicit object we thread lock on 
        private static SerializationManager _instance = new SerializationManager(); //the singleton instance
        
        private List<ISerializer> _serializers = new List<ISerializer>();
        #endregion

        #region Ctor
        static SerializationManager()
        {
        }
        private SerializationManager()
        {
            this._serializers = new List<ISerializer>();
            this._serializers.Add(MSJSONSerializer.New());
            this._serializers.Add(BinarySerializer.New());
            this._serializers.Add(JilSerializer.New());
            this._serializers.Add(DynamicJilSerializer.New());

        }
        #endregion

        #region Properties
        public static SerializationManager Instance { get { return _instance; } }
        private ISerializer EnvelopeSerializer { get { return this.Get<JilSerializer>(); } }
        #endregion

        #region Registry Methods
        public ISerializer Get<T>() 
            where T: ISerializer
        {
            return this._serializers.Where((x) => { return x is T; }).FirstOrDefault();
        }
        public ISerializer Get(string id)
        {
            return this._serializers.Where((x) => { return x.Id.Equals(id); }).FirstOrDefault();
        }
        #endregion


        #region Methods
        /// <summary>
        /// serializes with Binary Serializer by default
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string DefaultSerialize(object obj)
        {
            return this.Serialize(obj, this.Get<BinarySerializer>());
        }
        public string Serialize(object obj, ISerializer srlzr)
        {
            var rv = this.EnvelopeSerializer.Serialize(this.BuildEnvelope(obj, srlzr));
            return rv;
        }
        public object Deserialize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            SerializationEnvelope env = (SerializationEnvelope) this.EnvelopeSerializer.Deserialize(text, typeof(SerializationEnvelope));
            
            var innerSrlzr = this.Get(env.SerializerId);
            if (innerSrlzr == null)
                throw new InvalidOperationException("serializer not found");

            Type type = env.Type.FindType();
            if (type == null)
                throw new InvalidOperationException("type not found");

            var rv = innerSrlzr.Deserialize(env.Data, type);

            
            return rv;
        }
        #endregion

        #region Helpers

        private SerializationEnvelope BuildEnvelope(object obj, ISerializer srlzr)
        {
            if (srlzr == null)
                throw new ArgumentNullException("srlzr");

            if (!srlzr.CanHandle(obj))
                throw new ArgumentOutOfRangeException("mismatched srlzr");

            SerializationEnvelope rv = new SerializationEnvelope();
            rv.SerializerId = srlzr.Id;
            rv.Type = obj.GetType().FullName;
            rv.Data = srlzr.Serialize(obj);

            return rv;
        }
        #endregion
    }
}
