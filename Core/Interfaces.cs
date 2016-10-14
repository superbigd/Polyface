using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{

    public interface IHasName
    {
        string Name { get; }
    }

    public interface IHasId
    {
        string Id { get; }
    }

    public interface IHasValue
    {
        object Value { get; set; }
    }

    public interface IHasValue<T> : IHasValue
    {
        new T Value { get; set; }
    }

    public interface IDecorating
    {
        object Decorated { get; }
    }

    public interface IDecorating<T> : IDecorating
    {
        new T Decorated { get; }
    }

    public interface IPolyface
    {
        Type OfType { get; }
        object GetRoot();
        List<IDecorating> GetFaces();

        IDecorating As(string name);
        U As<U>(string name) where U : IDecorating;
        IPolyface Has(string name, IDecorating facet);

        IPolyface RemoveFace(string name);
    }

    public interface IPolyface<T>
    {
        T Root { get; }
    }

    public interface IKnowsPolyface
    {
        IPolyface Polyface { get; set; }
    }



}
