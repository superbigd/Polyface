using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Services.Serialization.Binary
{
    public static class Strategies
    {
        public static StrategizedSurrogateSelector GetBestSelector()
        {
            var rv = StrategizedSurrogateSelector.New((type) =>
            {
                //if it's a polyface, use Polyfacesurrogate
                if (typeof(Polyface).IsAssignableFrom(type))
                {
                    return PolyfaceSerializationSurrogate.New();
                }
                else if (!type.IsSerializable)
                {
                    if (typeof(IKnowsPolyface).IsAssignableFrom(type))
                    {
                        return StrategizedSerializationSurrogate.New((name, fieldType, parentType) =>
                        {
                            if (name.Equals("Polyface"))
                                return true;

                            return false;
                        });
                    }

                    return new UnattributedTypeSerializationSurrogate();
                }
                return null;
            });

            return rv;
        }
    }
}
