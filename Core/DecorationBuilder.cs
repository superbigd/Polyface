using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{
    /// <summary>
    /// provides the ability to decide which decorations apply to which things
    /// </summary>
    public class DecorationBuilder<T>
    {
        #region Inner Classes
        public class DecorationCondition
        {
            #region Ctor
            public DecorationCondition(Func<T, bool> buildCondition, Func<T, DecorationBase<T>> buildFn)
            {
                if (buildCondition == null)
                    throw new ArgumentNullException("buildCondition");
                if (buildFn == null)
                    throw new ArgumentNullException("buildFn");
                this.BuildCondition = buildCondition;
                this.BuildFunction = buildFn;
            }
            #endregion

            #region Properties
            public Func<T, bool> BuildCondition { get; private set; }
            public Func<T, DecorationBase<T>> BuildFunction { get; private set; }
            #endregion

            #region Method
            public bool TestAndDo(T thing, out DecorationBase<T> newThing)
            {
                bool rv = false;

                if (!this.BuildCondition(thing))
                {
                    newThing = null;
                    return rv;
                }

                newThing = this.BuildFunction(thing);
                rv = true;

                return rv;
            }
            #endregion
        }
        #endregion

        #region Ctor
        private DecorationBuilder()
        {
            this.Conditions = new List<Core.DecorationBuilder<T>.DecorationCondition>();
        }
        public static DecorationBuilder<T> New()
        {
            return new DecorationBuilder<T>();
        }
        #endregion

        #region Properties
        public List<DecorationCondition>  Conditions { get; private set; }
        #endregion

        #region Methods
        public DecorationBuilder<T> AddCondition(Func<T, bool> buildCondition, Func<T, DecorationBase<T>> buildFn)
        {
            this.Conditions.Add(new DecorationCondition(buildCondition, buildFn));
            return this;
        }
        public DecorationBase<T> Build(T thing)
        {
            DecorationBase<T> rv = null;

            foreach (var each in this.Conditions)
                if (each.TestAndDo(thing, out rv))
                    break;

            return rv;
        }
        #endregion
    }
}
