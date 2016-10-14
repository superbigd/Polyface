using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;
using System.Runtime.Remoting.Messaging;
using Polyfacing.Core;

namespace Polyfacing.Domain.Graphing.Tree
{

    public class GraphCompare : DecorationBase<Graph>
    {

        #region Ctor
        public GraphCompare(Graph decorated)
            : base(decorated)
        {
        }

        public static GraphCompare New(Graph decorated)
        {
            return new GraphCompare(decorated);
        }
        #endregion

        #region Methods
        public bool IsEquivalent(Graph graph, Func<Node, Node, bool> nodeCompareFn = null)
        {
            bool rv = true;

            graph.MoveToRoot();
            this.Decorated.MoveToRoot();

            Node nodeLeft = this.Decorated.Root;
            Node nodeRight = graph.Root;

            var iterator = new Tuple<Node, Node>(nodeLeft, nodeRight);

            Walker.BreadthFirstWalk(iterator, (x) =>
            {
                if (!rv)
                    return null;

                List<Tuple<Node, Node>> next = new List<Tuple<Node, Node>>();

                Tuple<Node, Node> leftright = x as Tuple<Node, Node>;

                var children1 = leftright.Item1.Children;
                var children2 = leftright.Item2.Children;

                if (children1.Count != children2.Count)
                {
                    rv = false;
                    return null;
                }

                for (int i = 0; i < children1.Count; i++)
                    next.Add(new Tuple<Node, Node>(children1[i], children2[i]));

                List<object> nextList = new List<object>(next);

                return nextList;
            }, (x) =>
            {
                Tuple<Node, Node> leftright = x as Tuple<Node, Node>;
                if (!AreEquivalentNodes(leftright.Item1, leftright.Item2, nodeCompareFn))
                {
                    rv = false;
                }
            });

            return rv;
        }
        private bool AreEquivalentNodes(Node n1, Node n2, Func<Node, Node, bool> nodeCompareFn = null)
        {
            if (nodeCompareFn != null)
                return nodeCompareFn(n1, n2);
            else
                return n1.Id.Equals(n2.Id) && object.Equals(n1.Value, n2.Value);
        }
        #endregion
    }

    public static class GraphCompareExtensions
    {
        #region Declarations
        public static string NAME = "GraphCompare";
        #endregion

        public static GraphCompare WithCompare(this Graph decorated)
        {
            return GraphCompare.New(decorated);
        }
        public static Polyface<Graph> WithCompare(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            var face = polyface.As<GraphCompare>(NAME);
            if (face == null)
            {
                var root = polyface.Root;
                var dec = GraphCompareExtensions.WithCompare(root);
                polyface.Has(NAME, dec);
            }

            return polyface;
        }
        public static Polyface<Graph> UndecorateCompare(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            polyface.RemoveFace(NAME);
            return polyface;
        }
        public static GraphCompare AsCompare(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            return polyface.As<GraphCompare>(NAME);
        }
    }
}
