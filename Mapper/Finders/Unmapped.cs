using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapper
{
    public class Pathfinder_Unmapped : Pathfinder
    {
        public Pathfinder_Unmapped(bool unReconed)
            : base()
        {
            UnReconed = unReconed;
        }

        private bool UnReconed;
        private uint[] AllowedAreas;

        public override void OnStartedPathfind()
        {
            base.OnStartedPathfind();

            List<uint> a = new List<uint>();
            foreach(Room r in StartRooms)
            {
                if(r.Area.Entry == uint.MaxValue)
                    continue;
                if(!a.Contains(r.Area.Entry))
                    a.Add(r.Area.Entry);
            }

            AllowedAreas = a.ToArray();
        }

        public override bool IsTargetRoom(Room r)
        {
            if(!AllowedAreas.Contains(r.Area.Entry))
                return false;

            foreach(Exit e in r.exits)
            {
                if(e.To.Area.Entry == uint.MaxValue)
                    return true;
            }

            if(UnReconed)
            {
                if(!r.HasCustomFlag("reconed"))
                    return true;
            }

            return base.IsTargetRoom(r);
        }
    }
}
