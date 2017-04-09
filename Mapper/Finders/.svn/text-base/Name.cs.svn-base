using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mapper
{
    public class Pathfinder_Name : Pathfinder
    {
        public Pathfinder_Name(NameTypes t, params string[] Names)
            : base()
        {
            nt = t;
            _names = Names;
            if((t & NameTypes.Regex) != NameTypes.None)
            {
                _regexString = new Regex[Names.Length];
                for(int i = 0; i < Names.Length; i++)
                {
                    if(string.IsNullOrEmpty(Names[i]))
                        continue;

                    try
                    {
                        _regexString[i] = new Regex(Names[i], (t & NameTypes.CaseInsensitive) != NameTypes.None ? RegexOptions.IgnoreCase : RegexOptions.None);
                    }
                    catch
                    {
                        // User entered invalid regex pattern
                    }
                }
            }
            else if((t & NameTypes.CaseInsensitive) != NameTypes.None)
            {
                for(int i = 0; i < _names.Length; i++)
                    _names[i] = _names[i].ToLower();
            }
        }

        private readonly Regex[] _regexString;
        private readonly string[] _names;
        private readonly NameTypes nt;

        public override bool IsTargetRoom(Room r)
        {
            if(string.IsNullOrEmpty(r.Name))
                return false;

            if((nt & NameTypes.Regex) != NameTypes.None)
            {
                foreach(Regex x in _regexString)
                {
                    if(x == null)
                        continue;

                    if(x.Match(r.Name).Success)
                        return true;
                }
            }
            else
            {
                string rname = r.Name;
                if((nt & NameTypes.CaseInsensitive) != NameTypes.None)
                    rname = rname.ToLower();
                foreach(string x in _names)
                {
                    if((nt & NameTypes.Partial) != NameTypes.None)
                    {
                        if(rname.Contains(x))
                            return true;
                    }
                    else if(rname == x)
                        return true;
                }
            }

            return false;
        }
    }

    [Flags]
    public enum NameTypes
    {
        /// <summary>
        /// Exact name search. Room name must match what you entered and case sensitive.
        /// </summary>
        None = 0,

        /// <summary>
        /// Room name must contain the string you entered. This setting is ignored if you set regex option.
        /// </summary>
        Partial = 1,

        /// <summary>
        /// Room name vs. what you entered is not case sensitive.
        /// </summary>
        CaseInsensitive = 2,

        /// <summary>
        /// You entered a regex string which must match room name.
        /// </summary>
        Regex = 4,
    }
}
