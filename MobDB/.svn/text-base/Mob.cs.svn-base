using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MobDB
{
    [DataContract]
    public class Mob
    {
        internal Mob(uint e)
        {
            Entry = e;
        }

        /// <summary>
        /// ID of the mob.
        /// </summary>
        [DataMember]
        public readonly uint Entry;

        /// <summary>
        /// (Short)name of the mob, such as "an ant". This is what we see when fighting the mob.
        /// This field is an array because same longname mobs can sometimes have multiple different names.
        /// Names here are automatically normalized where first letter of the name is made lower case, rest
        /// is left intact. Keep that in mind when comparing names.
        /// </summary>
        public string[] Name
        {
            get
            {
                return _name;
            }
            internal set
            {
                _name = value ?? new string[0];
                for(int i = 0; i < _name.Length; i++)
                {
                    if(!string.IsNullOrEmpty(_name[i]))
                        _name[i] = MobDB.NormalizeName(_name[i]);
                }
            }
        }

        [DataMember]
        private string[] _name;

        /// <summary>
        /// Return all names separated by a comma ",".
        /// </summary>
        public string Names
        {
            get
            {
                string str = "";
                foreach(string x in Name)
                {
                    if(str.Length > 0)
                        str += ", ";
                    str += x;
                }

                return str;
            }
        }

        /// <summary>
        /// Long name of the mob, this is what we see in room when we type look.
        /// </summary>
        [DataMember]
        public string Longname
        {
            get;
            internal set;
        }

        /// <summary>
        /// Keywords of the mob, can be multiple separated with a space.
        /// </summary>
        [DataMember]
        public string Keyword
        {
            get;
            internal set;
        }

        /// <summary>
        /// Level of the mob, we get this from lastkills. For non-killable mobs this is -1.
        /// </summary>
        [DataMember]
        public int Level
        {
            get;
            internal set;
        }

        /// <summary>
        /// Keywords of areas this mob can be in. Have a keyword "all" if you want mob to be in any area like the Firestorm Phoenix.
        /// </summary>
        [DataMember]
        public string[] Areas
        {
            get;
            internal set;
        }

        /// <summary>
        /// Override the default color of mob with this. Set to null or empty string to disable this and use
        /// default color from the configuration file instead.
        /// </summary>
        [DataMember]
        public string DefaultColor
        {
            get;
            internal set;
        }

        [DataMember]
        internal List<string> Flags;

        [DataMember]
        internal List<MobLocation> Locations = new List<MobLocation>();

        /// <summary>
        /// Calculate chance that mob will be in room with this ID.
        /// </summary>
        /// <param name="RoomId">Room ID to check mob in.</param>
        /// <returns></returns>
        public double GetChance(uint RoomId)
        {
            foreach(MobLocation ml in Locations)
            {
                if(ml.RoomEntry != RoomId)
                    continue;

                return (ml.CountSeen * 100) / (double)(ml.CountSeen - ml.TimesSeen + ml.TimesVisited);
            }

            return 0.0;
        }

        /// <summary>
        /// Calculate room ID with best chance of having mob in it.
        /// </summary>
        /// <returns></returns>
        public uint GetBestRoom()
        {
            double b = 0.0;
            uint c = uint.MaxValue;
            foreach(MobLocation ml in Locations)
            {
                double a = GetChance(ml.RoomEntry);
                if(a > b)
                {
                    b = a;
                    c = ml.RoomEntry;
                }
            }

            return c;
        }

        /// <summary>
        /// Add a flag to mob.
        /// </summary>
        /// <param name="f">Flag to add.</param>
        public void AddFlag(string f)
        {
            if(f == null)
                return;
            f = f.ToLower().Trim();
            if(f.Length == 0)
                return;

            if(Flags == null)
                Flags = new List<string>();
            if(!Flags.Contains(f))
                Flags.Add(f);
        }

        /// <summary>
        /// Remove a flag from mob.
        /// </summary>
        /// <param name="f">Flag to remove.</param>
        /// <returns></returns>
        public bool RemoveFlag(string f)
        {
            if(f == null || Flags == null)
                return false;
            f = f.ToLower().Trim();
            if(f.Length == 0)
                return false;
            return Flags.Remove(f);
        }

        public bool HasFlag(string f)
        {
            if(f == null)
                return false;
            f = f.ToLower().Trim();
            if(f.Length == 0)
                return false;
            return Flags != null && Flags.Contains(f);
        }
    }
}
