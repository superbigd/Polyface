
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;


namespace Polyfacing.Core.Decorations.Stringableness
{

    /// <summary>
    /// provides stringable facilities to enumerations.  does this by stringing.length all items
    /// </summary>
    public class StringableList : MarshalByRefObject, IKnowsPolyface, IStringable
    {
        #region Declarations
        public static string PREFIX = Delim.RS.ToString();
        public static string DELIM = Delim.US.ToString();
        public static string SUFFIX = Delim.RS.ToString();
        #endregion

        #region Ctor
        private StringableList(IEnumerable list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            this.Polyface = Polyface<StringableList>.New(this);
            this.Value = list;
        }
        public static StringableList New(IEnumerable list)
        {
            return new StringableList(list);
        }
        #endregion

        #region Calculated Properties
        public List<object> List
        {
            get
            {
                return this.Value as List<object>;
            }
        }
        #endregion

        #region IStringable
        public string GetText()
        {
            IEnumerable list = this.Value as IEnumerable;

            //for each item in the list, make it length stringable, and append to a larger string
            StringBuilder sb = new StringBuilder();

            foreach (var each in list)
            {
                var stringable = Stringable.New(each).WithLength();
                sb.Append(stringable.GetText());
            }

            var rawList = sb.ToString();

            //now wrap rawList as stringable
            return Stringable.New(rawList).WithLength().GetText();
        }
        public void ParseText(string text)
        {
            var outer = StringableLength.Parse(text);
            
            //if there's nothing to parse, stop
            if (outer.Value == null)
                return;

            //the inner object should be a string (the packed stringables), which we iteratively parse out
            string innerText = outer.Value.ToString();

            List<object> list = new List<object>();

            string remaining = text;
            string envelope = null;

            while (true)
            {
                if (StringableLength.StringableEnvelopeParse(remaining, out envelope, out remaining))
                {
                    var item = StringableLength.Parse(envelope);
                    if(item.Value != null)
                        list.Add(item.Value);
                }
                else
                    break;
            }

            this.Value = list;
        }
        public object Value { get; private set; }

        #endregion


        #region IKnowsPolyface - Polyface plumbing
        public Polyface<StringableList> TypedPolyface { get { return this.Polyface as Polyface<StringableList>; } }
        public IPolyface Polyface { get; set; }
        #endregion
    }
}
