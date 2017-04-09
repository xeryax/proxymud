using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapper
{
    public class Pathfinder_Area : Pathfinder
    {
        public Pathfinder_Area(params uint[] AreaID)
            : base()
        {
            AreaId = AreaID;
        }

        private readonly uint[] AreaId;

        public override bool IsTargetRoom(Room r)
        {
            return AreaId.Contains(r.Area.Entry);
        }
    }
}
