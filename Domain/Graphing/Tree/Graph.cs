using Polyfacing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Polyfacing.Domain.Graphing.Tree
{
    public class Graph : MarshalByRefObject, IKnowsPolyface
    {
        #region Declarations
        private readonly object _stateLock = new object();
        private int _nextId = 0;
        #endregion

        #region Ctor
        private Graph()
        {
            this.Polyface = Polyface<Graph>.New(this);
            this.Root = null;
            this.Current = this.Root;
        }
        public static Graph New()
        {
            return new Graph();
        }
        #endregion

        #region Properties
        public Node Root { get; private set; }
        public Node Current { get; private set; }
        #endregion

        #region IKnowsPolyface - Polyface plumbing
        public Polyface<Graph> TypedPolyface { get { return this.Polyface as Polyface<Graph>; } }
        public IPolyface Polyface { get; set; }
        #endregion

        #region Factory
        private string GetNextId()
        {
            var rv = Interlocked.Increment(ref this._nextId);
            return rv.ToString();
        }
        /// <summary>
        /// factory method to create a node
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private Node CreateNode(object val)
        {
            //look for an existing node with the same value
            return Node.New(this.GetNextId(), val, this.Current);
        }
        private bool InitRoot(object val)
        {
            lock (this._stateLock)
            {
                if (this.Root != null)
                    return false;

                this.Root = this.CreateNode(val);
                this.Current = this.Root;
                return true;
            }
        }
        #endregion

        #region Mutators
        public Graph AddChild(object val)
        {
            if (this.InitRoot(val))
                return this;

            lock (this._stateLock)
            {
                this.Current = this.Current.AppendChild(this.GetNextId(), val);
            }
            return this;
        }
        public Graph RemoveCurrent()
        {
            //if a next sibling exists set current to that
            //else if a previous sibling exists set current to that
            //if if a parent exists, set current to that

            var next = this.Current.GetNextSibling();
            if (next != null)
            {
                this.Current.Parent.RemoveChild(this.Current.Id);
                this.Current = next;
                return this;
            }

            var previous = this.Current.GetPreviousSibling();
            if (previous != null)
            {
                this.Current.Parent.RemoveChild(this.Current.Id);
                this.Current = previous;
                return this;
            }

            var parent = this.Current.Parent;
            if (parent != null)
            {
                this.Current.Parent.RemoveChild(this.Current.Id);
                this.Current = parent;
                return this;
            }

            return this;
        }
        public Graph AddNext(object val)
        {
            if (this.InitRoot(val))
                return this;

            lock (this._stateLock)
            {
                this.Current = this.Current.Parent.AppendChild(this.GetNextId(), val);
            }
            return this;
        }
        public Graph AddPrevious(object val)
        {
            if (this.InitRoot(val))
                return this;

            lock (this._stateLock)
            {
                this.Current = this.Current.Parent.PrependChild(this.GetNextId(), val);
            }
            return this;
        }
        #endregion

        #region Navigation
        public Graph MoveToRoot()
        {
            this.Current = this.Root;
            return this;
        }
        public Graph MoveUp()
        {
            if (this.Current.Parent != null)
                this.Current = this.Current.Parent;
            return this;
        }
        public Graph MoveDown(int i = 0)
        {
            if (this.Current.Children.Count == 0)
                return this;

            if (this.Current.Children.Count < i + 1)
                return this;

            this.Current = this.Current.Children[i];

            return this;
        }
        public Graph MoveNext()
        {
            var next = this.Current.GetNextSibling();
            if (next != null)
                this.Current = next;

            return this;
        }
        public Graph MovePrevious()
        {
            var previous = this.Current.GetPreviousSibling();
            if (previous != null)
                this.Current = previous;

            return this;
        }

        //public Graph Walk(Action<Node> walkAction)
        //{
        //    //starting from the root, walk the whole graph
        //    Walker.BreadthFirstWalk(this.Root, (x) =>
        //    {
        //        Node node = x as Node;

        //        //get prev/next/children/parents


        //    }, (x) =>
        //    {

        //    });
        //    return this;
        //}
        #endregion
    }
}
