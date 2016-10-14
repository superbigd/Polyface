using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Services.Serialization
{
    public interface ISerializer : IHasId
    {
        bool CanHandle(object obj);
        string Serialize(object obj);
        object Deserialize(string text, Type type);
    }
}
