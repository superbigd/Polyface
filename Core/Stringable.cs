
using Polyfacing.Core.Services.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Polyfacing.Core
{
    /// <summary>
    /// describes a string that is convertable to another string, and vice versa
    /// </summary>
    public interface IStringable
    {
        string GetText();
        void ParseText(string text);
        object Value { get; }
    }

    /// <summary>
    /// to facilitate string handling we create a base class upon which to decorate string-y functionality.  
    /// in this way a thing must be converted to stringable, and then the decoration functionality is avail.
    /// </summary>
    /// 
    [DebuggerDisplay("Stringable {Value}")]
    public class Stringable : MarshalByRefObject, IKnowsPolyface, IStringable
    {
        #region Ctor
 
        private Stringable(object obj)
        {
            this.Polyface = Polyface<Stringable>.New(this);
            this.Value = obj;
        }
        public static Stringable New(object obj)
        {
            return new Stringable(obj);
        }
        public static Stringable Parse(string text)
        {
            var rv = Stringable.New(null);
            rv.ParseText(text);
            return rv;
        }
        #endregion


        #region IStringable
        public string GetText()
        {
            var rv = SerializationManager.Instance.DefaultSerialize(this.Value);
            return rv;
        }
        public void ParseText(string text)
        {
            this.Value = SerializationManager.Instance.Deserialize(text);
        }
        public object Value { get; private set; }

        #endregion


        #region IKnowsPolyface - Polyface plumbing
        public Polyface<Stringable> TypedPolyface { get { return this.Polyface as Polyface<Stringable>; } }
        public IPolyface Polyface { get; set; }
        #endregion

    }



}
