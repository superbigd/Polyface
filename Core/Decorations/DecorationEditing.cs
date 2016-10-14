using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;

namespace Polyfacing.Core.Decorations
{
    /// <summary>
    /// decorates a decoration with the ability to modify Decorated within the cake including the topmost layer and the root
    /// </summary>
    public class DecorationEditing : DecorationBase<DecorationBase>
    {
        #region Ctor
        public DecorationEditing(DecorationBase decorated)
            : base(decorated)
        {
        }
        public static DecorationEditing New(DecorationBase decorated)
        {
            return new DecorationEditing(decorated);
        }
        #endregion

        #region Properties
        public object Replaced { get; private set; }
        public object Replacement { get; private set; }
        public bool IsTopLevelReplacement { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// walks the decorated chain and runs this function.  if a new object is produced that is replaced 
        /// at that place in the chain.  it will only replace one decorated instance before returning - the 
        /// chain is modified at this point, thus the need for new iteration.  
        /// </summary>
        /// <param name="replaceFn"></param>
        /// <returns></returns>
        public DecorationEditing ReplaceDecorated(Func<object,object> replaceFn)
        {
            var decorated = this.Decorated;

            bool isReplaced = false;
            object parentDec = this;

            Walker.DepthFirstWalk(decorated, (item) =>
            {
                if (isReplaced)
                    return null;

                if (item is IDecorating)
                {
                    return new List<object>() { ((IDecorating)item).Decorated };
                }

                return null;
            }, (item) =>
            {
                if (isReplaced)
                    return;

                var currentDec = item;
                var newDec = replaceFn(currentDec);

                //if nothing's changed move along
                if (Object.ReferenceEquals(currentDec, newDec))
                {
                    parentDec = currentDec;
                    return;
                }

                //we're replacing
                isReplaced = true;

                if (!(parentDec is DecorationBase))
                    return;

                //if we're replacing decorated itself, inform clients with the top level flag, as they may want to update their references
                if (object.ReferenceEquals(parentDec, this))
                    this.IsTopLevelReplacement = true;

                DecorationBase parentDec2 = parentDec as DecorationBase;
                parentDec2.SetDecorated(newDec); 
                this.Replaced = currentDec;
                this.Replacement = newDec;

                //from parentDec and beneath, set the polyface to the parentDec's
                
                IKnowsPolyface ikp = parentDec2.Decorated as IKnowsPolyface;
                while (ikp != null)
                {
                    ikp.Polyface = parentDec2.Polyface;

                    if (!(ikp is IDecorating))
                        ikp = null;
                    else
                        ikp = (ikp as IDecorating).Decorated as IKnowsPolyface;

                }
            });

            return this;
        }
        #endregion
    }

    public static class DecorationEditingExtensions
    {
        #region Declarations
        public static string NAME = "DecorationEditing";
        #endregion

        public static DecorationEditing WithDecorationEditing(this DecorationBase decorated)
        {
            return DecorationEditing.New(decorated);
        }

    }
}
