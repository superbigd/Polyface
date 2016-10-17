using Polyfacing.Core.Services.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Domain.CallNarration
{
    /// <summary>
    /// represents both call target and return value.  
    /// </summary>
    public class ProxyOperand
    {
        #region Ctor
        private ProxyOperand(int id, object value)
        {
            if (!ProxyOperand.IsCompatible(value))
                throw new ArgumentOutOfRangeException("value");

            this.Id = id;

            //if we're dealing with a polyface only serialize the core

            //serialize the initial state to a string
            //this.InitialState = SerializationManager.Instance.DefaultSerialize(value);
        }
        public static ProxyOperand New(int id, object value)
        {
            return new ProxyOperand(id, value);
        }
        #endregion

        #region Properties
        public int Id { get; private set; }
        public string InitialState { get; private set; }
        #endregion

        #region Static Methods
        static internal bool IsCompatible(object value)
        {
            if (value == null)
                return false;

            Type type = value.GetType();
            if (type.IsValueType)
                return false;

            if (type.Equals(typeof(DateTime)))
                return false;

            return true;
        }
        #endregion
    }
}
