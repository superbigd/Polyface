using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{
    /// <summary>
    /// pluralizes something.  is a New Thing and not a decoration.
    /// </summary>
    public class Many<T> : MarshalByRefObject, IKnowsPolyface
    {
        #region Ctor
        private Many(T thing) 
        {
            this.Polyface = Polyface<Many<T>>.New(this);
            
            this.Items = new List<T>();
            this.Items.Add(thing);
        }
        public static Many<T> New(T thing)
        {
            var rv = new Many<T>(thing);

            return rv;
        }
        #endregion

        #region Properties
        public List<T> Items { get; private set; }
        #endregion

        #region Methods
        public Many<T> Add(T item)
        {
            this.Items.Add(item);
            return this;
        }
        public Many<T> Remove(T item)
        {
            this.Items.Remove(item);
            return this;
        }
        #endregion

        #region IKnowsPolyface
        public IPolyface Polyface { get; set; }
        public Polyface<Many<T>> TypedPolyface { get { return this.Polyface as Polyface<Many<T>>; } }
        #endregion
    }

    public static class ManyExtensions
    {
        public static Many<T> ToMany<T>(this T thing)
        {
            var rv = Many<T>.New(thing);
            return rv;
        }
    }
}
