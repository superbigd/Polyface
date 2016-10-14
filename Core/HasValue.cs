using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{

    /// <summary>
    /// most basic polyfaceable thing
    /// </summary>
    public class HasValue : MarshalByRefObject, IKnowsPolyface, IHasValue
    {
        #region Ctor
        public HasValue(object val)
        {
            this.Polyface = Polyface<HasValue>.New(this);

            this.Value = val;
        }
        public static HasValue New(object val)
        {
            return new HasValue(val);
        }
        #endregion

        #region IHasValue
        public object Value { get; set; } 
        #endregion

        #region IKnowsPolyface - Polyface plumbing
        public Polyface<HasValue> TypedPolyface { get { return this.Polyface as Polyface<HasValue>; } }
        public IPolyface Polyface { get; set; }
        #endregion
    }

    /// <summary>
    /// most basic generic polyfaceable thing
    /// </summary>
    public class HasValue<T> : MarshalByRefObject, IKnowsPolyface, IHasValue<T>
    {
        #region Ctor
        public HasValue(T val)
        {
            this.Polyface = Polyface<HasValue<T>>.New(this);
            this.Value = val;
        }
        public static HasValue<T> New(T val)
        {
            return new HasValue<T>(val);
        }
        #endregion

        #region IHasValue
        public T Value { get; set; }
        object IHasValue.Value { get { return this.Value; } set { this.Value = (T)value; } }
        #endregion

        #region IKnowsPolyface - Polyface plumbing
        public Polyface<HasValue<T>> TypedPolyface { get { return this.Polyface as Polyface<HasValue<T>>; } }
        public IPolyface Polyface { get; set; }
        #endregion
    }
}
