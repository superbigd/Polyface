using Polyfacing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations;
using Polyfacing.Core.Decorations.Stringableness;

namespace Polyfacing.Tests
{
    public static class DecorationTests
    {
        public static void Test()
        {
            //test decoration editing
            var root = Stringable.New(1);
            var dec = root.TypedPolyface.WithLength().AsLength();

            var edit = dec.WithDecorationEditing().ReplaceDecorated((x) =>
            {
                if (x is Stringable)
                    return Stringable.New("a");

                return x;
            });

            if (edit.IsTopLevelReplacement)
                throw new InvalidOperationException();

            if (!dec.Decorated.Value.Equals("a"))
                throw new InvalidOperationException();

            //test polyface root editing

            var edited =  Stringable.New(1).TypedPolyface.WithLength().WithHasRootEditing().ReplaceRoot((x)=>{
                return Stringable.New("a");
            });

            if (!edited.Decorated.Root.Value.Equals("a"))
                throw new InvalidOperationException();
            
        }
    }
}
