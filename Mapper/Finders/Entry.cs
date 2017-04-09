using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapper
{
    public class Pathfinder_Entry : Pathfinder
    {
        public Pathfinder_Entry(params uint[] roomEntries)
            : base()
        {
            TargetRoomEntries = roomEntries;
        }

        public readonly uint[] TargetRoomEntries;

        public override bool IsTargetRoom(Room r)
        {
            return TargetRoomEntries.Contains(r.Entry);
        }
    }
}
