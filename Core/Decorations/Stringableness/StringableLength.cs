using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyfacing.Core.Decorations.Stringableness
{

    /// <summary>
    /// decorates stringable with length data and some parsing functionality
    /// </summary>
    public class StringableLength : DecorationBase<Stringable>, IStringable
    {
        #region Declarations
        public static string PREFIX = Delim.RS.ToString();
        public static string DELIM = Delim.US.ToString();
        public static string SUFFIX = Delim.RS.ToString();
        #endregion

        #region Ctor
        public StringableLength(Stringable decorated)
            : base(decorated)
        {
        }
        public static StringableLength New(Stringable decorated)
        {
            return new StringableLength(decorated);
        }
        public static StringableLength Parse(string text)
        {
            var rv = Stringable.New(null).WithLength();
            rv.ParseText(text);
            return rv;
        }
        #endregion

        #region IStringable
        public string GetText()
        {
            // An encoded string will look like this {RS}Length{US}Data{RS}.  

            var rawText = this.Decorated.GetText();

            int length = 0;
            if (!string.IsNullOrEmpty(rawText))
                length = rawText.Length;

            var rv = string.Format("{0}{1}{2}{3}{4}", PREFIX, length, DELIM, rawText, SUFFIX);
            return rv;
        }
        public void ParseText(string text)
        {
            if (!IsLengthFormatted(text))
                throw new InvalidOperationException("bad format");

            var data = text.GetFrom(DELIM);
            data = data.Substring(0, data.Length - SUFFIX.Length);

            this.Decorated.ParseText(data);
        }

        public object Value
        {
            get
            {
                return this.Decorated.Value;
            }
        }
        #endregion

        #region Methods
        
        /// <summary>
        /// using the length formatting, parses out a stringable envelope from the text, returning the parsed text and
        /// the remaining text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool StringableEnvelopeParse(string sourceText, out string envelope, out string remainingSource)
        {
            bool rv = false;

            //validate the prefix
            if (!sourceText.StartsWith(PREFIX))
            {
                envelope = null;
                remainingSource = null;
                return rv;
            }

            //get the length prefix
            var lengthText = sourceText.GetFrom(PREFIX).GetTo(DELIM);
            var length = lengthText.ConvertToInt();
            if (length < 0)
            {
                envelope = null;
                remainingSource = null;
                return rv;
            }

            //the length of the envelope is prefix length + payload length length + delim length + payload length + suffix length 
            length = length +  PREFIX.Length + SUFFIX.Length + DELIM.Length + lengthText.Length;

            envelope = sourceText.Substring(0, length);

            if (sourceText.Length > length)
                remainingSource = sourceText.Substring(length + 1);
            else
                remainingSource = null;

            rv = true;

            return rv;
        }
        /// <summary>
        /// checks that the text has valid length formatting
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsLengthFormatted(string text)
        {
            if (!text.StartsWith(PREFIX))
                return false;

            if (!text.EndsWith(SUFFIX))
                return false;

            //get the length prefix
            var length = text.GetFrom(PREFIX).GetTo(DELIM).ConvertToInt();
            if (length < 0)
                return false;

            //parse using length.  
            var payloadByCount = text.GetFrom(DELIM).Substring(0, length);
            
            //validate the length is correct on the parsed text
            if (payloadByCount.Length != length)
                return false;

            //parse using delim
            var payloadByDelim = text.GetFrom(DELIM).GetTo(SUFFIX);
            
            //validate both parses return the same payload
            if (!payloadByCount.Equals(payloadByDelim))
                return false;

            return true;
        }
        #endregion


    }

    public static class StringableLengthExtensions
    {
        #region Declarations
        public static string NAME = "StringableLength";
        #endregion

        public static StringableLength WithLength(this Stringable decorated)
        {
            return StringableLength.New(decorated);
        }
        public static Polyface<Stringable> WithLength(this Polyface<Stringable> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            var face = polyface.As<StringableLength>(NAME);
            if (face == null)
            {
                var root = polyface.Root;
                var dec = StringableLengthExtensions.WithLength(root);
                polyface.Has(NAME, dec);
            }

            return polyface;
        }
        public static Polyface<Stringable> UndecorateLength(this Polyface<Stringable> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            polyface.RemoveFace(NAME);
            return polyface;
        }
        public static StringableLength AsLength(this Polyface<Stringable> polyface)
        {
            if (polyface == null)
                throw new ArgumentNullException("polyface");

            return polyface.As<StringableLength>(NAME);
        }
    }
}
