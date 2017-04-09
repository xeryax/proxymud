using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Mapper
{
    [DataContract]
    public class Exit
    {
        public Exit(uint entry)
        {
            Entry = entry;
            MinLevel = 0;
            MaxLevel = 210;
            Cost = 1;
        }

        [DataMember]
        public readonly uint Entry;

        [DataMember]
        public int MinLevel
        {
            get;
            internal set;
        }

        [DataMember]
        public int MaxLevel
        {
            get;
            internal set;
        }

        public Room From
        {
            get;
            internal set;
        }

        public Room To
        {
            get
            {
                return _to;
            }
            internal set
            {
                _to = value;
                ToRoom = value != null ? value.Entry : uint.MaxValue;
            }
        }

        private Room _to;

        [DataMember]
        internal uint ToRoom;

        [DataMember]
        public string Command
        {
            get;
            internal set;
        }

        [DataMember]
        public string DoorCommand
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

        [DataMember]
        public uint Cost
        {
            get;
            internal set;
        }

        /// <summary>
        /// Check if exit has a flag.
        /// </summary>
        /// <param name="flag">Flag to check for.</param>
        /// <returns></returns>
        public bool HasFlag(string flag)
        {
            return IFlags != null && IFlags.Contains(flag.ToLower().Trim());
        }

        /// <summary>
        /// Add a flag to exit.
        /// </summary>
        /// <param name="flag">Flag to add.</param>
        public void AddFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(flag == "portal")
                return;
            if(IFlags == null)
                IFlags = new List<string>();
            if(!string.IsNullOrEmpty(flag) && !IFlags.Contains(flag))
                IFlags.Add(flag);
        }

        /// <summary>
        /// Remove a flag from exit.
        /// </summary>
        /// <param name="flag">Flag to remove.</param>
        /// <returns></returns>
        public bool RemoveFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(flag == "portal")
                return false;
            if(IFlags != null && IFlags.Contains(flag))
            {
                IFlags.Remove(flag);
                return true;
            }
            return false;
        }
    }
}
