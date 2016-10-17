using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Domain.CallNarration
{
    /*
        Since we have graph functionality with Graph (ie. parent, child, sibling)
        we use that to do graphing, and the node payload becomes the domain-specific 
        carrier of information.
     
     * 
     *  
     */




    /// <summary>
    /// the node in a narrative graph
    /// </summary>
    public class CallNarrativeNode
    {
        #region Ctor
        private CallNarrativeNode()
        {
        }
        public static CallNarrativeNode New()
        {
            return new CallNarrativeNode();
        }
        #endregion

        #region Properties
        public string Name { get; private set; }
        public int Actor { get; private set; }
        public List<object> Args { get; private set; }
        public object ReturnValue { get; private set; }
        public Exception Error { get; private set; }
        private bool HasStarted { get; set; }
        private bool HasEnded { get; set; }
        #endregion

        #region Methods
        public CallNarrativeNode Begin(string name, int actor, List<object> args)
        {
            //if we've already begun we can't do this again
            if (HasStarted)
                throw new InvalidOperationException("already started");

            this.HasStarted = true;
            this.Actor = actor;
            this.Name = name;
            this.Args = args;

            return this;
        }
        public CallNarrativeNode End(object rv, Exception ex)
        {
            //if we've already completed we can't do this again
            if (HasEnded)
                throw new InvalidOperationException("already ended");

            this.HasEnded = true;

            this.ReturnValue = rv;
            this.Error = ex;

            return this;
        }
        #endregion
    }
}
