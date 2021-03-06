﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polyfacing.Core.Decorations.Interception;
using System.Runtime.Remoting.Messaging;
using System.Web.Script.Serialization;
using Polyfacing.Core;

namespace Polyfacing.Domain.Graphing.Tree
{
    /// <summary>
    /// decorates tracing with a text dump
    /// </summary>
    public class GraphTracerDump : DecorationBase<Graph>
    {
        #region Ctor
        public GraphTracerDump(Graph decorated)
            : base(decorated)
        {
            //add the dependencies
            this.TypedPolyface.WithTracer();
        }

        public static GraphTracerDump New(Graph decorated)
        {
            return new GraphTracerDump(decorated);
        }
        #endregion

        #region Methods
        public List<string> GetTraceLines()
        {
            List<string> rv = new List<string>();

            var calls = this.TypedPolyface.AsTracer().Calls;

            foreach (var each in calls)
            {
                string line = GetTraceLine(each);
                rv.Add(line);
            }

            return rv;
        }
        private string GetTraceLine(UnitofWorkInfo uow)
        {
//            string.Format("{0},{1}", each.Name, each.Args.AppendFormatEach((x)=>{return x.Serialize();})

            return string.Empty;
        }
        #endregion
    }

    public static class GraphTracerDumpExtensions
    {
        #region Declarations
        public static string NAME = "GraphTracerDump";
        #endregion

        public static GraphTracerDump WithTracerDump(Graph decorated)
        {
            return GraphTracerDump.New(decorated);
        }
        public static Polyface<Graph> WithTracerDump(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            var face = polyface.As<GraphTracerDump>(NAME);
            if (face == null)
            {
                var root = polyface.Root;
                var dec = GraphTracerDumpExtensions.WithTracerDump(root);
                polyface.Has(NAME, dec);
            }

            return polyface;
        }
        public static Polyface<Graph> UndecorateTracerDump(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            polyface.RemoveFace(NAME);
            return polyface;
        }
        public static GraphTracerDump AsTracerDump(this Polyface<Graph> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            return polyface.As<GraphTracerDump>(NAME);
        }
    }
}
