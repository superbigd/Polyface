using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Decorations.Graphing.Tree
{
    /// <summary>
    /// is a node in a graph.  standalone thing  
    /// </summary>
    public class Node : MarshalByRefObject, IKnowsPolyface, IHasValue, IHasId
    {
        #region Ctor
        private Node(string id, object val, Node parent = null)
        {
            this.Polyface = Polyface<Node>.New(this);
            this.Id = id;
            this.Value = val;
            this.Parent = parent;
            this.Children = new List<Node>();
        }
        public static Node New(string id, object val, Node parent = null)
        {
            return new Node(id, val, parent);
        }
        #endregion

        #region Properties
        public Node Parent { get; private set; }
        public List<Node> Children { get; private set; }
        #endregion

        #region IHasId
        public string Id { get; private set; }
        #endregion

        #region IHasValue
        public object Value { get; set; }
        #endregion

        #region IKnowsPolyface - Polyface plumbing
        public Polyface<Node> TypedPolyface { get { return this.Polyface as Polyface<Node>; } }
        public IPolyface Polyface { get; set; }
        #endregion

        #region Mutation
        /// <summary>
        /// returns new child
        /// </summary>
        /// <param name="id"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public Node AppendChild(string id, object val)
        {
            Node rv = Node.New(id, val, this);
            this.Children.Add(rv);
            return rv;
        }
        public Node PrependChild(string id, object val)
        {
            Node rv = Node.New(id, val, this);
            this.Children.Insert(0, rv);
            return rv;
        }
        /// <summary>
        /// fluent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Node RemoveChild(string id)
        {
            var childNode = this.Children.Where((x) => { return x.Id.Equals(id); }).SingleOrDefault();
            if (childNode != null)
                this.Children.Remove(childNode);

            return this;
        }
        #endregion

        #region Navigation
        public Node GetChild(string id)
        {
            var childNode = this.Children.Where((x) => { return x.Id.Equals(id); }).SingleOrDefault();
            return childNode;
        }
        public Node GetNextSibling()
        {
            if (this.Parent == null)
                return null;

            var idx = this.Parent.Children.IndexOf(this);
            if (!(this.Parent.Children.Count > idx + 1))
                return null;

            return this.Parent.Children[idx + 1];
        }
        public Node GetPreviousSibling()
        {
            if (this.Parent == null)
                return null;

            var idx = this.Parent.Children.IndexOf(this);
            if (idx - 1 < 0)
                return null;

            return this.Parent.Children[idx - 1];
        }

        #endregion

    }
}
