using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{
    /// <summary>
    /// the base class smell we're using for things that are polyface-compatible or Wholly
    /// </summary>
    public abstract class ThingBase : MarshalByRefObject, IKnowsPolyface
    {
        #region Ctor
        protected ThingBase()
        {
        }
        #endregion

        #region IKnowsPolyface - Polyface plumbing
        public IPolyface Polyface { get; set; }
        #endregion
    }
}
