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
    /// serializes types not marked serializable
    /// </summary>
    public class UnattributedTypeSerializationSurrogate : ISerializationSurrogate
    {
        private const BindingFlags publicOrNonPublicInstanceFields = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private bool ContainsEntry(SerializationInfo info, string name)
        {
            bool returnValue = false;
            foreach(SerializationEntry entry in info) 
            {
                if (entry.Name == name)
                {
                    returnValue = true;
                    break;
                }
            }
            return returnValue;
        }
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var type = obj.GetType();
            
            var fields = ReflectionUtil.GetFieldInfosIncludingBaseClasses(type, publicOrNonPublicInstanceFields);
            //var fields =  type.GetFields(publicOrNonPublicInstanceFields);
            
            foreach (var field in fields)
            {
                //if the field has already been serialized, we skip
                if(this.ContainsEntry(info, field.Name))
                    continue;

                var fieldValue = field.GetValue(obj);
                var fieldValueIsNotNull = fieldValue != null;
                
                if (fieldValueIsNotNull)
                {
                    var fieldValueRuntimeType = fieldValue.GetType();
                    info.AddValue(field.Name + "RuntimeType", fieldValueRuntimeType.AssemblyQualifiedName);
                }

                info.AddValue(field.Name + "ValueIsNotNull", fieldValueIsNotNull);
                info.AddValue(field.Name, fieldValue);
            }
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var type = obj.GetType();
            var fields = ReflectionUtil.GetFieldInfosIncludingBaseClasses(type, publicOrNonPublicInstanceFields);
            //var fields =  type.GetFields(publicOrNonPublicInstanceFields);

            foreach (var field in fields)
            {
                var fieldValueIsNotNull = info.GetBoolean(field.Name + "ValueIsNotNull");
                if (fieldValueIsNotNull)
                {
                    var fieldValueRuntimeType = info.GetString(field.Name + "RuntimeType");
                    field.SetValue(obj, info.GetValue(field.Name, Type.GetType(fieldValueRuntimeType)));
                }
            }

            return obj;
        }
    }
}
