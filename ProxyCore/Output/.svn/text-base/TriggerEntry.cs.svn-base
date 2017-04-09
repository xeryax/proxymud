using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyCore.Output
{
    internal class TriggerEntry
    {
        internal Regex Pattern;
        internal string PatternStr;
        internal TriggerFunction Function;
        internal int Priority;
        internal string Name;
        internal TriggerFlags Flags;
        internal int Arg;
        internal string Plugin;
        internal List<string> Disabled;
    }

    /// <summary>
    /// This is the function template that will be called when a trigger fires. Return true to gag it - this
    /// will also prevent other triggers from triggering on this line.
    /// </summary>
    /// <param name="Data">Triggered text data.</param>
    /// <returns></returns>
    public delegate bool TriggerFunction(TriggerData Data);

    [Flags]
    public enum TriggerFlags
    {
        None = 0,

        /// <summary>
        /// Normal triggers stop after finding the first match in a line,
        /// with this flag the trigger repeats for each match in the same line.
        /// This only applies if you use regex pattern.
        /// </summary>
        Repeat = 1,

        /// <summary>
        /// Ignore lower and upper case. This only applies if you use regex pattern.
        /// </summary>
        CaseInsensitive = 2,

        /// <summary>
        /// Pattern is not regex but instead just raw string. For example if you want
        /// to trigger on "@w--&gt; @WTICK @w&lt;--" there's no need to create a regex pattern
        /// just insert this string and set this flag in options and the trigger will be MUCH
        /// faster. Use regex only where necessary.
        /// </summary>
        NotRegex = 4,

        /// <summary>
        /// Trigger ignores color codes. If you set this flag you should NOT include any color codes
        /// in your trigger pattern.
        /// </summary>
        NonAnsi = 8,

        /// <summary>
        /// Matching will start from right and go to left.
        /// </summary>
        RightToLeft = 0x10,
    }
}
