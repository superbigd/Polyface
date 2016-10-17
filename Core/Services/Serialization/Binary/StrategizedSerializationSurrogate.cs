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

    public class StrategizedSerializationSurrogate : ISerializationSurrogate
    {
        #region Declarations
        private const BindingFlags publicOrNonPublicInstanceFields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        #endregion

        #region Ctor
        public StrategizedSerializationSurrogate(Func<string, Type, Type, bool> fieldFilterStrategy)
        {
            this.FieldFilterStrategy = fieldFilterStrategy;
        }
        public static StrategizedSerializationSurrogate New(Func<string, Type, Type, bool> fieldFilterStrategy)
        {
            return new StrategizedSerializationSurrogate(fieldFilterStrategy);
        }
        #endregion

        #region Properties
        public Func<string, Type, Type, bool> FieldFilterStrategy { get; set; }

        #endregion

        #region Implementation
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var type = obj.GetType();

            var fields = ReflectionUtil.GetFieldInfosIncludingBaseClasses(type, publicOrNonPublicInstanceFields);

            foreach (var field in fields)
            {
                //if the field has already been serialized, we skip
                if (this.ContainsEntry(info, field.Name))
                    continue;

                if (this.CanFilterField(field.Name, field.FieldType, type))
                    continue;

                var fieldValue = field.GetValue(obj);
                this.SerializeField(field.Name, fieldValue, info, context);
            }
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var type = obj.GetType();
            var fields = ReflectionUtil.GetFieldInfosIncludingBaseClasses(type, publicOrNonPublicInstanceFields);

            foreach (var field in fields)
            {
                if (this.CanFilterField(field.Name, field.FieldType, type))
                    continue;

                this.DeserializeField(obj, field, field.Name, info, context, selector);
            }

            return obj;
        }
        #endregion

        #region Helpers
        private bool CanFilterField(string fieldName, Type fieldType, Type parentType)
        {
            //don't filter if we have no filter
            if (this.FieldFilterStrategy == null)
                return false;

            var rv = this.FieldFilterStrategy(fieldName, fieldType, parentType);
            return rv;
        }
        private bool ContainsEntry(SerializationInfo info, string name)
        {
            bool returnValue = false;
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == name)
                {
                    returnValue = true;
                    break;
                }
            }
            return returnValue;
        }
        private void SerializeField(string fieldName, object fieldValue, SerializationInfo info, StreamingContext context)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");

            //if the field has already been serialized, we skip
            if (this.ContainsEntry(info, fieldName))
                return;

            if (fieldValue == null)
            {
                info.AddValue(fieldName + "ValueIsNotNull", false);
                info.AddValue(fieldName, fieldValue);
            }
            else
            {
                info.AddValue(fieldName + "ValueIsNotNull", true);
                info.AddValue(fieldName, fieldValue);
                var fieldValueRuntimeType = fieldValue.GetType();
                info.AddValue(fieldName + "RuntimeType", fieldValueRuntimeType.AssemblyQualifiedName);
            }
        }
        private void DeserializeField(object obj, FieldInfo fi, string fieldName, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            if (obj == null)
                return;

            if (fi == null)
                throw new ArgumentNullException("fi");

            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");

            var fieldValueIsNotNull = info.GetBoolean(fieldName + "ValueIsNotNull");
            if (fieldValueIsNotNull)
            {
                var fieldValueRuntimeType = info.GetString(fieldName + "RuntimeType");
                //steps back into the serializer
                var val = info.GetValue(fieldName, Type.GetType(fieldValueRuntimeType));
                
                fi.SetValue(obj, val);
            }
        }
        #endregion
    }


}
