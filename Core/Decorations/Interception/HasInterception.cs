using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Decorations.Interception
{
    /// <summary>
    /// decorates with interception functionalities:  
    /// -has an interception strategy
    /// -has factory methods to produce proxies according to this strategy
    /// 
    /// </summary>
    public class HasInterception<T> : DecorationBase<T>
    {
        #region Ctor
        private HasInterception(T decorated, InterceptionStrategy logic)
            : base(decorated)
        {
            this.Logic = logic;
        }
        public static HasInterception<T> New(T decorated, InterceptionStrategy logic)
        {
            return new HasInterception<T>(decorated, logic);
        }
        #endregion

        #region Properties
        public InterceptionStrategy Logic { get; set; }
        #endregion

        #region Methods
        public U GetInterceptingProxy<U>(U thingToProxy)
        {
            return thingToProxy.GetInterceptingProxy(this.Logic);
        }
        #endregion

        #region Self Polyface Interception
        public HasInterception<T> InterceptRoot()
        {
            if (this.Polyface == null)
                return this;

            var polyface = this.TypedPolyface;
            var root = this.TypedRoot;

            //replace the root with an intercepting proxy to the root
            polyface.WithHasRootEditing().ReplaceRoot((origRoot) =>
            {
                return this.GetInterceptingProxy(root);
            });

            return this;
        }
        public HasInterception<T> UnInterceptRoot()
        {
            if (this.Polyface == null)
                return this;

            var polyface = this.TypedPolyface;
            var root = this.TypedRoot;

            polyface.WithHasRootEditing().ReplaceRoot((origRoot) =>
            {
                //ignore if not a proxy
                if (!(origRoot is IInterceptingProxy))
                    return origRoot;

                //get decorated, the thing we're going back to
                var decorated = (origRoot as IDecorating).Decorated;
                if (decorated == null)
                    return origRoot;

                return (T)decorated;
            });

            return this;
        }
        /// <summary>
        /// replaces a polyface face with an intercepting proxy to the original face
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HasInterception<T> InterceptFacet(string name)
        {
            if (this.Polyface == null)
                return this;

            //get the original
            var face = this.TypedPolyface.As(name);
            if (face == null)
                return this;

            //proxy it
            var proxy = this.GetInterceptingProxy(face);
            var newFace = proxy as DecorationBase;
            if (newFace == null)
                return this;

            //replace it.  this call will validate that the face is a decoration of the root thing
            this.TypedPolyface.Has(name, newFace);

            return this;
        }
        public HasInterception<T> UnInterceptFacet(string name)
        {
            if (this.Polyface == null)
                return this;

            //get the original
            var face = this.TypedPolyface.As(name);
            if (face == null)
                return this;

            //ignore if not a proxy
            if (!(face is IInterceptingProxy))
                return this;

            //get decorated, the thing we're going back to
            var decorated = (face as IDecorating).Decorated;
            if (decorated == null)
                return this;

            var newFace = decorated as DecorationBase;
            if (newFace == null)
                return this;

            //replace it.  
            //TODO: add decoration to validate that the face is a decoration of the root thing.  or replace decoration
            this.TypedPolyface.Has(name, newFace);

            return this;
        }
        public HasInterception<T> InterceptAllFaces()
        {
            if (this.Polyface == null)
                return this;

            //intercept all but self
            foreach (var each in this.TypedPolyface.Faces)
                if (!object.ReferenceEquals(each.Value, this))
                    this.InterceptFacet(each.Key);

            return this;
        }
        public HasInterception<T> UnInterceptAllFaces()
        {
            if (this.Polyface == null)
                return this;

            //intercept all but self
            foreach (var each in this.TypedPolyface.Faces)
                if (!object.ReferenceEquals(each.Value, this))
                    this.UnInterceptFacet(each.Key);

            return this;
        }
        #endregion
    }

    public static class HasInterceptionExtensions
    {
        #region Declarations
        public static string NAME = "HasInterception";
        #endregion

        public static HasInterception<T> WithHasInterception<T>(this T decorated, InterceptionStrategy logic)
        {
            return HasInterception<T>.New(decorated, logic);
        }
        public static Polyface<T> WithHasInterception<T>(this Polyface<T> polyface, InterceptionStrategy logic)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            if (polyface.As<HasInterception<T>>(NAME) == null)
                polyface.Has(NAME, polyface.Root.WithHasInterception(logic));

            return polyface;
        }
        public static Polyface<T> UndecorateHasInterception<T>(this Polyface<T> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            //unintercept anything that's being intercepted
            var face = polyface.AsHasInterception();
            if (face != null)
                face.UnInterceptAllFaces().UnInterceptRoot();

            polyface.RemoveFace(NAME);
            return polyface;
        }
        public static HasInterception<T> AsHasInterception<T>(this Polyface<T> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            return polyface.As<HasInterception<T>>(NAME);
        }

        /*
         tell pf to do pre and post face adding interception
         
         
         
         */
    }
}
