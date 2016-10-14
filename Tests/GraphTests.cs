using Polyfacing.Core.Decorations.Graphing.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Tests
{
    public static class GraphTests
    {
        public static void Test()
        {

            //build a simple graph - test mutation
            var graph = Graph.New("root");
            var graph2 = Graph.New("root");

            for (int i = 0; i < 10; i++)
            {
                graph.AddChild(i).AddNext("next to " + i).AddPrevious("previous to " + i);
                graph2.AddChild(i).AddNext("next to " + i).AddPrevious("previous to " + i);
            }

            if (!(graph.WithCompare().IsEquivalent(graph2)))
                throw new InvalidOperationException();


            //test nav
            graph.MoveToRoot();
            graph2.MoveToRoot();

            for (int i = 0; i < 10; i++)
            {
                graph.MoveDown();
                if (!graph.Current.Value.Equals("previous to " + i))
                    throw new InvalidOperationException();

                graph.MoveNext();
                if(!graph.Current.Value.Equals(i))
                    throw new InvalidOperationException();
                
                graph.MoveNext();
                if(!graph.Current.Value.Equals("next to " + i))
                    throw new InvalidOperationException();

                graph.MovePrevious().MovePrevious();
            }


        }

        public static void TestTrace()
        {

            //build a simple graph - test mutation
            var graph = Graph.New("root");
            var tracer = graph.TypedPolyface.WithTracer().AsTracer();

            for (int i = 0; i < 10; i++)
            {
                tracer.TypedRoot.AddChild(i).AddNext("next to " + i).AddPrevious("previous to " + i);
            }

            //this should be picked up on the trace
            var calls = tracer.Calls;


        }
    }
}
