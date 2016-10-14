using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{
    public abstract class DecorationBase : MarshalByRefObject, IDecorating, IKnowsPolyface
    {
        #region Ctor
        public DecorationBase(object decorated)
        {
            if (decorated == null)
                throw new ArgumentNullException("decorated");

            this.Decorated = decorated;

            //when decorating, the created decoration inherits the polyface of the decorated thing.  ie. "wholeness" is associative.
            //this is a constructor paradigm.  when creating a new decoration, one is not always calling from the polyface, 
            //and we want to ensure that, as a default, a thing decorating another thing is just adding to a given Whole.
            if (decorated is IKnowsPolyface)
            {
                //wire the old polyface faces to the decorated polyface
                (this as IKnowsPolyface).Polyface = (decorated as IKnowsPolyface).Polyface;
            }
        }
        #endregion

        #region IDecorating
        public object Decorated { get; private set; }
        internal void SetDecorated(object decorated)
        {
            if (decorated == null)
                throw new ArgumentNullException("decorated");

            this.Decorated = decorated;

        }
        #endregion

        #region IKnowsPolyface
        private IPolyface _pf = null;
        public IPolyface Polyface
        {
            get { return _pf; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                this._pf = value;
            }
        }
        #endregion
    }

    public abstract class DecorationBase<T> : DecorationBase, IDecorating<T>
    {
        #region Ctor
        public DecorationBase(T decorated)
            : base(decorated)
        {
        }
        #endregion

        #region IDecorating
        public new T Decorated { get { return (T)base.Decorated; } }

        object IDecorating.Decorated { get { return base.Decorated; } }
        #endregion

        #region IKnowsPolyface
        /// <summary>
        /// Note: this will only resolve correctly if the polyface is exactly Polyface of T.  
        /// </summary>
        public Polyface<T> TypedPolyface { get { return base.Polyface as Polyface<T>; } }

        /// <summary>
        /// Note: this will only resolve correctly if the polyface is Polyface of T.  
        /// </summary>
        public T TypedRoot { get { return this.TypedPolyface.Root; } }
        #endregion
    }
}
