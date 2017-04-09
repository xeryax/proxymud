using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using ProxyCore;

namespace Mapper.Scripting
{
    public class AreaScript
    {
        public AreaScript(params uint[] AreaID)
        {
            AreaId = AreaID;
        }

        public readonly uint[] AreaId;

        public int RequiredMapperVersion
        {
            get;
            protected set;
        }

        public virtual bool CanUseExit(Exit e)
        {
            return true;
        }

        public virtual bool CanUsePortal(Exit p, Room r)
        {
            return true;
        }

        public virtual string GetExitCommand(Exit e)
        {
            return e.Command == "stop" ? null : e.Command;
        }

        public virtual string GetDoorCommand(Exit e)
        {
            if(!string.IsNullOrEmpty(e.DoorCommand))
                return e.DoorCommand;
            if(e.HasFlag("door") && PathfindResult.IsDirectionCommand(e.Command) != 'x')
                return "open " + e.Command;
            return string.Empty;
        }

        public virtual uint GetLeaveRoomCost(Room r, Exit e)
        {
            return r.LeaveCost;
        }

        public virtual uint GetExitCost(Exit e)
        {
            return e.Cost;
        }

        public virtual uint GetPortalCost(Exit e)
        {
            return e.Cost;
        }

        public virtual uint GetEnterRoomCost(Room r, Exit e)
        {
            return r.EntryCost;
        }

        public virtual void OnEnterRoom(Room r, Room oldRoom)
        {
        }

        public virtual void OnLeaveRoom(Room r, Room newRoom)
        {
        }

        public virtual void OnEnterArea(Area a, Area oldArea)
        {
        }

        public virtual void OnLeaveArea(Area a, Area newArea)
        {
        }
    }

    public class AreaScriptMgr
    {
        internal static Dictionary<uint, AreaScript> Scripts = new Dictionary<uint, AreaScript>();
        internal static AreaScript Default = new AreaScript();

        public static void RegisterScript(uint AreaId, AreaScript Script)
        {
            if(Script != null)
                Scripts[AreaId] = Script;
        }

        internal static AreaScript GetScript(uint AreaId)
        {
            return Scripts.ContainsKey(AreaId) ? Scripts[AreaId] : Default;
        }
    }
}
