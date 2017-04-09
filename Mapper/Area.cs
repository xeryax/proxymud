using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Mapper
{
    [DataContract]
    public class Area
    {
        public Area(uint entry)
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
        public string Keyword
        {
            get;
            internal set;
        }

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

        [DataMember]
        public int LevelLock
        {
            get;
            internal set;
        }

        [DataMember]
        public uint StartRoom
        {
            get;
            internal set;
        }

        [DataMember]
        internal List<Room> rooms = new List<Room>();

        [DataMember]
        internal List<Exit> Portals = new List<Exit>();

        public Room[] Rooms
        {
            get
            {
                return rooms.ToArray();
            }
        }

        [DataMember]
        public int ExplorableRooms
        {
            get;
            internal set;
        }

        /// <summary>
        /// Get all rooms in area with this name (exact, case sensitive).
        /// </summary>
        /// <param name="Name">Name of room.</param>
        /// <returns></returns>
        public List<Room> GetRooms(string Name)
        {
            List<Room> r = new List<Room>();
            foreach(Room x in rooms)
            {
                if(x.Name == Name)
                    r.Add(x);
            }

            return r;
        }

        [DataMember]
        internal List<string> Flags = null;

        /// <summary>
        /// Check if area has a flag.
        /// </summary>
        /// <param name="flag">Flag to check for.</param>
        /// <returns></returns>
        public bool HasFlag(string flag)
        {
            return Flags != null && Flags.Contains(flag.ToLower().Trim());
        }

        /// <summary>
        /// Add a flag to area.
        /// </summary>
        /// <param name="flag">Flag to add.</param>
        public void AddFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(Flags == null)
                Flags = new List<string>();
            if(!string.IsNullOrEmpty(flag) && !Flags.Contains(flag))
                Flags.Add(flag);
        }

        /// <summary>
        /// Remove a flag from area.
        /// </summary>
        /// <param name="flag">Flag to remove.</param>
        /// <returns></returns>
        public bool RemoveFlag(string flag)
        {
            flag = flag != null ? flag.ToLower().Trim() : "";
            if(Flags != null && Flags.Contains(flag))
            {
                Flags.Remove(flag);
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Area '" + (!string.IsNullOrEmpty(Name) ? Name : "NULL") + "' [" + Entry + "]";
        }
    }
}
