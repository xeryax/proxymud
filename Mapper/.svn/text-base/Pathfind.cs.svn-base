using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mapper.Scripting;

namespace Mapper
{
    public partial class Mapper
    {
        private List<Exit>[] _pGrid = new List<Exit>[4096];
        private uint _pGridMax = 0;
        private List<Exit>[] _pExits = new[] { new List<Exit>(), new List<Exit>() };

        private void Path_Init()
        {
            for(int i = 0; i < _pGrid.Length; i++)
                _pGrid[i] = new List<Exit>(512);
        }

        private bool CanUsePortal(Exit p, Pathfinder x)
        {
            if(!p.HasFlag("recallmechanic"))
            {
                if(!x.CanUsePortals)
                    return false;
            }
            else
            {
                if(!x.CanUseRecalls)
                    return false;
            }

            if(p.MinLevel > x.CharacterLevel + x.CharacterTier * 10 ||
                p.MaxLevel < x.CharacterLevel)
                return false;

            if(p.HasFlag("nogq") && x.IsGlobalQuest)
                return false;

            if(x.SkipPortals != null && x.SkipPortals.Contains(p))
                return false;

            return true;
        }

        internal static AreaScript GetScript(Room r)
        {
            return AreaScriptMgr.GetScript(r.Area.Entry);
        }

        public PathfindResult Get(Pathfinder p)
        {
            if(p.StartRooms != null)
                p.StartRooms = p.StartRooms.Where(val => val != null).ToArray();
            if(p.StartRooms == null || p.StartRooms.Length == 0)
                return new PathfindResult() { Success = false };

            for(int i = 0; i < _pGridMax; i++)
                _pGrid[i].Clear();
            _pExits[0].Clear();
            _pExits[1].Clear();
            _pGridMax = 0;
            foreach(KeyValuePair<uint, Room> x in IRooms)
            {
                x.Value.Mapper_OpenBy = null;
                x.Value.Mapper_OpenCost = 0;
            }

            p.OnStartedPathfind();

            if(p.OverridePortals == null)
            {
                foreach(KeyValuePair<uint, Exit> x in IPortals)
                {
                    if(!x.Value.HasFlag("disabled") && CanUsePortal(x.Value, p))
                        _pExits[!x.Value.HasFlag("recallmechanic") ? 0 : 1].Add(x.Value);
                }
            }
            else
            {
                foreach(Exit x in p.OverridePortals)
                {
                    if(!x.HasFlag("disabled") && CanUsePortal(x, p))
                        _pExits[!x.HasFlag("recallmechanic") ? 0 : 1].Add(x);
                }
            }

            Room Finish = null;
            if(p.StartRooms != null)
            {
                foreach(Room r in p.StartRooms)
                {
                    r.Mapper_OpenCost = uint.MaxValue;
                    if(OpenRoom(r, null, 0, p))
                    {
                        Finish = r;
                        break;
                    }
                }
            }

            int itr1 = 0, itr2 = 0;
            if(Finish == null)
            {
                while(itr1 < _pGrid.Length && itr1 < _pGridMax)
                {
                    if(itr2 >= _pGrid[itr1].Count)
                    {
                        itr2 = 0;
                        itr1++;
                        continue;
                    }

                    if(OpenRoom(_pGrid[itr1][itr2].To, _pGrid[itr1][itr2], (uint)itr1, p))
                    {
                        Finish = _pGrid[itr1][itr2].To;
                        break;
                    }

                    itr2++;
                }
            }

            PathfindResult pr = new PathfindResult();
            pr.Success = Finish != null;
            pr.Cost = itr1;
            pr.Target = Finish;
            while(Finish != null && Finish.Mapper_OpenBy != null)
            {
                pr.Path.Insert(0, Finish.Mapper_OpenBy);
                Finish = Finish.Mapper_OpenBy.From;
            }
            pr.Start = pr.Path.Count != 0 ? pr.Path[0].From : Finish;

            p.OnEndedPathFind(pr);
            return pr;
        }

        private bool OpenRoom(Room r, Exit from, uint cost, Pathfinder p)
        {
            if(r.Mapper_OpenBy != null || (from != null && p.StartRooms.Contains(r)) || r.Area.Entry == uint.MaxValue)
                return false;

            r.Mapper_OpenBy = from;
            r.Mapper_OpenCost = cost;

            if(p.IsTargetRoom(r))
                return true;

            if(_pExits[0].Count > 0 && !r.HasFlag("prison"))
            {
                for(int i = _pExits[0].Count - 1; i >= 0; i--)
                {
                    if(_pExits[0][i].To.Mapper_OpenBy != null)
                        _pExits[0].RemoveAt(i);
                    else if(GetScript(r).CanUsePortal(_pExits[0][i], r) && p.CanUsePortal(_pExits[0][i], r))
                    {
                        _pExits[0][i].From = r;
                        AddExit(_pExits[0][i], p, true);
                        _pExits[0].RemoveAt(i);
                    }
                }
            }
            if(_pExits[1].Count > 0 && !r.HasFlag("norecall"))
            {
                for(int i = _pExits[1].Count - 1; i >= 0; i--)
                {
                    if(_pExits[1][i].To.Mapper_OpenBy != null)
                        _pExits[1].RemoveAt(i);
                    else if(GetScript(r).CanUsePortal(_pExits[1][i], r) && p.CanUsePortal(_pExits[1][i], r))
                    {
                        _pExits[1][i].From = r;
                        AddExit(_pExits[1][i], p, true);
                        _pExits[1].RemoveAt(i);
                    }
                }
            }
            foreach(Exit e in r.exits)
            {
                if(e.To.Mapper_OpenBy != null)
                    continue;
                if(e.To.Area.Entry == uint.MaxValue)
                    continue;
                if(e.HasFlag("disabled"))
                    continue;
                if(!GetScript(r).CanUseExit(e))
                    continue;
                if(e.MinLevel > p.CharacterLevel)
                    continue;
                if(e.MaxLevel < p.CharacterLevel)
                    continue;
                if(!p.CanUseExit(e))
                    continue;
                AddExit(e, p, false);
            }

            return false;
        }

        private void AddExit(Exit e, Pathfinder p, bool isPortal)
        {
            uint cost = e.From.Mapper_OpenCost + GetScript(e.From).GetLeaveRoomCost(e.From, e) +
                (!isPortal ? GetScript(e.From).GetExitCost(e) : GetScript(e.From).GetPortalCost(e)) + GetScript(e.To).GetEnterRoomCost(e.To, e) +
                p.GetLeaveRoomCost(e.From, e) +
                (!isPortal ? p.GetExitCost(e) : p.GetPortalCost(e)) + p.GetEnterRoomCost(e.To, e);

            uint o = p.GetOverrideCost(e, isPortal);
            if(o != uint.MaxValue)
                cost = e.From.Mapper_OpenCost + o;

            if(cost >= _pGrid.Length)
                return;
            _pGrid[cost].Add(e);
            cost++;
            if(cost > _pGridMax)
                _pGridMax = cost;
        }
    }

    public class PathfindResult
    {
        internal PathfindResult()
        {
        }

        public List<Exit> Path = new List<Exit>();
        public int Cost = 0;
        public bool Success = false;
        public Room Target;
        public Room Start;

        /// <summary>
        /// Generate a speedwalk from exit list.
        /// </summary>
        /// <param name="Exits">List of exits to generate a speedwalk from.</param>
        /// <returns></returns>
        public static string Speedwalk(IEnumerable<Exit> Exits)
        {
            List<string> sPath = new List<string>();

            foreach(Exit x in Exits)
            {
                string Door = Mapper.GetScript(x.From).GetDoorCommand(x);
                string Command = Mapper.GetScript(x.From).GetExitCommand(x);
                if(!string.IsNullOrEmpty(Door))
                    sPath.AddRange(Door.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                if(Command == null)
                    break;
                sPath.AddRange(Command.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries));
            }

            StringBuilder strSW = new StringBuilder();
            StringBuilder strCur = new StringBuilder();
            foreach(string x in sPath)
            {
                if(IsDirectionCommand(x) != 'x')
                {
                    strCur.Append(char.ToLower(x[0]));
                }
                else
                {
                    if(strCur.Length > 0)
                    {
                        if(strSW.Length > 0)
                            strSW.Append(';');
                        if(strCur.Length > 1)
                            strSW.Append("run ");
                        strSW.Append(CompressPath(strCur.ToString()));
                        strCur.Remove(0, strCur.Length);
                    }

                    if(strSW.Length > 0)
                        strSW.Append(';');

                    strSW.Append(x);
                }
            }

            if(strCur.Length > 0)
            {
                if(strSW.Length > 0)
                    strSW.Append(';');
                if(strCur.Length > 1)
                    strSW.Append("run ");
                strSW.Append(CompressPath(strCur.ToString()));
                strCur.Remove(0, strCur.Length);
            }

            return strSW.ToString();
        }

        /// <summary>
        /// Check if command is direction command. For example "west" -> 'w'. If it's not a direction
        /// then return 'x'.
        /// </summary>
        /// <param name="cmd">Command to check.</param>
        /// <returns></returns>
        public static char IsDirectionCommand(string cmd)
        {
            cmd = cmd.ToLower().Trim();

            if(cmd.Length == 0)
                return 'x';

            if(cmd.Length <= "north".Length && "north".StartsWith(cmd))
                return 'n';
            if(cmd.Length <= "west".Length && "west".StartsWith(cmd))
                return 'w';
            if(cmd.Length <= "south".Length && "south".StartsWith(cmd))
                return 's';
            if(cmd.Length <= "east".Length && "east".StartsWith(cmd))
                return 'e';
            if(cmd.Length <= "down".Length && "down".StartsWith(cmd))
                return 'd';
            if(cmd.Length <= "up".Length && "up".StartsWith(cmd))
                return 'u';
            return 'x';
        }

        /// <summary>
        /// Compresses "wwssse" into "2w3se".
        /// </summary>
        /// <param name="path">Path to compress.</param>
        /// <returns></returns>
        public static string CompressPath(string path)
        {
            char dir = 'x';
            int c = 0;
            StringBuilder str = new StringBuilder();
            for(int i = 0; i < path.Length; i++)
            {
                if(path[i] == dir)
                {
                    c++;
                    continue;
                }

                if(dir == 'x')
                {
                    dir = path[i];
                    c = 1;
                    continue;
                }

                if(c > 1)
                    str.Append(c.ToString());
                str.Append(dir.ToString());
                dir = path[i];
                c = 1;
            }

            if(dir != 'x')
            {
                if(c > 1)
                    str.Append(c.ToString());
                str.Append(dir.ToString());
            }

            return str.ToString();
        }
    }

    public class Pathfinder
    {
        protected Pathfinder()
        {
        }

        /// <summary>
        /// Set the character level for this path find. It is used in level lock areas and portals.
        /// </summary>
        public int CharacterLevel = 1;
        public int CharacterTier = 0;
        public bool IsGlobalQuest = false;
        public bool CanUsePortals = true;
        public bool CanUseRecalls = true;
        public bool IsSingleClassTier0 = false;
        public Room[] SkipRooms;
        public Exit[] SkipExits;
        public Exit[] SkipPortals;
        public Room[] StartRooms;
        public Exit[] OverridePortals;

        public virtual void CopyFrom(Pathfinder pf)
        {
            if(pf == null)
                return;

            CharacterLevel = pf.CharacterLevel;
            CharacterTier = pf.CharacterTier;
            IsGlobalQuest = pf.IsGlobalQuest;
            CanUsePortals = pf.CanUsePortals;
            CanUseRecalls = pf.CanUseRecalls;
            IsSingleClassTier0 = pf.IsSingleClassTier0;
            SkipRooms = pf.SkipRooms;
            SkipExits = pf.SkipExits;
            SkipPortals = pf.SkipPortals;
            StartRooms = pf.StartRooms;
            OverridePortals = pf.OverridePortals;
        }

        public virtual uint GetOverrideCost(Exit e, bool isPortal)
        {
            return uint.MaxValue;
        }

        public virtual void OnStartedPathfind()
        {
        }

        public virtual bool IsTargetRoom(Room r)
        {
            return false;
        }

        public virtual void OnEndedPathFind(PathfindResult pr)
        {
        }

        public virtual bool CanUseExit(Exit e)
        {
            return true;
        }

        public virtual bool CanUsePortal(Exit p, Room r)
        {
            return true;
        }

        public virtual uint GetLeaveRoomCost(Room r, Exit e)
        {
            return 0;
        }

        public virtual uint GetExitCost(Exit e)
        {
            return 0;
        }

        public virtual uint GetPortalCost(Exit e)
        {
            return 0;
        }

        public virtual uint GetEnterRoomCost(Room r, Exit e)
        {
            return 0;
        }
    }
}
