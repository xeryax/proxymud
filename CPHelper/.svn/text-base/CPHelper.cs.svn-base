using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mapper;
using MobDB;
using ProxyCore;
using ProxyCore.Input;
using ProxyCore.Output;
using ProxyCore.Scripting;

namespace CPHelper
{
    public class CPHelper : Plugin
    {
        public CPHelper()
            : base("cphelper", "Campaign Helper")
        {
            Author = "Duckbat";
            Version = 5;
            Description = "Matches mobs from database to rooms. Creates useful tags for campaign check.";
            UpdateUrl = "www.duckbat.com/plugins/update.cphelper.txt";
            Website = "code.google.com/p/proxymud/";

            Config = new CPHelperConfig();

            RegisterCommand("autonoexp", @"^(\d+)", AutoNoexpCommand);
            RegisterCommand("gowhere", "", GoWhereCommand, 3);
            RegisterCommand("where", "(.+)", WhereCommand, 3);
            RegisterCommand("gquest", "(.+)", GQCheckCommand, 2);
            RegisterCommand("campaign", "(.+)", CampaignCheckCommand, 5);
            RegisterCommand("cp", "(.+)", CampaignCheckCommand, 0);
            RegisterCommand("awhere", "(.+)", AutoWhereCommand, 2);
            RegisterCommand("ahunttrick", "(.+)", AutoHuntTrickCommand, 2);
            RegisterCommand("hunt", "(.+)", HuntCommand, 2);
            RegisterCommand("astop", "", AutoStopCommand, 2);

            RegisterTrigger("gq.quit", @"@wYou are no longer part of the current quest.", TriggerGQLevel2, TriggerFlags.NotRegex);
            RegisterTrigger("hunt.target", @"@wYou seem unable to hunt that target for some reason.", TriggerHuntT, TriggerFlags.NotRegex);
            RegisterTrigger("hunt.fail1", @"@wNo one in this area by that name.", TriggerHuntFail, TriggerFlags.NotRegex);
            RegisterTrigger("hunt.fail2", @"^@wYou couldn't find a path to .+from here\.$", TriggerHuntNT);
            RegisterTrigger("hunt.fail3", @"(.+?)is here!$", TriggerHuntFail);
            RegisterTrigger("hunt.nottarget", @"^@wYou are confident that .+passed through here, heading \w+\.$", TriggerHuntNT);
            RegisterTrigger("hunt.nottarget2", @"^@wYou have no idea what you're doing, but maybe .+left .+\?$", TriggerHuntNT);
            RegisterTrigger("hunt.nottarget3", @"^@wYou are almost certain that .+is .+from here\.$", TriggerHuntNT);
            RegisterTrigger("hunt.nottarget4", @"^@wThe trail of .+is confusing, but you're reasonable sure .+headed .+\.$", TriggerHuntNT);
            RegisterTrigger("hunt.nottarget5", @"^@wThere are traces of .+having been here\. Perhaps they lead .+\?$", TriggerHuntNT);
            RegisterTrigger("hunt.nottarget6", @"^@wYou are certain that .+is .+from here\.$", TriggerHuntNT);
            RegisterTrigger("where.fail1", @"@wThere are too many doors and fences to see who is in this area.", TriggerWhereFail, TriggerFlags.NotRegex);
            RegisterTrigger("where.fail2", @"^@wThere is no .+around here\.$", TriggerWhereFail);
            RegisterTrigger("quest0", @"^\$gmcp\.comm\.quest\.action start$", TriggerQuest0);
            RegisterTrigger("quest1", @"^\$gmcp\.comm\.quest\.room (.+)", TriggerQuest2);
            RegisterTrigger("quest2", @"^\$gmcp\.comm\.quest\.area (.+)", TriggerQuest3);
            RegisterTrigger("quest3", @"^\$gmcp\.comm\.quest\.targ (.+)", TriggerQuest1);
            RegisterTrigger("enterroom", @"^\$gmcp\.room\.info\.num (-?\d+)$", TriggerRoomNum);
            RegisterTrigger("check", @"^@wYou still have to kill (@c\d @w)?\* (.+?) (@w)?\((.+?)(@w)?\)$", TriggerCheck);
            RegisterTrigger("char.level", @"^\$gmcp\.char\.status\.level (\d+)$", TriggerLevel);
            RegisterTrigger("char.tnl", @"^\$gmcp\.char\.status\.tnl (\d+)$", TriggerTNL);
            RegisterTrigger("request", @"^@.Commander Barcett tells you 'Good luck in your campaign!'$", TriggerRequest);
            RegisterTrigger("cplevel", @"^@cLevel Taken\.\.\.\.\.\.\.\.: @g\[ \s+@w(\d+) @g\]$", TriggerCPLevel);
            RegisterTrigger("gqlevel", @"^@RGlobal Quest@Y: @gGlobal quest # \d+ @whas been declared for levels @g(\d+) @wto @g(\d+)@w\.$", TriggerGQLevel);
            RegisterTrigger("gqlevel2", @"^@RGlobal Quest@Y: @wThe global quest has been won by @Y\w+ @w- @Y\d+(st|th|rd|nd) @wwin\.$", TriggerGQLevel2);
            RegisterTrigger("noexp1", @"@RYou will no longer receive experience. Happy questing!", NoexpTrigger1, TriggerFlags.NotRegex);
            RegisterTrigger("noexp2", @"@wYou will now receive experience. Happy leveling!", NoexpTrigger2, TriggerFlags.NotRegex);
            RegisterTrigger("cpcomplete", @"@GCONGRATULATIONS! @wYou have completed your campaign.", TriggerCPComplete, TriggerFlags.NotRegex);
            RegisterTrigger("cpfail", "@wCampaign cleared.", TriggerCPComplete, TriggerFlags.NotRegex);
            RegisterTrigger("scan1", "@w{scan}", TriggerScan1, TriggerFlags.NotRegex);
            RegisterTrigger("scan0", "@w{/scan}", TriggerScan0, TriggerFlags.NotRegex);
            RegisterTrigger("scan2", "^     @w- (.+)", TriggerScan2);
            RegisterTrigger("scan.room", @"^@C(\d )?(North|East|West|South|Down|Up) from here you see:", TriggerScanRoom);
            RegisterTrigger("scan.room2", @"^@CRight here you see:", TriggerScanRoom2);
            RegisterTrigger("enemy", @"^$gmcp\.char\.status\.enemy(.*)", TriggerCurrentEnemy);
            RegisterTrigger("cp.kill", @"@WCongratulations, that was one of your CAMPAIGN mobs!", TriggerOnKilled1, TriggerFlags.NotRegex);
            RegisterTrigger("gq.kill", @"@RCongratulations, that was one of the GLOBAL QUEST mobs!", TriggerOnKilled0, TriggerFlags.NotRegex);
            RegisterTrigger("room.area", @"^\$gmcp\.room\.info\.zone (.*)$", TriggerRoomInfoArea);
            RegisterTrigger("where", @"^(.{28}) (.+)", TriggerWhere, TriggerFlags.NonAnsi);
        }

        private uint CurrentRoomId = uint.MaxValue;
        private bool AllowScan = false;
        private int MyLvl = 0;
        private int MyXP = 99999;
        private int CampaignLvl = 0;
        private int GQLvl = 0;
        private long Spam = 0;
        private bool IsNoexp = false;
        private int AutoNoexp = 0;
        private bool didEnter = false;
        private readonly List<CPEntry>[] Targets = new[] { new List<CPEntry>(), new List<CPEntry>() };
        private string CurEnemy = "";
        private string RoomInfoArea = "";
        private long WhereTimeout = 0;
        private long HuntTimeout = 0;
        private readonly List<uint> WhereRooms = new List<uint>();
        private uint FirstWhereRoom = uint.MaxValue;
        private readonly List<uint> ScanRooms = new List<uint>();
        private uint CurScanRoom = uint.MaxValue;
        private string QuestTarget = string.Empty;
        private string QuestRoom = string.Empty;
        private string QuestArea = string.Empty;
        private bool QuestListen = false;

        private int AutoWhereNth = 1;
        private string AutoWhereKey = string.Empty;
        private int AutoHuntNth = 1;
        private string AutoHuntKey = string.Empty;

        private bool TriggerHuntT(TriggerData t)
        {
            World.Instance.Execute("where " + AutoHuntNth + "." + AutoHuntKey, true);
            AutoHuntKey = string.Empty;
            AutoHuntNth = 1;
            return false;
        }

        private bool TriggerHuntNT(TriggerData t)
        {
            HuntTimeout = 0;
            AutoHuntNth++;
            return false;
        }

        private bool TriggerWhereFail(TriggerData t)
        {
            AutoWhereKey = string.Empty;
            AutoWhereNth = 1;
            WhereTimeout = 0;
            return false;
        }

        private bool TriggerHuntFail(TriggerData t)
        {
            AutoHuntKey = string.Empty;
            AutoHuntNth = 1;
            HuntTimeout = 0;
            return false;
        }

        private bool AutoStopCommand(InputData i)
        {
            AutoWhereKey = string.Empty;
            AutoHuntKey = string.Empty;
            return true;
        }

        private bool AutoWhereCommand(InputData i)
        {
            string arg;
            if(!i.Arguments.Success || (arg = i.Arguments.Groups[1].Value.Trim()).Length == 0)
            {
                World.Instance.SendMessage("@wSyntax: awhere <mob keyword>", i.ClientMask);
                return true;
            }

            WhereTimeout = 0;
            Match m = _argRegex.Match(arg);
            if(m.Success)
            {
                AutoWhereKey = m.Groups[2].Value;
                if(!int.TryParse(m.Groups[1].Value, out AutoWhereNth))
                    AutoWhereNth = 1;
            }
            else
            {
                AutoWhereNth = 1;
                AutoWhereKey = arg;
            }
            return true;
        }

        private bool AutoHuntTrickCommand(InputData i)
        {
            string arg;
            if(!i.Arguments.Success || (arg = i.Arguments.Groups[1].Value.Trim()).Length == 0)
            {
                World.Instance.SendMessage("@wSyntax: ahunttrick <mob keyword>", i.ClientMask);
                return true;
            }

            HuntTimeout = 0;
            Match m = _argRegex.Match(arg);
            if(m.Success)
            {
                AutoHuntKey = m.Groups[2].Value;
                if(!int.TryParse(m.Groups[1].Value, out AutoHuntNth))
                    AutoHuntNth = 1;
            }
            else
            {
                AutoHuntNth = 1;
                AutoHuntKey = arg;
            }
            return true;
        }

        private static Regex _argRegex = new Regex(@"^(\d+)\.(.+)", RegexOptions.Compiled);

        private bool TriggerQuest0(TriggerData t)
        {
            QuestListen = true;
            return false;
        }

        private bool TriggerQuest1(TriggerData t)
        {
            if(!QuestListen)
                return false;

            QuestTarget = MobDB.MobDB.NormalizeName(t.Match.Groups[1].Value.Trim());
            return false;
        }

        private bool TriggerQuest2(TriggerData t)
        {
            if(!QuestListen)
                return false;

            QuestRoom = t.Match.Groups[1].Value.Trim();
            return false;
        }

        private bool TriggerQuest3(TriggerData t)
        {
            if(!QuestListen)
                return false;

            QuestListen = false;
            QuestArea = t.Match.Groups[1].Value.Trim();

            int minLevel = MyLvl - 8;
            int maxLevel = MyLvl + 10;
            if(MyLvl >= 200)
                maxLevel = MyLvl + 20;

            uint bestMobRoom = uint.MaxValue;
            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
            List<string> Areas = new List<string>();
            List<uint> Rooms = new List<uint>();
            foreach(Area x in map.Areas)
            {
                if(x.Name == QuestArea)
                    Areas.Add(x.Keyword);
            }
            foreach(Room r in map.Rooms)
            {
                if(r.Area.Name != QuestArea)
                    continue;

                if(r.Name != QuestRoom)
                    continue;

                Rooms.Add(r.Entry);
            }
            foreach(Mob m in db.Mobs)
            {
                if(m.Level < minLevel || m.Level > maxLevel)
                    continue;

                if(!m.Name.Contains(QuestTarget))
                    continue;

                if(!m.Areas.Contains("all"))
                {
                    bool f = false;
                    foreach(string x in m.Areas)
                    {
                        if(Areas.Contains(x))
                        {
                            f = true;
                            break;
                        }
                    }

                    if(!f)
                        continue;
                }

                bestMobRoom = m.GetBestRoom();
                if(bestMobRoom == uint.MaxValue)
                    continue;

                Room br = map.GetRoom(bestMobRoom);
                if(br != null)
                {
                    Pathfinder_Mob pf = new Pathfinder_Mob(m, Rooms.ToArray());
                    pf.StartRooms = new[] { br };
                    PathfindResult pr = map.Get(pf);
                    if(!pr.Success || pr.Target == null)
                    {
                        bestMobRoom = uint.MaxValue;
                        continue;
                    }

                    bestMobRoom = pr.Target.Entry;
                    break;
                }
            }


            if(Rooms.Count != 0)
            {
                WhereRooms.Clear();
                WhereRooms.AddRange(Rooms);
                FirstWhereRoom = bestMobRoom;
                World.Instance.SendMessage("@wAdded @C" + Rooms.Count + " @wroom" + (Rooms.Count != 1 ? "s" : "") + " to @G\"where\" @wlist. Use '@Wgowhere@w' to visit them.");
            }

            return false;
        }

        private bool TriggerScanRoom(TriggerData t)
        {
            CurScanRoom = uint.MaxValue;
            int c = 1;
            if(t.Match.Groups[1].Length != 0)
                int.TryParse(t.Match.Groups[1].Value, out c);
            char d = t.Match.Groups[2].Value.ToLower()[0];

            Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
            if(map == null)
                return false;

            Room r = map.GetRoom(CurrentRoomId);
            if(r == null)
                return false;

            for(int i = 0; i < c; i++)
            {
                Exit e = r.GetExit(d);
                if(e == null)
                    return false;

                r = e.To;
            }

            if(string.IsNullOrEmpty(r.Name))
                return false;

            CurScanRoom = r.Entry;
            return false;
        }

        private bool TriggerScanRoom2(TriggerData t)
        {
            CurScanRoom = CurrentRoomId;
            return false;
        }

        private bool GQCheckCommand(InputData i)
        {
            if(i.Arguments.Success && i.Arguments.Groups[1].Length <= 5 &&
               "check".StartsWith(i.Arguments.Groups[1].Value.ToLower()))
                Targets[0].Clear();
            return false;
        }

        private bool CampaignCheckCommand(InputData i)
        {
            if(i.Arguments.Success && i.Arguments.Groups[1].Length <= 5 &&
               "check".StartsWith(i.Arguments.Groups[1].Value.ToLower()))
                Targets[1].Clear();
            return false;
        }

        private bool TriggerRoomNum(TriggerData t)
        {
            uint u;
            if(uint.TryParse(t.Match.Groups[1].Value, out u))
            {
                CurrentRoomId = u;
                if(FirstWhereRoom == u)
                    FirstWhereRoom = uint.MaxValue;
                if(WhereRooms.Remove(u))
                    World.Instance.SendMessage("@wEntered a @G\"where\" @wroom. Now have @C" + WhereRooms.Count + " @wroom" + (WhereRooms.Count != 1 ? "s" : "") + " remaining. Use '@Wgowhere@w' to go to the next one.");
            }
            else
                CurrentRoomId = uint.MaxValue;
            return false;
        }

        private bool TriggerWhere(TriggerData t)
        {
            if(WhereTimeout < World.Instance.MSTime)
                return false;

            Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
            if(map == null)
                return false;

            string rName = t.Match.Groups[2].Value.Trim();

            List<Room> r = new List<Room>();
            foreach(Room x in map.Rooms)
            {
                if(x.Area.Keyword == RoomInfoArea && x.Name == rName)
                    r.Add(x);
            }

            string mobName = t.Match.Groups[1].Value.Trim();
            mobName = MobDB.MobDB.NormalizeName(mobName);

            if(!string.IsNullOrEmpty(AutoWhereKey))
            {
                if(Targets[0].Count != 0)
                {
                    foreach(CPEntry x in Targets[0])
                    {
                        if(x.Name.StartsWith(mobName))
                        {
                            AutoWhereKey = string.Empty;
                            break;
                        }
                    }
                }
                else
                {
                    foreach(CPEntry x in Targets[1])
                    {
                        if(x.Name.StartsWith(mobName))
                        {
                            AutoWhereKey = string.Empty;
                            break;
                        }
                    }

                    if(QuestTarget.StartsWith(mobName))
                        AutoWhereKey = string.Empty;
                }
            }

            if(r.Count == 0)
                return false;

            WhereRooms.Clear();
            FirstWhereRoom = uint.MaxValue;
            WhereTimeout = 0;
            foreach(Room x in r)
            {
                if(CurrentRoomId != x.Entry)
                    WhereRooms.Add(x.Entry);
            }

            if(WhereRooms.Count == 0)
                return false;

            World.Instance.SendMessage("@wAdded @C" + WhereRooms.Count + " @wroom" + (WhereRooms.Count != 1 ? "s" : "") + " to @G\"where\"@w. Type '@Wgowhere@w' to go there.");
            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            if(db == null)
                return false;

            foreach(Mob m in db.Mobs)
            {
                if(!m.Areas.Contains("all") && !m.Areas.Contains(RoomInfoArea))
                    continue;

                bool f = false;
                foreach(string z in m.Name)
                {
                    if(mobName.Length >= 26)
                    {
                        if(z.StartsWith(mobName))
                        {
                            f = true;
                            break;
                        }
                    }
                    else if(z == mobName)
                    {
                        f = true;
                        break;
                    }
                }

                if(!f)
                    continue;

                uint b = m.GetBestRoom();
                if(b == uint.MaxValue || b == CurrentRoomId)
                    continue;

                Room br = map.GetRoom(b);
                if(br == null || br.Entry == CurrentRoomId)
                    continue;

                if(WhereRooms.Contains(b))
                    FirstWhereRoom = b;
                else
                {
                    Pathfinder_Mob pf = new Pathfinder_Mob(m, WhereRooms.ToArray());
                    pf.StartRooms = new[] { br };

                    PathfindResult pr = map.Get(pf);
                    if(pr.Success && pr.Target != null)
                        FirstWhereRoom = pr.Target.Entry;
                }

                break;
            }

            return false;
        }

        private bool WhereCommand(InputData i)
        {
            if(i.Arguments.Success && i.Arguments.Groups[1].Value.Trim().Length > 0)
                WhereTimeout = World.Instance.MSTime + 6000;
            return false;
        }

        private bool HuntCommand(InputData i)
        {
            if(i.Arguments.Success && i.Arguments.Groups[1].Value.Trim().Length > 0)
                HuntTimeout = World.Instance.MSTime + 6000;
            return false;
        }

        private bool GoWhereCommand(InputData i)
        {
            if(WhereRooms.Count == 0)
                World.Instance.SendMessage("@wThere are no where rooms available.", i.ClientMask);
            else
            {
                Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
                if(map == null)
                {
                    World.Instance.SendMessage("@wInternal error.", i.ClientMask);
                    return true;
                }

                Pathfinder_Entry pf = new Pathfinder_Entry(FirstWhereRoom != uint.MaxValue ? new[] { FirstWhereRoom } : WhereRooms.ToArray());
                map.FillPathFinder(pf);
                PathfindResult pr = map.Get(pf);
                if(!pr.Success)
                    World.Instance.SendMessage("@wCouldn't find a path to any of the where rooms.", i.ClientMask);
                else
                    map.Goto(pr);
            }
            return true;
        }

        public override void OnLogin()
        {
            base.OnLogin();

            RegisterCommand("goto", @"(.+)", GotoMobCommand, 2, CMDFlags.None, "mobdb");
        }

        private bool GotoMobCommand(InputData i)
        {
            if(string.IsNullOrEmpty(RoomInfoArea))
            {
                World.Instance.SendMessage("@wDon't know yet which area we are in. Type look to find out.", i.ClientMask);
                return true;
            }

            if(!i.Arguments.Success)
            {
                World.Instance.SendMessage("@wSyntax: mobdb goto <mob partial name>", i.ClientMask);
                return true;
            }

            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            if(db == null)
            {
                World.Instance.SendMessage("@wInternal error.", i.ClientMask);
                return true;
            }

            List<uint> toGoRooms = new List<uint>();
            string name = i.Arguments.Groups[1].Value.ToLower();
            foreach(Mob m in db.Mobs)
            {
                if(!m.Areas.Contains("all") && !m.Areas.Contains(RoomInfoArea))
                    continue;

                uint p = m.GetBestRoom();
                if(p == uint.MaxValue)
                    continue;

                foreach(string z in m.Name)
                {
                    if(z.ToLower().Contains(name))
                    {
                        if(!toGoRooms.Contains(p))
                            toGoRooms.Add(p);
                        break;
                    }
                }
            }

            if(toGoRooms.Count == 0)
            {
                World.Instance.SendMessage("@wFound no mobs with that name in current area.", i.ClientMask);
                return true;
            }

            Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
            if(map == null)
            {
                World.Instance.SendMessage("@wInternal error.", i.ClientMask);
                return true;
            }

            Pathfinder_Entry pf = new Pathfinder_Entry(toGoRooms.ToArray());
            if(!map.FillPathFinder(pf))
            {
                World.Instance.SendMessage("@wWe are in an unknown room.", i.ClientMask);
                return true;
            }

            PathfindResult pr = map.Get(pf);
            if(!pr.Success)
            {
                World.Instance.SendMessage("@wCouldn't find a path to any of the mobs with that name.", i.ClientMask);
                return true;
            }

            map.Goto(pr);
            return true;
        }

        private bool TriggerRoomInfoArea(TriggerData t)
        {
            RoomInfoArea = t.Match.Groups[1].Value.Trim();
            return false;
        }

        private bool TriggerOnKilled0(TriggerData t)
        {
            if(string.IsNullOrEmpty(CurEnemy))
                return false;

            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            if(db == null)
                return false;

            CurEnemy = MobDB.MobDB.NormalizeName(CurEnemy);

            for(int i = 0; i < Targets[0].Count; i++)
            {
                CPEntry x = Targets[0][i];

                if(x.Name != CurEnemy)
                    continue;

                foreach(uint u in x.Mobs)
                {
                    Mob m = db.GetMob(u);
                    if(m == null)
                        continue;

                    if(!m.Areas.Contains("all") && !m.Areas.Contains(RoomInfoArea))
                        continue;

                    if(m.Name.Contains(CurEnemy))
                    {
                        if(x.Count > 1)
                        {
                            x.Count--;
                            return false;
                        }

                        Targets[0].RemoveAt(i);
                        UpdateColors();
                        return false;
                    }
                }
            }

            return false;
        }

        private bool TriggerOnKilled1(TriggerData t)
        {
            if(string.IsNullOrEmpty(CurEnemy))
                return false;

            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            if(db == null)
                return false;

            CurEnemy = MobDB.MobDB.NormalizeName(CurEnemy);

            for(int i = 0; i < Targets[1].Count; i++)
            {
                CPEntry x = Targets[1][i];

                if(x.Name != CurEnemy)
                    continue;

                foreach(uint u in x.Mobs)
                {
                    Mob m = db.GetMob(u);
                    if(m == null)
                        continue;

                    if(!m.Areas.Contains("all") && !m.Areas.Contains(RoomInfoArea))
                        continue;

                    if(m.Name.Contains(CurEnemy))
                    {
                        if(x.Count > 1)
                        {
                            x.Count--;
                            return false;
                        }

                        Targets[1].RemoveAt(i);
                        UpdateColors();
                        return false;
                    }
                }
            }

            return false;
        }

        private bool TriggerCurrentEnemy(TriggerData t)
        {
            CurEnemy = Colors.RemoveColors(t.Match.Groups[1].Value, false).Trim();
            return false;
        }

        private bool TriggerScan0(TriggerData t)
        {
            AllowScan = false;
            if(Config.GetInt32("Tags.Scan.Gag", 1) != 0)
                t.Msg.AuthMask = 0;

            StringBuilder strScan = new StringBuilder();
            foreach(uint u in ScanRooms)
            {
                if(strScan.Length > 0)
                    strScan.Append(" ");
                strScan.Append(u);
            }
            World.Instance.SendMessage("@w{scan_rooms}" + strScan);
            return false;
        }

        private bool TriggerScan1(TriggerData t)
        {
            ScanRooms.Clear();
            AllowScan = true;
            if(Config.GetInt32("Tags.Scan.Gag", 1) != 0)
                t.Msg.AuthMask = 0;
            return false;
        }

        private bool TriggerScan2(TriggerData td)
        {
            if(!AllowScan)
                return false;

            if(Targets[0].Count == 0 && Targets[1].Count == 0)
                return false;

            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            if(db == null)
                return false;

            string t = Colors.RemoveDuplicateColors("@w" + td.Match.Groups[1].Value);
            if(t.StartsWith("@R(Wounded)"))
                t = Colors.RemoveDuplicateColors("@R" + t.Substring("@R(Wounded)".Length)).Trim();

            if(t.StartsWith("@w(Invis)"))
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(Invis)".Length)).Trim();
            else if(t.StartsWith("@w(I)"))
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(I)".Length)).Trim();

            if(t.StartsWith("@w(Hidden)"))
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(Hidden)".Length)).Trim();
            else if(t.StartsWith("@w(H)"))
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(H)".Length)).Trim();

            if(t.StartsWith("@W(Translucent)"))
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(Translucent)".Length)).Trim();
            else if(t.StartsWith("@W(T)"))
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(T)".Length)).Trim();

            if(t.StartsWith("@C(Charmed) "))
                return false;
            else if(t.StartsWith("@C(C)"))
                return false;

            if(t.StartsWith("@M(Animated) "))
                return false;
            else if(t.StartsWith("@M(A)"))
                return false;

            if(t.StartsWith("@B(Diseased)"))
                t = Colors.RemoveDuplicateColors("@B" + t.Substring("@B(Diseased)".Length)).Trim();
            else if(t.StartsWith("@B(D)"))
                t = Colors.RemoveDuplicateColors("@B" + t.Substring("@B(D)".Length)).Trim();

            if(t.StartsWith("@D(Marked)"))
                t = Colors.RemoveDuplicateColors("@D" + t.Substring("@D(Marked)".Length)).Trim();
            else if(t.StartsWith("@D(X)"))
                t = Colors.RemoveDuplicateColors("@D" + t.Substring("@D(X)".Length)).Trim();

            if(t.StartsWith("@r(Red Aura)"))
                t = Colors.RemoveDuplicateColors("@r" + t.Substring("@r(Red Aura)".Length)).Trim();
            else if(t.StartsWith("@r(R)"))
                t = Colors.RemoveDuplicateColors("@r" + t.Substring("@r(R)".Length)).Trim();
            else if(t.StartsWith("@y(Golden Aura)"))
                t = Colors.RemoveDuplicateColors("@y" + t.Substring("@y(Golden Aura)".Length)).Trim();
            else if(t.StartsWith("@y(G)"))
                t = Colors.RemoveDuplicateColors("@y" + t.Substring("@y(G)".Length)).Trim();

            if(t.StartsWith("@W(White Aura)"))
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(White Aura)".Length)).Trim();
            else if(t.StartsWith("@W(W)"))
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(W)".Length)).Trim();

            if(t.StartsWith("@R(Angry)"))
                t = Colors.RemoveDuplicateColors("@R" + t.Substring("@R(Angry)".Length)).Trim();

            if(t.StartsWith("@w(Linkdead)"))
                return false;

            if(t.StartsWith("@R(RAIDER)"))
                return false;

            if(t.StartsWith("@R(TRAITOR)"))
                return false;

            if(t.StartsWith("@G(DEFENDER)"))
                return false;

            if(t.StartsWith("@R(WANTED)"))
                return false;

            Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
            if(map == null)
                return false;

            string n = Colors.RemoveColors(t, false).ToLower().Trim();
            if(t.StartsWith("@") && !t.StartsWith("@@") && t.Length >= 2)
                t = t.Substring(2);
            if(Targets[0].Count != 0)
            {
                foreach(CPEntry x in Targets[0])
                {
                    if(x.Name.ToLower() != n)
                        continue;

                    int ind = td.Msg.Msg.LastIndexOf(t);
                    if(ind != -1)
                    {
                        td.Msg.Msg = td.Msg.Msg.Remove(ind);
                        td.Msg.Msg = td.Msg.Msg.Insert(ind, Config.GetString("Color.Scan", "@R") + x.Name);
                    }

                    if(CurScanRoom != uint.MaxValue && !ScanRooms.Contains(CurScanRoom))
                        ScanRooms.Add(CurScanRoom);

                    return false;
                }
            }
            else
            {
                foreach(CPEntry x in Targets[1])
                {
                    if(x.Name.ToLower() == n)
                    {
                        int ind = td.Msg.Msg.LastIndexOf(t);
                        if(ind != -1)
                        {
                            td.Msg.Msg = td.Msg.Msg.Remove(ind);
                            td.Msg.Msg = td.Msg.Msg.Insert(ind, Config.GetString("Color.Scan", "@R") + x.Name);
                        }

                        if(CurScanRoom != uint.MaxValue && !ScanRooms.Contains(CurScanRoom))
                            ScanRooms.Add(CurScanRoom);

                        return false;
                    }
                }
            }

            return false;
        }

        private bool TriggerCPComplete(TriggerData t)
        {
            Targets[1].Clear();
            UpdateColors();
            return false;
        }

        private bool NoexpTrigger1(TriggerData t)
        {
            IsNoexp = true;
            didEnter = false;
            return false;
        }

        private bool NoexpTrigger2(TriggerData t)
        {
            IsNoexp = false;
            didEnter = false;
            return false;
        }

        public override void Update(long msTime)
        {
            base.Update(msTime);

            if(!string.IsNullOrEmpty(AutoWhereKey))
            {
                if(WhereTimeout > msTime)
                    return;
                if(WhereTimeout != 0)
                    AutoWhereKey = string.Empty;
                else
                {
                    World.Instance.Execute("where " + AutoWhereNth + "." + AutoWhereKey, true);
                    AutoWhereNth++;
                    return;
                }
            }

            if(!string.IsNullOrEmpty(AutoHuntKey))
            {
                if(HuntTimeout > msTime)
                    return;
                if(HuntTimeout != 0)
                    AutoHuntKey = string.Empty;
                else
                {
                    World.Instance.Execute("hunt " + AutoHuntNth + "." + AutoHuntKey, true);
                    return;
                }
            }

            if(didEnter)
                return;

            if(AutoNoexp == 0)
                return;

            bool shouldNoExp = MyLvl > CampaignLvl && MyXP <= AutoNoexp;
            if(shouldNoExp)
            {
                if(IsNoexp)
                    return;
            }
            else
            {
                if(!IsNoexp)
                    return;
            }

            World.Instance.Execute("noexp", true);
            didEnter = true;
        }

        private bool AutoNoexpCommand(InputData t)
        {
            int n;
            if(!t.Arguments.Success || !int.TryParse(t.Arguments.Groups[1].Value, out n))
            {
                World.Instance.SendMessage("@wSyntax: autonoexp <XP amount>", t.ClientMask);
                return true;
            }

            if(n <= 0)
            {
                AutoNoexp = 0;
                World.Instance.SendMessage("@wYou will no longer turn noexp on automatically.", t.ClientMask);
                return true;
            }

            AutoNoexp = n;
            World.Instance.SendMessage("@wYou will now turn noexp on when you can ask a campaign and XP goes to @C" + n + " @wor below.", t.ClientMask);
            World.Instance.SendMessage("@wUse '@Wautonoexp 0@w' to turn this off.", t.ClientMask);
            return true;
        }

        private bool TriggerGQLevel(TriggerData t)
        {
            Targets[0].Clear();
            int lvl;
            if(int.TryParse(t.Match.Groups[1].Value, out lvl))
                GQLvl = lvl;
            UpdateColors();
            return false;
        }

        private bool TriggerGQLevel2(TriggerData t)
        {
            Targets[0].Clear();
            GQLvl = 0;
            UpdateColors();
            return false;
        }

        private bool TriggerCPLevel(TriggerData t)
        {
            int lvl;
            if(int.TryParse(t.Match.Groups[1].Value, out lvl))
                CampaignLvl = lvl;
            return false;
        }

        private bool TriggerRequest(TriggerData t)
        {
            CampaignLvl = MyLvl;
            return false;
        }

        private bool TriggerLevel(TriggerData t)
        {
            int lvl;
            if(int.TryParse(t.Match.Groups[1].Value, out lvl))
                MyLvl = lvl;
            return false;
        }

        private bool TriggerTNL(TriggerData t)
        {
            int lvl;
            if(int.TryParse(t.Match.Groups[1].Value, out lvl))
                MyXP = lvl;
            return false;
        }

        private bool TriggerCheck(TriggerData t)
        {
            string mobName = MobDB.MobDB.NormalizeName(Colors.RemoveColors(t.Match.Groups[2].Value, false).Trim());
            string placeName = Colors.RemoveColors(t.Match.Groups[4].Value, false).Trim();
            if(placeName.EndsWith(" - Dead"))
                placeName = placeName.Substring(0, placeName.LastIndexOf(" - Dead"));

            if(Config.GetInt32("Tags.On", 1) != 0)
            {
                World.Instance.SendMessage("@w{cp_mob}" + mobName);
                World.Instance.SendMessage("@w{cp_place}" + placeName);
            }

            if(CampaignLvl == 0 && t.Match.Groups[1].Length == 0)
            {
                if(Spam < World.Instance.MSTime)
                {
                    Spam = World.Instance.MSTime + 300;
                    World.Instance.SendMessage("@WPlease see '@Rcampaign info@W' first! I need the level to check for mobs.");
                }
                return false;
            }

            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
            if(db == null || map == null)
                return false;

            int minLevel = CampaignLvl - 5;
            int maxLevel = CampaignLvl + 12;
            if(CampaignLvl >= 200)
                maxLevel = CampaignLvl + 20;

            if(t.Match.Groups[1].Length != 0)
            {
                minLevel = MyLvl - 20;
                maxLevel = MyLvl + 10;
                if(GQLvl != 0)
                {
                    minLevel = GQLvl - 6;
                    maxLevel = GQLvl + 15;
                    if(GQLvl >= 186)
                        maxLevel = GQLvl + 21;
                }
            }

            List<uint> Rooms = new List<uint>();
            List<string> Areas = new List<string>();
            foreach(Area z in map.Areas)
            {
                if(z.Name == placeName)
                    Areas.Add(z.Keyword);
            }

            CPEntry e = new CPEntry();
            e.Name = mobName;
            e.PlaceName = placeName;

            if(Areas.Count != 0)
            {
                // Area CP / GQ?
                foreach(string a in Areas)
                {
                    foreach(Mob x in db.Mobs)
                    {
                        if(x.Level < minLevel || x.Level > maxLevel)
                            continue;
                        if(!x.Areas.Contains("all") && !x.Areas.Contains(a))
                            continue;

                        if(!x.Name.Contains(mobName))
                            continue;

                        uint roomId = x.GetBestRoom();
                        if(roomId == uint.MaxValue)
                            continue;

                        Rooms.Add(roomId);
                        if(!e.Mobs.Contains(x.Entry))
                            e.Mobs.Add(x.Entry);
                    }
                }
            }
            else
            {
                List<uint> r = new List<uint>();
                List<string> p = new List<string>();
                foreach(Room x in map.Rooms)
                {
                    if(x.Name == placeName)
                    {
                        r.Add(x.Entry);
                        if(!p.Contains(x.Area.Keyword))
                            p.Add(x.Area.Keyword);
                    }
                }

                foreach(Mob x in db.Mobs)
                {
                    if(x.Level < minLevel || x.Level > maxLevel)
                        continue;
                    if(!x.Areas.Contains("all"))
                    {
                        bool f = false;
                        foreach(string z in x.Areas)
                        {
                            if(p.Contains(z))
                            {
                                f = true;
                                break;
                            }
                        }

                        if(!f)
                            continue;
                    }

                    if(!x.Name.Contains(mobName))
                        continue;

                    if(!e.Mobs.Contains(x.Entry))
                        e.Mobs.Add(x.Entry);

                    uint u = x.GetBestRoom();
                    if(u == uint.MaxValue)
                        continue;

                    Room br = map.GetRoom(u);
                    if(br == null)
                        continue;

                    Pathfinder_Mob pf = new Pathfinder_Mob(x, r.ToArray());
                    pf.StartRooms = new[] { br };
                    PathfindResult pr = map.Get(pf);
                    if(pr.Success && pr.Target != null)
                        u = pr.Target.Entry;

                    if(!Rooms.Contains(u))
                        Rooms.Add(u);
                }
            }

            if(t.Match.Groups[1].Length == 0)
                e.Count = 1;
            else
            {
                int n;
                if(int.TryParse(Colors.RemoveColors(t.Match.Groups[1].Value, false).Trim(), out n))
                {
                    e.Count = n;
                    e.IsGQ = true;
                }
                else
                {
                    e.Count = 1;
                    e.IsGQ = true;
                }
            }

            if(e.IsGQ)
                Targets[0].Add(e);
            else
                Targets[1].Add(e);

            StringBuilder strRooms = new StringBuilder();
            foreach(uint u in Rooms)
            {
                if(strRooms.Length > 0)
                    strRooms.Append(" ");
                strRooms.Append(u);
            }
            World.Instance.SendMessage("@w{cp_room}" + strRooms);

            UpdateColors();
            return false;
        }

        private void UpdateColors()
        {
            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            if(db == null)
                return;

            db.ClearMobColors();
            if(string.IsNullOrEmpty(Config.GetString("Color.Room", "@C")))
                return;

            if(Targets[0].Count != 0)
            {
                foreach(CPEntry x in Targets[0])
                {
                    foreach(uint u in x.Mobs)
                        db.SetMobColor(u, Config.GetString("Color.Room", "@C"));
                }
            }
            else
            {
                foreach(CPEntry x in Targets[1])
                {
                    foreach(uint u in x.Mobs)
                        db.SetMobColor(u, Config.GetString("Color.Room", "@C"));
                }
            }
        }

        private CPEntry IsSame(List<CPEntry> entries, string mobName, string placeName)
        {
            Mapper.Mapper map = PluginMgr.GetPlugin("mapper") as Mapper.Mapper;
            MobDB.MobDB db = PluginMgr.GetPlugin("mobdb") as MobDB.MobDB;
            if(map == null || db == null)
                return null;

            List<string> areas = new List<string>();
            foreach(Area x in map.Areas)
            {
                if(x.Name == placeName)
                    areas.Add(x.Keyword);
            }
            if(areas.Count == 0)
            {
                foreach(Room x in map.Rooms)
                {
                    if(x.Name == placeName && !areas.Contains(x.Area.Keyword))
                        areas.Add(x.Area.Keyword);
                }
            }

            foreach(CPEntry x in entries)
            {
                if(x.Name != mobName)
                    continue;

                foreach(uint u in x.Mobs)
                {
                    Mob m = db.GetMob(u);
                    if(m == null)
                        continue;

                    if(!m.Areas.Contains("all"))
                    {
                        bool f = false;
                        foreach(string y in m.Areas)
                        {
                            if(areas.Contains(y))
                            {
                                f = true;
                                break;
                            }
                        }

                        if(!f)
                            continue;
                    }

                    return x;
                }
            }

            return null;
        }
    }

    public class Pathfinder_Mob : Pathfinder_Entry
    {
        public Pathfinder_Mob(Mob m, params uint[] roomId)
            : base(roomId)
        {
            mob = m;
            CanUsePortals = false;
            CanUseRecalls = false;
            CharacterLevel = m.Level;
            CharacterTier = 0;
            IsGlobalQuest = false;
            IsSingleClassTier0 = false;
        }

        private Mob mob;
        private static string[] Normals = new[] { "n", "e", "s", "w", "u", "d" };

        public override bool CanUseExit(Exit e)
        {
            if(!Normals.Contains(e.Command))
                return false;

            if(!mob.Areas.Contains("all") && !mob.Areas.Contains(e.To.Area.Keyword))
                return false;

            return base.CanUseExit(e);
        }
    }

    public class CPHelperConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("Tags.On", 1, "Display tags on campaign / gq check messages for easier capture. If you turn tags off then room IDs will still be displayed if we find a mob in database.");
            CreateSetting("Tags.Scan.Gag", 1, "Gag {scan} tags.");
            CreateSetting("Color.Room", "@C", "Color campaign / gq mobs differently (the $mob.color value) when we have a CP / GQ mob target. Enter empty value to skip this. Mob has to be in database for this to work.");
            CreateSetting("Color.Scan", "@R", "Color campaign / gq mobs differently when scanning. Enter empty value to skip this. You need to have \"tags scan on\" for this to work. Mob has to be in database for this to work.");
        }
    }

    internal class CPEntry
    {
        internal bool IsGQ = false;
        internal string Name;
        internal int Count;
        internal string PlaceName;
        internal List<uint> Mobs = new List<uint>();
    }
}
