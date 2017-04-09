using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProxyCore.Messages;

namespace ProxyCore.Output
{
    public class TriggerData
    {
        internal TriggerData()
        {
        }

        /// <summary>
        /// Match data.
        /// </summary>
        public Match Match
        {
            get;
            internal set;
        }

        /// <summary>
        /// What we triggered on, you can change this value to replace the text.
        /// </summary>
        public Message Msg;

        /// <summary>
        /// Custom argument if you registered a trigger to have one.
        /// </summary>
        public int Arg;

        /// <summary>
        /// Replace matched %0 value with new string. This only works in regex triggers. You can have {%1} - {%n} in the new string for matched data.
        /// Use {%%1} to escape. If you use % higher than what was captured then a NULL will be replaced there.
        /// For example using %3 when there are only 2 things captured with regex.<para></para>
        /// <para></para>
        /// Example:<para></para>
        /// Line: "@mQuest Points      @w:         @Y19,361"<para></para>
        /// Pattern: "^@mQuest Points      @w: (\s*)@Y([\d,]+)$"<para></para>
        /// Replace: "@mQuest Points      @w: {%1}@G{%2}"<para></para>
        /// This would make quest points green in the "worth" command output. Where {%1} inserts the right amount
        /// of spaces (what was captured) and {%2} inserts the quest points amount (19,361).
        /// </summary>
        /// <param name="New">New string to replace with. See function summary for help with this.</param>
        /// <param name="AllowParse">Allow parsing {%n} in the New string or not. If not then string
        /// is replaced as is without parsing for arguments.</param>
        public void Replace(string New, bool AllowParse)
        {
            if(Match == null || !Match.Success)
                return;

            if(AllowParse)
            {
                Match m;
                if(New.Contains("{%")) // Make fast check first if we want to start messing with regex
                {
                    while((m = _replaceRegex.Match(New)).Success)
                    {
                        int i;
                        if(!int.TryParse(m.Groups[1].Value, out i) || i >= Match.Groups.Count || i < 0)
                            New = New.Replace(m.Groups[0].Value, "NULL");
                        else
                            New = New.Replace(m.Groups[0].Value, Match.Groups[i].Value);
                    }
                }
                if(New.Contains("{%%"))
                {
                    while((m = _replaceRegex2.Match(New)).Success)
                    {
                        New = New.Replace(m.Groups[0].Value, "{%" + m.Groups[1].Value + "}");
                    }
                }
            }

            // Don't use replace here because we only want to replace one occurance, wherever it was matched
            Msg.Msg = Msg.Msg.Remove(Match.Index, Match.Length).Insert(Match.Index, New);
        }

        private static readonly Regex _replaceRegex = new Regex(@"\{%(\d+)\}", RegexOptions.Compiled);
        private static readonly Regex _replaceRegex2 = new Regex(@"\{%%(\d+)\}", RegexOptions.Compiled);
    }
}
