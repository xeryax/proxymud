using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Mapper
{
    [DataContract]
    public class Room
    {
        public Room(uint entry)
        {
            Entry = entry;
        }

        [DataMember]
        public readonly uint Entry;

        [DataMember]
        public string Name
        {
            get;
            internal set;
        }

        [DataMember]
        public string Sector
        {
            get;
            internal set;
        }

        [DataMember]
        internal List<string> IFlags = null;

        public IEnumerable<string> Flags
        {
            get
            {
                return IFlags;
            }
        }

        /// <summary>
        /// Check if room has a flag. These are actual flags that you get from recon and not custom flags.
        /// </summary>
        /// <param name="flag">Flag to check for.</param>
        /// <returns></returns>
        public bool HasFlag(string flag)
        {
            return IFlags != null && IFlags.Contains(flag.ToLower().Trim());
        }

        /// <summary>
        /// Add a flag to room. These are actual flags that you get from recon and not custom flags.
        /// </summary>
        /// <param name="flag">Flag to add.</param>
        public void AddFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(IFlags == null)
                IFlags = new List<string>();
            if(!string.IsNullOrEmpty(flag) && !IFlags.Contains(flag))
                IFlags.Add(flag);
        }

        /// <summary>
        /// Remove a flag from room. These are actual flags that you get from recon and not custom flags.
        /// </summary>
        /// <param name="flag">Flag to remove.</param>
        /// <returns></returns>
        public bool RemoveFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(IFlags != null && IFlags.Contains(flag))
            {
                IFlags.Remove(flag);
                return true;
            }
            return false;
        }

        [DataMember]
        internal List<string> CFlags = null;

        public IEnumerable<string> CustomFlags
        {
            get
            {
                return CFlags;
            }
        }

        [DataMember]
        public uint EntryCost
        {
            get;
            internal set;
        }

        [DataMember]
        public uint LeaveCost
        {
            get;
            internal set;
        }

        /// <summary>
        /// Check if room has a flag. These are custom flags and not actual flags like norecall which you get from recon.
        /// </summary>
        /// <param name="flag">Flag to check for.</param>
        /// <returns></returns>
        public bool HasCustomFlag(string flag)
        {
            return CFlags != null && CFlags.Contains(flag.ToLower().Trim());
        }

        /// <summary>
        /// Add a flag to room. These are custom flags and not actual flags like norecall which you get from recon.
        /// </summary>
        /// <param name="flag">Flag to add.</param>
        public void AddCustomFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(CFlags == null)
                CFlags = new List<string>();
            if(!string.IsNullOrEmpty(flag) && !CFlags.Contains(flag))
                CFlags.Add(flag);
        }

        /// <summary>
        /// Remove a flag from room. These are custom flags and not actual flags like norecall which you get from recon.
        /// </summary>
        /// <param name="flag">Flag to remove.</param>
        /// <returns></returns>
        public bool RemoveCustomFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(CFlags != null && CFlags.Contains(flag))
            {
                CFlags.Remove(flag);
                return true;
            }
            return false;
        }

        [DataMember]
        public int HealRate
        {
            get;
            internal set;
        }

        [DataMember]
        public int ManaRate
        {
            get;
            internal set;
        }

        [DataMember]
        internal List<Exit> exits = new List<Exit>();

        internal uint Mapper_OpenCost = 0;
        internal Exit Mapper_OpenBy = null;

        public Exit[] Exits
        {
            get
            {
                return exits.ToArray();
            }
        }

        private Area _area;

        public Area Area
        {
            get
            {
                return _area;
            }
            internal set
            {
                if(_area != null)
                    _area.rooms.Remove(this);
                _area = value;
                if(!_area.rooms.Contains(this))
                    _area.rooms.Add(this);
            }
        }

        public Exit GetExit(char Direction)
        {
            foreach(Exit e in exits)
            {
                if(!string.IsNullOrEmpty(e.Command) && e.Command.Length == 1 && e.Command[0] == Direction)
                    return e;
            }

            return null;
        }

        public Exit GetExit(string Command)
        {
            foreach(Exit e in exits)
            {
                if(!string.IsNullOrEmpty(e.Command) && e.Command == Command)
                    return e;
            }

            return null;
        }

        public Exit GetExit(uint Entry)
        {
            foreach(Exit e in exits)
            {
                if(e.Entry == Entry)
                    return e;
            }

            return null;
        }

        internal void UpdateExits()
        {
            SortedDictionary<string, List<Exit>> x = new SortedDictionary<string,List<Exit>>();
            foreach(Exit e in exits)
            {
                string cmd;
                if(string.IsNullOrEmpty(e.Command) || string.IsNullOrEmpty(cmd = e.Command.ToLower().Trim()))
                    continue;

                e.Command = cmd;
                if(!x.ContainsKey(e.Command))
                    x[e.Command] = new List<Exit>();
                x[e.Command].Add(e);
            }

            exits.Clear();

            foreach(KeyValuePair<string, List<Exit>> i in x)
            {
                foreach(Exit j in i.Value)
                    exits.Add(j);
            }
        }

        public override string ToString()
        {
            return "Room '" + (Name ?? "NULL") + "' [" + Entry + "]";
        }
    }
}
