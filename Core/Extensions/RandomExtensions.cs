using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Extensions
{
    public static class RandomExtensions
    {
        public static bool NextBoolean(this Random rng)
        {
            var rv = rng.NextDouble() > 0.5;
            return rv;
        }
        /*
         * http://stackoverflow.com/questions/609501/generating-a-random-decimal-in-c-sharp
        */

        public static int NextInt32(this Random rng)
        {
            unchecked
            {
                int firstBits = rng.Next(0, 1 << 4) << 28;
                int lastBits = rng.Next(0, 1 << 28);
                return firstBits | lastBits;
            }
        }
        public static decimal NextDecimal(this Random rng)
        {
            byte scale = (byte)rng.Next(29);
            bool sign = rng.Next(2) == 1;
            return new decimal(rng.NextInt32(),
                               rng.NextInt32(),
                               rng.NextInt32(),
                               sign,
                               scale);
        }
        public static float NextFloat(this Random rng)
        {
            // Perform arithmetic in double type to avoid overflowing
            double range = (double)float.MaxValue - (double)float.MinValue;
            double sample = rng.NextDouble();
            double scaled = (sample * range) + float.MinValue;
            float f = (float)scaled;

            return f;
        }
        /*
         * http://stackoverflow.com/questions/677373/generate-random-values-in-c-sharp
         */
        public static Int64 NextInt64(this Random rnd)
        {
            var buffer = new byte[sizeof(Int64)];
            rnd.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }
        public static UInt64 NextUInt64(this Random rnd)
        {
            var buffer = new byte[sizeof(UInt64)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
        public static Int16 NextInt16(this Random rnd)
        {
            var buffer = new byte[sizeof(Int16)];
            rnd.NextBytes(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }
        public static UInt16 NextUInt16(this Random rnd)
        {
            var buffer = new byte[sizeof(UInt16)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt16(buffer, 0);
        }
        public static UInt32 NextUInt32(this Random rnd)
        {
            var buffer = new byte[sizeof(UInt32)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }
        public static string NextString(this Random rnd, int length, string validChars = "abcdefghijklmnopqrstuvwxyz0123456789")
        {
            StringBuilder sb = new StringBuilder();

            char c;
            for (int i = 0; i < length; i++)
            {
                c = validChars[rnd.Next(0, validChars.Length)];
                sb.Append(c);
            }

            return sb.ToString();
        }
        public static byte NextByte(this Random rnd)
        {
            byte rv = (byte)rnd.Next(0, 255);
            return rv;
        }
        public static sbyte NextSByte(this Random rnd)
        {
            sbyte rv = (sbyte)rnd.Next(-128, 127);
            return rv;
        }
 
        public static char NextChar(this Random rnd)
        {
            var validChars = "abcdefghijklmnopqrstuvwxyz0123456789";
            char rv = validChars[rnd.Next(0, validChars.Length)];
            return rv;
        }
        public static DateTime NextDate(this Random rnd, DateTime from, DateTime to)
        {
            var range = to - from;
            var randTimeSpan = new TimeSpan((long)(rnd.NextDouble() * range.Ticks));
            var rv = from + randTimeSpan;
            return rv;
        }
        public static DateTime NextFutureDate(this Random rnd, int maxDaysAhead)
        {
            return NextDate(rnd, DateTime.Now, DateTime.Now.AddDays(maxDaysAhead));
        }

        public static T NextEnum<T>(this Random rnd)
where T : struct
        {
            Type type = typeof(T);
            if (type.IsEnum == false) throw new InvalidOperationException();

            var array = Enum.GetValues(type);
            var index = rnd.Next(array.GetLowerBound(0), array.GetUpperBound(0) + 1);
            return (T)array.GetValue(index);
        }
    }
}
