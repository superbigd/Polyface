using Polyfacing.Core;
using Polyfacing.Domain.CallNarration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Domain.Graphing.Tree;

namespace Polyfacing.Tests
{
    public static class NarrationTests
    {
        //define a class who has nested method calls
        public class Tester : ThingBase
        {
            public Tester() :base()
            {
                this.Polyface = Polyface<Tester>.New(this);
            }
            public string ValA { get; set; }
            public string ValANotice { get; set; }

            public Tester SetA(string A)
            {
                this.ValA = A;

                this.SetANotice("ValA set to " + A);

                return this;
            }
            public Tester SetANotice(string ANotice)
            {
                this.ValANotice = ANotice;
                return this;
            }

            public Polyface<Tester> TypedPolyface { get { return this.Polyface as Polyface<Tester>; } }
        }

        public static void Test()
        {
            //create the thing we want to intercept as a narrative, decorated it with interception, and test
            var tester = new Tester();


            var narrator = tester.TypedPolyface.WithCallNarration().AsCallNarration();
            narrator.InterceptRoot();

            narrator.TypedRoot.SetA("xxx");

            var xml = narrator.Narration.GetXMLReport();
            

        }
    }
}
