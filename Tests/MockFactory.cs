using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Extensions;
using System.Dynamic;

namespace Polyfacing.Tests
{
    public enum MockTypes
    {
        _boolean,
        _byte,
        _sbyte,
        _char,
        _decimal,
        _double,
        _float,
        _int32,
        _uint32,
        _int64,
        _uint64,
        _int16,
        _uint16,
        _string,
        _date,
        _object
    }

    public static class MockFactory
    {
        #region Declarations
        private static Random _rnd = new Random();

        #endregion

        #region Methods
        public static object Random(MockTypes mocktype)
        {
            switch (mocktype)
            {
                case MockTypes._boolean:
                    return _rnd.NextBoolean();

                case MockTypes._byte:
                    return _rnd.NextByte();

                case MockTypes._char:
                    return _rnd.NextChar();

                case MockTypes._date:
                    return _rnd.NextFutureDate(30);

                case MockTypes._decimal:
                    return _rnd.NextDecimal();

                case MockTypes._double:
                    return _rnd.NextDouble();

                case MockTypes._float:
                    return _rnd.NextFloat();

                case MockTypes._int16:
                    return _rnd.NextInt16();

                case MockTypes._int32:
                    return _rnd.NextInt32();

                case MockTypes._int64:
                    return _rnd.NextInt64();

                case MockTypes._sbyte:
                    return _rnd.NextSByte();

                case MockTypes._string:
                    return _rnd.NextString(10);

                case MockTypes._uint16:
                    return _rnd.NextUInt16();

                case MockTypes._uint32:
                    return _rnd.NextUInt32();

                case MockTypes._uint64:
                    return _rnd.NextUInt64();

                case MockTypes._object:
                    var x = new ExpandoObject() as IDictionary<string, Object>;
                    int propCount = _rnd.Next(10);
                    for (int i = 0; i < propCount; i++)
                    {
                        var propName = _rnd.NextString(10, "abcdefghijklmnopqrstuvwxyz");
                        var propVal = Random(_rnd.NextEnum<MockTypes>());
                        x.Add(propName, propVal);

                    }
                    return x;

            }

            return null;
        }

        public static List<object> GetEachRandomMock()
        {
            List<object> rv = new List<object>();



            foreach (MockTypes enumValue in Enum.GetValues(typeof(MockTypes)))
            {
                if (enumValue.Equals(MockTypes._char))
                    continue;

                rv.Add(Random(enumValue));
            }

            return rv;
        }

        /// <summary>
        /// compares two mock data instances to see if they're equivalent
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool CompareMock(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null)
                return false;

            if (obj2 == null)
                return false;

            if (obj1.GetType().Equals(typeof(ExpandoObject)))
            {
                return ExpandoCompare(obj1, obj2);
            }
            else
            {
                var t1 = obj1.ToString();
                var t2 = obj2.ToString();
 
                bool rv = false;
                rv = t1.Equals(t2);

                if (!rv)
                {
                    string s = "Asdfa";
                }
                return rv;
            }

        }

        //https://github.com/jcwrequests/Comparer/blob/master/CSharp.System.Dynamic.Extensions/ExpandoObjectComparer.cs
        private static bool ExpandoCompare(object x, object y)
        {

            if (object.ReferenceEquals(x, y)) return true;

            if (x.GetType().Equals(y.GetType()) && x.GetType().Equals(typeof(ExpandoObject)))
            {
                var xKeyValues = new Dictionary<string, object>(x as IDictionary<string, object>);
                var yKeyValues = new Dictionary<string, object>(y as IDictionary<string, object>);

                var xFieldsCount = xKeyValues.Count();
                var yFieldsCount = yKeyValues.Count();

                if (xFieldsCount != yFieldsCount) return false;
                var missingKey = xKeyValues.Keys.Where(k => !yKeyValues.ContainsKey(k)).FirstOrDefault();
                if (missingKey != null) return false;

                foreach (var keyValue in xKeyValues)
                {

                    var key = keyValue.Key;
                    var xValueItem = keyValue.Value;
                    var yValueItem = yKeyValues[key];

                    if (xValueItem == null & yValueItem != null) return false;
                    if (xValueItem != null & yValueItem == null) return false;

                    if (xValueItem != null & yValueItem != null)
                    {
                        if (!CompareMock(xValueItem, yValueItem)) return false;
                        //if (!xValueItem.Equals(yValueItem)) return false;
                    }
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}
