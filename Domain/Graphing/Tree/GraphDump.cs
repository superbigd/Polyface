using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;
using System.Runtime.Remoting.Messaging;
using Polyfacing.Core;
using Polyfacing.Core.Services.Serialization;
using Polyfacing.Core.Utils;

namespace Polyfacing.Domain.Graphing.Tree
{

    public class GraphDump : DecorationBase<Graph>
    {
        #region Inner Classes
        /// <summary>
        /// less polyfacing version of node, for serialization  
        /// </summary>
        public class SimpleNode
        {
            #region Ctor
            public SimpleNode() { }
            private SimpleNode(Node node)
            {
                if (node == null)
                    throw new ArgumentNullException("node");

                this.Children = new List<SimpleNode>();
                this.Id = node.Id;
                this.Value = node.Value;

                foreach (var each in node.Children)
                    if(each != null)
                        this.Children.Add(SimpleNode.New(each));
            }
            public static SimpleNode New(Node node)
            {
                return new SimpleNode(node);
            }
            #endregion

            #region Properties

            public string Id { get; set; }
            public object Value { get; set; }
            public List<SimpleNode> Children { get; set; }
            #endregion


        }

        #endregion

        #region Ctor
        public GraphDump(Graph decorated)
            : base(decorated)
        {
        }

        public static GraphDump New(Graph decorated)
        {
            return new GraphDump(decorated);
        }
        #endregion

        #region Methods
        public string DumpToJSON()
        {
            var root = SimpleNode.New(this.Decorated.Root);

            var rv = SerializationManager.Instance.Get<MSJSONSerializer>().Serialize(root);
            return rv;
        }
        public string DumpToXML()
        {
            var json = this.DumpToJSON();
            var xml = JSON2XML.ToXML(json);
            
            return xml.ToString();
        }
        #endregion
    }

    public static class GraphDumpExtensions
    {
        #region Declarations
        public static string NAME = "GraphDump";
        #endregion

        public static GraphDump WithDump(this Graph decorated)
        {
            return GraphDump.New(decorated);
        }
        public static Polyface<Graph> WithDump(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            var face = polyface.As<GraphDump>(NAME);
            if (face == null)
            {
                var root = polyface.Root;
                var dec = GraphDumpExtensions.WithDump(root);
                polyface.Has(NAME, dec);
            }

            return polyface;
        }
        public static Polyface<Graph> UndecorateDump(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            polyface.RemoveFace(NAME);
            return polyface;
        }
        public static GraphDump AsDump(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            return polyface.As<GraphDump>(NAME);
        }
    }
}
