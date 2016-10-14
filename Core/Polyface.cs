using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core
{
    /*
     * Theory of Polyface
     * 
     * It's a design approach that structurally breaks an object into a root thing, and named facets linked to that 
     * root thing.    
     * 
     * To understand Polyface we look at another structure, the linked list. 
     * In the simplest linked list-y structure, each node knows the next node.  Thus the first node in the list is required
     * to traverse the whole list.  If you wanted every node to be able to traverse the list you'd decorate it with FirstNode ness
     * or a containing List structure itself that had a FirstNode property.  The design constraint here is: basic traversal of a
     * graph from anywhere within the graph requires knowledge of the starting point.  
     * 
     * The idea of the polyface is that it extends this traversal principle by adding the idea of an ending point, or an outwardsness,
     * or a Thing of Things ness.  A polyface is a container of faces/aspects/facets decorating a core thing, such that the
     * core thing has knowledge of the polyface, and each face does too.  Traversal is accomplished anywhere in the structure 
     * as each node has knowledge of the start and the end.  As well, each facet has a named lookup from the polyface, and this is 
     * where we have a view of the "whole entity".
     * 
     * Polyfaces on their own aren't the whole story.  The whole story requires a root type that implements IKnowsPolyface.
     * An example of this is the Value<> type.  When a type implements IKnowsPolyface you enable the entity to know its "whole self".
     * It becomes "wholly".  Thus you can extend the entity using the decorating extensions, and this will update the polyface,
     * and thus the "whole self".  The magic of this is that we can do this fluently, in a sentence like structure, and mutate
     * the entity, but because we're mutating the referenced polyface, we don't run into problems wherein each fluent call
     * changes the output type.  No, we keep the same Whole-y output type, and thus we "stay fluent".  
     * 
     * 
     * In the natural progression of modelling.  
     *  Thing -> 
     *          Decoration / LinkedList ->
     *                  Thing of Things -> 
     *                      DigitSets (full set traversal to find a digit)
     *                          -> Circular Digit Sets (traversal rolls over) -> 
     *                              -> Numbers (LinkedList of Circular DigitSets, with some navigable/counting)
     *          .. Numbers -> Encoded Symbol Sets (ie. number <-> symbol) 
     *                          -> Strings (ie. words)
     *                              -> Dictionaries (ie. word <-> value)
     *                                  -> Classes / Various object modelling paradigms that use Labels as a core idea
     *                                  -> Polyface.  An object modelling paradigm that provides lookup of faces (ie. properties)
     *                                      but done in a decorative (ie. sequenced, traceable, reversable, extensible) way
     *          .. Polyface -> Graph
     *                          HasChild, HasNext, HasParent, HasCurrent
     * 
     * A polyface is a way of having a more extensible modelling paradigm through the use of the decorator.  And a more 
     * fluent paradigm as the polyface's navigable structure enable either IKnowsPolyface or IPolyface to resolve to
     * the facets/behaviours.  Thus, if we make every decoration have fluency, we can have a sentence-like way of 
     * programming.  Each step opens up other steps to make.  Fluency enables auditability and data integrity.  
     * 
     *  Thus the typical usage will locate the polyface first, and then the facet next.
     *  
     * Examples:
     *  ??somewhere on the pf??.ResolvePolyface().AsX
     *  root.TypedPolyface.AsX
     *  pf.AsX
     *  
     * Next data structure : sequenced faces, so that the construction order is maintained.  or perhaps do this as a decoration
     * 
     */

    /// <summary>
    /// a container of named decorations of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 
    [DebuggerDisplay("{GetHashCode()}")]
    public class Polyface<T> : MarshalByRefObject, IPolyface, IPolyface<T>
    {
        #region Declarations
        private Dictionary<string, IDecorating> _faces = new Dictionary<string, IDecorating>(); 
        private T _root;
        #endregion

        #region Ctor
        private Polyface(T root)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            this._root = root;
        }
        public static Polyface<T> New(T root)
        {
            return new Polyface<T>(root);
        }
        #endregion

        #region IPolyface
        public Type OfType { get { return typeof(T); } }
        public object GetRoot() { return this.Root; }
        public List<IDecorating> GetFaces() { return new List<IDecorating>(this.Faces.Values); }
       
        public IPolyface RemoveFace(string name)
        {
            this.Faces.Remove(name);
            return this;
        }
        #endregion

        #region Properties
        public T Root { get { return this._root; } }
        public Dictionary<string, IDecorating> Faces { get { return this._faces; } }
        #endregion

        #region Internal Mutators
        internal void SetRoot(T root)
        {
            this._root = root;
        }
        #endregion

        #region Queries
        public IDecorating As(string name)
        {
            IDecorating item = null;
            if (this._faces.TryGetValue(name, out item))
            {
                return item;
            }
            return item;
        }
        public U As<U>(string name) where U : IDecorating
        {
            var utype = typeof(U);

            IDecorating item = null;
            if (this._faces.TryGetValue(name, out item))
            {
                U rv = (U)item;
                return rv;
            }
            return default(U);
        }

        #endregion

        #region Fluent 

        public IPolyface Has(string name, IDecorating facet)
        {
            if (facet == null)
                throw new ArgumentNullException("facet");

            var core = facet.GetLastNonNullDecorated();

            if(!RemotingServices.IsTransparentProxy(this.Root))
                if(!RemotingServices.IsTransparentProxy(core))
                    if (!object.ReferenceEquals(this.Root, core))
                        throw new ArgumentOutOfRangeException("invalid decoration.  must decorate root");

            if (facet is IKnowsPolyface)
                (facet as IKnowsPolyface).Polyface = this;
            
            this._faces[name] = facet;

            return this;
        }

        public Polyface<T> Remove(string name)
        {
            this._faces.Remove(name);
            return this;
        }

        public Polyface<T> DoAs(string name, Action<object> action)
        {
            object thing = this.As(name);
            action(thing);
            return this;
        }
        public Polyface<T> DoAs<U>(string name, Action<U> action) where U : DecorationBase
        {
            U thing = this.As<U>(name);
            action(thing);
            return this;
        }
        public Polyface<T> Do(Action<Polyface<T>> action)
        {
            action(this);
            return this;
        }
        #endregion


        #region Navigation
        /// <summary>
        /// walks the polyface performing the strategy.  
        /// </summary>
        /// <param name="strategy"></param>
        public void Walk( /*key, core/face*/ Action<string, object> strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException("strategy");

            strategy(null, this.Root);
            foreach(var each in this._faces)
            {
                strategy(each.Key, each.Value);
            }
        }
        #endregion



    }

    public static class PolyfaceExtensions
    {
        /// <summary>
        /// wires polyface's polyfaceness throughout the polyface
        /// </summary>
        /// <param name="pf"></param>
        /// <returns></returns>
        internal static IPolyface RefreshPolyfaceReferences(this IPolyface pf)
        {
            pf.GetFaces().ForEach((x) => 
            {
                if (x is IKnowsPolyface)
                    (x as IKnowsPolyface).Polyface = pf;
            });
            
            var root = pf.GetRoot();
            if (root is IKnowsPolyface)
                (root as IKnowsPolyface).Polyface = pf;

            return pf;
        }

        /// <summary>
        /// if a polyface returns self..if knowspolyface returns polyface
        /// </summary>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static IPolyface ResolvePolyface(this object thing)
        {
            if (thing == null)
                throw new ArgumentNullException("thing");

            if (thing is IPolyface)
                return thing as IPolyface;

            if (thing is IKnowsPolyface)
                return (thing as IKnowsPolyface).Polyface;

            return null;
        }

        public static bool IsPolyfaceRoot(this object thing)
        {
            if (thing != null &&
                (thing is IKnowsPolyface) &&
                ((thing as IKnowsPolyface).Polyface != null) &&
                (Object.ReferenceEquals(thing, (thing as IKnowsPolyface).Polyface.GetRoot())))
                    return true;

            return false;
        }

        
    }

}
