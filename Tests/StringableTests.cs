using Polyfacing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Stringableness;
using System.Diagnostics;

namespace Polyfacing.Tests
{
    public static class StringableTests
    {
        public static void Test()
        {
            //verify the double/decimal/float formatting works
            int maxIterations = 1000;
            for (int i = 0; i < maxIterations; i++)
            {
                var mock = MockFactory.Random(MockTypes._decimal);
                StringableTest(mock);
            }
            for (int i = 0; i < maxIterations; i++)
            {
                var mock = MockFactory.Random(MockTypes._double);
                StringableTest(mock);
            }
            for (int i = 0; i < maxIterations; i++)
            {
                var mock = MockFactory.Random(MockTypes._float);
                StringableTest(mock);
            }
            //now the dynamic object
            for (int i = 0; i < maxIterations; i++)
            {
                var mock = MockFactory.Random(MockTypes._object);
                StringableTest(mock);
            }

            var all = MockFactory.GetEachRandomMock();

            foreach (var each in all)
            {
                StringableTest(each);
                StringableLengthTest(each);
            }
        }

        private static void StringableTest(object obj)
        {
            var s = Stringable.New(obj);
            var text = s.GetText();
            var s2 = Stringable.Parse(text);
            var text2 = s2.GetText();

            if (!MockFactory.CompareMock(s.Value, s2.Value))
            {
                throw new InvalidOperationException();
            }
        }

        private static void StringableLengthTest(object obj)
        {
            var s = Stringable.New(obj).WithLength();
            var text = s.GetText();
            Debug.WriteLine("serialized to " + text);
            var s2 = StringableLength.Parse(text);
            var text2 = s2.GetText();
            Debug.WriteLine("roundtrip to " + text2);

            if (!MockFactory.CompareMock(s.Value, s2.Value))
            {
                throw new InvalidOperationException();
            }
        }

        //as we get more stringable decorations add tests for them here
    }
}
