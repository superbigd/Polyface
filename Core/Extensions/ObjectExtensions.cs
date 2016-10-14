using Jil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static string Serialize(this object obj)
        {
            //JSON.
            using (var output = new StringWriter())
            {
                JSON.Serialize(
                    obj,
                    output
                );

                return output.r
            }
        }
        public static object Deserialize(this string text)
        {

        }

    }
}
