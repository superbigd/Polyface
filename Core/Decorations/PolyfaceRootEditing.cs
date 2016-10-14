using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;

namespace Polyfacing.Core.Decorations
{
    /// <summary>
    /// decorates a polyface with some root functionality
    /// </summary>
    public class PolyfaceRootEditing<T> : DecorationBase<Polyface<T>>
    {
        #region Ctor
        public PolyfaceRootEditing(Polyface<T> decorated) : base(decorated)
        {
        }

        public static PolyfaceRootEditing<T> New(Polyface<T> decorated)
        {
            return new PolyfaceRootEditing<T>(decorated);
        }
        #endregion

        #region Methods
        public PolyfaceRootEditing<T> ReplaceRoot(Func<T,T> getRootFn)
        {
            var decorated = this.Decorated;
            var oldRoot = decorated.Root;
            var newRoot = getRootFn(oldRoot);

            decorated.SetRoot(newRoot);
            decorated.RefreshPolyfaceReferences();

            //decorated.GetType().GetField("_root", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            //    .SetValue(decorated, newRoot);

            var faces = decorated.Faces;
            foreach (var each in faces)
                if (each.Value is DecorationBase)
                {
                    var editor = (each.Value as DecorationBase).WithDecorationEditing();
                    
                    //walk the decoration chain down to the root which should be the oldRoot instance.  replace it
                    editor.ReplaceDecorated((layer) =>
                    {
                        if (!object.ReferenceEquals(oldRoot, layer))
                            return layer;

                        return newRoot;
                    });

                    //each.Value.GetType().GetProperty("Decorated", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).SetValue(each.Value, newRoot);

                }
                    
            return this;
        }
        #endregion
    }

    public static class HasRootEditingExtensions
    {
        #region Declarations
        public static string NAME = "HasRootEditing";
        #endregion

        public static PolyfaceRootEditing<T> WithHasRootEditing<T>(this Polyface<T> decorated)
        {
            return PolyfaceRootEditing<T>.New(decorated);
        }

    }
}
