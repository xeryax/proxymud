using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mapper
{
    public class PathFinder_VisitAll : Pathfinder
    {
        public PathFinder_VisitAll(Mapper mapper, params Room[] roomId)
            : base()
        {
            map = mapper;
            foreach(Room u in roomId)
            {
                if(!AllRooms.Contains(u))
                    AllRooms.Add(u);
            }

            if(_grid[0] == null)
            {
                for(int i = 0; i < _grid.Length; i++)
                    _grid[i] = new List<uint[]>(512);
            }

            for(int i = 0; i < _grid.Length; i++)
                _grid[i].Clear();
        }

        private readonly Mapper map;
        private readonly List<Room> AllRooms = new List<Room>();
        private readonly Dictionary<uint, Dictionary<uint, uint>> Costs = new Dictionary<uint, Dictionary<uint, uint>>();
        private static List<uint[]>[] _grid = new List<uint[]>[1024];

        public override bool IsTargetRoom(Room r)
        {
            return true;
        }

        public override void OnStartedPathfind()
        {
            base.OnStartedPathfind();

            foreach(Room x in StartRooms)
                AllRooms.Remove(x);
        }

        public override void OnEndedPathFind(PathfindResult pr)
        {
            base.OnEndedPathFind(pr);

            foreach(Room x in StartRooms)
                AllRooms.Add(x);

            foreach(Room u in AllRooms)
            {
                _Internal_PathFinder_VisitAll pf = new _Internal_PathFinder_VisitAll((from val in AllRooms where val != u select val).ToArray());
                pf.CanUsePortals = CanUsePortals;
                pf.CanUseRecalls = CanUseRecalls;
                pf.CharacterLevel = CharacterLevel;
                pf.CharacterTier = CharacterTier;
                pf.IsGlobalQuest = IsGlobalQuest;
                pf.IsSingleClassTier0 = IsSingleClassTier0;
                pf.OverridePortals = OverridePortals;
                pf.SkipExits = SkipExits;
                pf.SkipPortals = SkipPortals;
                pf.SkipRooms = SkipRooms;
                pf.StartRooms = new[] { u };

                PathfindResult p = map.Get(pf);
                if(!p.Success)
                {
                    pr.Success = false;
                    return;
                }

                Costs[u.Entry] = new Dictionary<uint, uint>();
                foreach(Room x in AllRooms)
                {
                    if(x == u)
                        continue;

                    if(x.Mapper_OpenBy != null)
                        Costs[u.Entry][x.Entry] = x.Mapper_OpenCost;
                    else
                    {
                        pr.Success = false;
                        return;
                    }
                }
            }

            OpenRoom(new[] { StartRooms[0].Entry }, 0);

            uint[] Finish = null;
            int itr1 = 0, itr2 = 0;
            while(itr1 < _grid.Length)
            {
                if(itr2 >= _grid[itr1].Count)
                {
                    itr2 = 0;
                    _grid[itr1].Clear();
                    itr1++;
                    continue;
                }

                if(OpenRoom(_grid[itr1][itr2], (uint)itr1))
                {
                    Finish = _grid[itr1][itr2];
                    break;
                }

                itr2++;
            }

            if(Finish == null)
                pr.Success = false;
            else
            {
                pr.Cost = itr1;
                pr.Path.Clear();
                pr.Target = map.GetRoom(Finish[Finish.Length - 1]);
                Room u = map.GetRoom(Finish[0]);
                int j = 1;
                while(u != null && j < Finish.Length)
                {
                    Pathfinder_Entry pf = new Pathfinder_Entry(Finish[j]);
                    pf.StartRooms = new[] { u };
                    pf.CanUsePortals = CanUsePortals;
                    pf.CanUseRecalls = CanUseRecalls;
                    pf.CharacterLevel = CharacterLevel;
                    pf.CharacterTier = CharacterTier;
                    pf.IsGlobalQuest = IsGlobalQuest;
                    pf.IsSingleClassTier0 = IsSingleClassTier0;
                    pf.OverridePortals = OverridePortals;
                    pf.SkipExits = SkipExits;
                    pf.SkipPortals = SkipPortals;
                    pf.SkipRooms = SkipRooms;

                    PathfindResult p = map.Get(pf);
                    if(!p.Success)
                    {
                        pr.Success = false;
                        return;
                    }

                    pr.Path.AddRange(p.Path);
                    u = map.GetRoom(Finish[j]);
                    j++;
                }
            }
        }

        private bool OpenRoom(uint[] r, uint cost)
        {
            if(r.Length == AllRooms.Count)
                return true;

            foreach(KeyValuePair<uint, uint> x in Costs[r[r.Length - 1]])
            {
                if(r.Contains(x.Key))
                    continue;

                uint[] z = r.Concat(new[] { x.Key }).ToArray();
                uint c = cost + x.Value;
                if(c < _grid.Length)
                    _grid[(int)c].Add(z);
            }

            return false;
        }
    }

    public class _Internal_PathFinder_VisitAll : Pathfinder
    {
        public _Internal_PathFinder_VisitAll(params Room[] roomId)
        {
            AllRooms.AddRange(roomId);
        }

        private readonly List<Room> AllRooms = new List<Room>();

        public override bool IsTargetRoom(Room r)
        {
            AllRooms.Remove(r);
            return AllRooms.Count == 0;
        }
    }
}
