using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProxyCore;
using ProxyCore.Input;
using ProxyCore.Output;
using ProxyCore.Scripting;

namespace MobDB
{
    public class MobDB : Plugin
    {
        public MobDB()
            : base("mobdb", "Mob database")
        {
            Author = "Duckbat";
            Version = 5;
            Description =
                "You can add mobs to database so we can automatically record their locations and later find them when campaigning.";
            UpdateUrl = "www.duckbat.com/plugins/update.mobdb.txt";
            Website = "code.google.com/p/proxymud/";

            RequiredPlayerConfig.Add("tags roomchars on");

            Config = new MobDBConfig();

            RegisterCommand("mobdb", "", CommandMobDB);
            RegisterCommand("add", @"(.+)", CommandAdd, 0, CMDFlags.None, "mobdb");
            RegisterCommand("count", "", CommandCount, 0, CMDFlags.None, "mobdb");
            RegisterCommand("delete", @"(.+)", CommandDelete, 0, CMDFlags.None, "mobdb");
            RegisterCommand("find", @"(exact\s+)?(case\s+)?(.+)", CommandFind, 0, CMDFlags.None, "mobdb");
            RegisterCommand("mobinfo", @"^\s*(\d+)?(\s+.+)?", CommandMobInfo, 3, CMDFlags.None, "mobdb");
            RegisterCommand("save", @"(.+)", CommandSave, 0, CMDFlags.None, "mobdb");
            RegisterCommand("where", @"(.+)", CommandWhere, 2, CMDFlags.None, "mobdb");

            RegisterTrigger("room.id", @"^\$gmcp\.room\.info\.num (-?\d+)$", TriggerRoomInfoNum);
            RegisterTrigger("room.area", @"^\$gmcp\.room\.info\.zone (.*)$", TriggerRoomInfoArea);
            RegisterTrigger("room.chars1", @"^@w\{roomchars\}$", TriggerRoomChars1, TriggerFlags.None, 1500);
            RegisterTrigger("room.chars2", @"^@w\{/roomchars\}$", TriggerRoomChars2, TriggerFlags.None, 500);
            RegisterTrigger("room.chars3", @"(.*)", TriggerRoomChars3);
            RegisterTrigger("lastkills1", "@WName                            Level  Exp  From", TriggerLastKills,
                            TriggerFlags.NotRegex);
            RegisterTrigger("lastkills2", @"^@w(.{30})\s+(\d+)\s+\d+\s+(.+)", TriggerLastKills2);

            Load();
        }

        private string AddMobName;
        private int AddMobLevel;
        private string AddMobKeywords;
        private string AddMobArea;

        private uint CurrentTime;
        private uint RoomInfoEntry = uint.MaxValue;
        private string RoomInfoArea;
        private const string DBFileName = "mobdb.xml";
        private const string DBFileBackup = "mobdb_backup.xml";
        private readonly Dictionary<uint, Mob> IMobs = new Dictionary<uint, Mob>();
        private uint _guidMob = 0;
        private long WhenSave = 0;
        private readonly List<string> UnknownMobs = new List<string>();
        private bool ListeningRoomChars = false;
        private readonly Dictionary<uint, string> overrideColors = new Dictionary<uint, string>();
        private int ChooseFromUnknown = 0;
        private readonly Dictionary<uint, List<MobLocation>> RoomLocations = new Dictionary<uint, List<MobLocation>>();

        /// <summary>
        /// A collection of all mobs.
        /// </summary>
        public IEnumerable<Mob> Mobs
        {
            get
            {
                return IMobs.Values;
            }
        }

        /// <summary>
        /// Normalize mob name. This means first character of name will be made lower case while others are left as is.
        /// </summary>
        /// <param name="mobName">Name of mob.</param>
        /// <returns></returns>
        public static string NormalizeName(string mobName)
        {
            if(mobName == null)
                return string.Empty;

            if(mobName.Length > 0)
                mobName = mobName.Substring(0, 1).ToLower() + mobName.Substring(1);
            return mobName;
        }

        /// <summary>
        /// Override mob colors when replacing with our longname.
        /// </summary>
        /// <param name="Entry">Mob ID.</param>
        /// <param name="colorCode">Color code, for example "@M"</param>
        public void SetMobColor(uint Entry, string colorCode)
        {
            if(string.IsNullOrEmpty(colorCode))
                overrideColors.Remove(Entry);
            else
                overrideColors[Entry] = colorCode;
        }

        /// <summary>
        /// Clear all overridden mob colors.
        /// </summary>
        public void ClearMobColors()
        {
            overrideColors.Clear();
        }

        /// <summary>
        /// Get mob by ID.
        /// </summary>
        /// <param name="Id">ID of mob.</param>
        /// <returns></returns>
        public Mob GetMob(uint Id)
        {
            return IMobs.ContainsKey(Id) ? IMobs[Id] : null;
        }

        private Mob GetMob(ref string t)
        {
            string orig = t;
            t = t.Trim();

            // Mobs can't be afk
            if(t.StartsWith("@w[AFK]"))
            {
                t = orig;
                return null;
            }

            StringBuilder str = new StringBuilder();

            // Unit is wounded
            if(t.StartsWith("@R(Wounded)"))
            {
                t = Colors.RemoveDuplicateColors("@R" + t.Substring("@R(Wounded)".Length)).Trim();
                str.Append("@R(Wounded)");
            }

            if(t.StartsWith("@w(Invis)"))
            {
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(Invis)".Length)).Trim();
                str.Append("@w(Invis) ");
            }
            else if(t.StartsWith("@w(I)"))
            {
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(I)".Length)).Trim();
                str.Append("@w(I)");
            }

            if(t.StartsWith("@w(Hidden)"))
            {
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(Hidden)".Length)).Trim();
                str.Append("@w(Hidden) ");
            }
            else if(t.StartsWith("@w(H)"))
            {
                t = Colors.RemoveDuplicateColors("@w" + t.Substring("@w(H)".Length)).Trim();
                str.Append("@w(H)");
            }

            if(t.StartsWith("@W(Translucent)"))
            {
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(Translucent)".Length)).Trim();
                str.Append("@W(Translucent) ");
            }
            else if(t.StartsWith("@W(T)"))
            {
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(T)".Length)).Trim();
                str.Append("@W(T)");
            }

            if(t.StartsWith("@C(Charmed) "))
            {
                //t = Colors.RemoveDuplicateColors("@C" + t.Substring("@C(Charmed) ".Length));
                t = orig;
                return null;
            }
            else if(t.StartsWith("@C(C)"))
            {
                //t = Colors.RemoveDuplicateColors("@C" + t.Substring("@C(C)".Length));
                t = orig;
                return null;
            }

            if(t.StartsWith("@M(Animated) "))
            {
                //t = Colors.RemoveDuplicateColors("@M" + t.Substring("@M(Animated) ".Length));
                t = orig;
                return null;
            }
            else if(t.StartsWith("@M(A)"))
            {
                //t = Colors.RemoveDuplicateColors("@M" + t.Substring("@M(A)".Length));
                t = orig;
                return null;
            }

            if(t.StartsWith("@B(Diseased)"))
            {
                t = Colors.RemoveDuplicateColors("@B" + t.Substring("@B(Diseased)".Length)).Trim();
                str.Append("@B(Diseased) ");
            }
            else if(t.StartsWith("@B(D)"))
            {
                t = Colors.RemoveDuplicateColors("@B" + t.Substring("@B(D)".Length)).Trim();
                str.Append("@B(D)");
            }

            if(t.StartsWith("@D(Marked)"))
            {
                t = Colors.RemoveDuplicateColors("@D" + t.Substring("@D(Marked)".Length)).Trim();
                str.Append("@D(Marked) ");
            }
            else if(t.StartsWith("@D(X)"))
            {
                t = Colors.RemoveDuplicateColors("@D" + t.Substring("@D(X)".Length)).Trim();
                str.Append("@D(X)");
            }

            if(t.StartsWith("@r(Red Aura)"))
            {
                t = Colors.RemoveDuplicateColors("@r" + t.Substring("@r(Red Aura)".Length)).Trim();
                str.Append("@r(Red Aura)");
            }
            else if(t.StartsWith("@r(R)"))
            {
                t = Colors.RemoveDuplicateColors("@r" + t.Substring("@r(R)".Length)).Trim();
                str.Append("@r(R)");
            }
            else if(t.StartsWith("@y(Golden Aura)"))
            {
                t = Colors.RemoveDuplicateColors("@y" + t.Substring("@y(Golden Aura)".Length)).Trim();
                str.Append("@y(Golden Aura) ");
            }
            else if(t.StartsWith("@y(G)"))
            {
                t = Colors.RemoveDuplicateColors("@y" + t.Substring("@y(G)".Length)).Trim();
                str.Append("@y(G)");
            }

            if(t.StartsWith("@W(White Aura)"))
            {
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(White Aura)".Length)).Trim();
                str.Append("@W(White Aura) ");
            }
            else if(t.StartsWith("@W(W)"))
            {
                t = Colors.RemoveDuplicateColors("@W" + t.Substring("@W(W)".Length)).Trim();
                str.Append("@W(W)");
            }

            if(t.StartsWith("@R(Angry)"))
            {
                t = Colors.RemoveDuplicateColors("@R" + t.Substring("@R(Angry)".Length)).Trim();
                str.Append("@R(Angry) ");
            }

            if(t.StartsWith("@w(Linkdead)"))
            {
                t = orig;
                return null;
            }

            if(t.StartsWith("@R(RAIDER)"))
            {
                t = orig;
                return null;
            }

            if(t.StartsWith("@R(TRAITOR)"))
            {
                t = orig;
                return null;
            }

            if(t.StartsWith("@G(DEFENDER)"))
            {
                t = orig;
                return null;
            }

            if(t.StartsWith("@R(WANTED)"))
            {
                t = orig;
                return null;
            }

            if(str.Length != 0 && str[str.Length - 1] != ' ')
                str.Append(' ');
            str.Append("{MOB}");

            if(t.EndsWith("[TARGET]"))
            {
                t = Colors.RemoveDuplicateColors(t.Substring(0, t.LastIndexOf("[TARGET]"))).Trim();
                str.Append(" @R[TARGET]");
            }

            string area = RoomInfoArea.ToLower();
            foreach(KeyValuePair<uint, Mob> x in IMobs)
            {
                if(x.Value.Areas == null)
                    continue;

                if(!x.Value.Areas.Contains("all") && !x.Value.Areas.Contains(RoomInfoArea))
                    continue;

                if(x.Value.Longname == t)
                {
                    string n = Config.GetString("Mob.Longname",
                                                "@w[@G$mob.level@w] $mob.color$mob.name @D($mob.keywords)");
                    if(string.IsNullOrEmpty(n))
                    {
                        t = orig;
                        return x.Value;
                    }

                    n = n.Replace("$$", "\nE");
                    n = n.Replace("$mob.level", x.Value.Level.ToString());
                    n = n.Replace("$mob.keywords", x.Value.Keyword);
                    if(n.Contains("$mob.name"))
                        n = n.Replace("$mob.name", x.Value.Names);
                    n = n.Replace("$mob.color",
                                  overrideColors.ContainsKey(x.Key)
                                      ? overrideColors[x.Key]
                                      : (!string.IsNullOrEmpty(x.Value.DefaultColor)
                                             ? x.Value.DefaultColor
                                             : Config.GetString("Mob.DefaultColor", "@y")));
                    n = n.Replace("$mob.longname", x.Value.Longname);
                    n = n.Replace("$mob.entry", x.Key.ToString());
                    n = n.Replace("\nE", "$");
                    str.Replace("{MOB}", Colors.RemoveDuplicateColors(n));
                    t = str.ToString();
                    return x.Value;
                }
            }

            if(!UnknownMobs.Contains(t))
                UnknownMobs.Add(t);
            t = orig;
            return null;
        }

        public override void OnEnteredCommandBefore(ref string Msg, uint ClientId, int AuthLevel)
        {
            base.OnEnteredCommandBefore(ref Msg, ClientId, AuthLevel);

            if(ChooseFromUnknown == 2)
            {
                int i;
                if(int.TryParse(Msg, out i) && i >= 1 && i <= UnknownMobs.Count)
                {
                    Msg = null;
                    i--;
                    ChooseFromUnknown = 0;

                    Mob m = new Mob(++_guidMob);
                    m.Name = new[] { AddMobName };
                    m.Longname = UnknownMobs[i];
                    UnknownMobs.RemoveAt(i);
                    m.Level = AddMobLevel;
                    m.Keyword = AddMobKeywords;
                    m.Areas = new[] {AddMobArea};
                    IMobs[m.Entry] = m;

                    if(RoomInfoEntry != uint.MaxValue)
                    {
                        MobLocation ml = new MobLocation();
                        ml.CountSeen = 1;
                        ml.LastVisited = 0;
                        ml.MobEntry = m.Entry;
                        ml.RoomEntry = RoomInfoEntry;
                        ml.TimesSeen = 1;
                        ml.TimesVisited = 1;
                        m.Locations.Add(ml);

                        if(!RoomLocations.ContainsKey(RoomInfoEntry))
                            RoomLocations[RoomInfoEntry] = new List<MobLocation>();
                        RoomLocations[RoomInfoEntry].Add(ml);
                    }

                    World.Instance.SendMessage("@wAdded a new mob to database '@G" + m.Names + " @w' [@Y" + m.Entry +
                                               "@w] and set location to current room.");
                }
            }
        }

        #region Commands
        private bool CommandWhere(InputData i)
        {
            if(!i.Arguments.Success)
            {
                World.Instance.SendMessage("@wSyntax: mobdb where <mob ID>", i.ClientMask);
                World.Instance.SendMessage("@wShows up to @W20 @wbest locations for a mob.", i.ClientMask);
                World.Instance.SendMessage("", i.ClientMask);
                World.Instance.SendMessage("        @wmobdb where <mob partial name>", i.ClientMask);
                World.Instance.SendMessage("@wShows up to @W5 @wbest locations for all mobs that match this name in current area.", i.ClientMask);
                return true;
            }

            uint mobId;
            if(uint.TryParse(i.Arguments.Groups[1].Value, out mobId))
            {
                Mob m = GetMob(mobId);
                if(m == null)
                {
                    World.Instance.SendMessage("@wNo such mob in database (@R" + mobId + "@w).", i.ClientMask);
                    return true;
                }

                if(!ShowLocations(m, 20, i.ClientMask, false))
                    World.Instance.SendMessage("@wDon't know where '@G" + m.Names + "@w' [@Y" + m.Entry + "@w] is.", i.ClientMask);
            }
            else
            {
                string n = i.Arguments.Groups[1].Value.ToLower();
                bool showed = false;
                foreach(KeyValuePair<uint, Mob> x in IMobs)
                {
                    if(!x.Value.Areas.Contains("all") && !x.Value.Areas.Contains(RoomInfoArea))
                        continue;

                    bool f = false;
                    foreach(string z in x.Value.Name)
                    {
                        if(!z.ToLower().Contains(n))
                            continue;

                        f = true;
                        break;
                    }

                    if(!f)
                        continue;

                    showed = ShowLocations(x.Value, 5, i.ClientMask, showed);
                }

                if(!showed)
                    World.Instance.SendMessage("@wFound no mob locations with that name.", i.ClientMask);
            }

            return true;
        }

        private bool ShowLocations(Mob m, int count, uint[] cm, bool isMulti)
        {
            SortedDictionary<double, List<uint>> bl = new SortedDictionary<double, List<uint>>();
            foreach(MobLocation ml in m.Locations)
            {
                double c = m.GetChance(ml.RoomEntry);
                if(!bl.ContainsKey(c))
                    bl[c] = new List<uint>();
                bl[c].Add(ml.RoomEntry);
            }

            StringBuilder strRooms = new StringBuilder();
            bool showed = false;
            IEnumerable<KeyValuePair<double, List<uint>>> x = bl.Reverse();
            foreach(KeyValuePair<double, List<uint>> y in x)
            {
                foreach(uint z in y.Value)
                {
                    if(!showed)
                    {
                        if(isMulti)
                            World.Instance.SendMessage("", cm);
                        World.Instance.SendMessage("@wLocations for '@G" + m.Names + "@w' [@Y" + m.Entry + "@w]:", cm);
                        showed = true;
                    }

                    World.Instance.SendMessage("@Y" + string.Format("{0,-6}", z) + " @C" + string.Format("{0:0.00}", y.Key) + "%", cm);
                    if(strRooms.Length > 0)
                        strRooms.Append(' ');
                    strRooms.Append(z);
                    count--;
                    if(count == 0)
                        break;
                }

                if(count == 0)
                    break;
            }

            if(strRooms.Length > 0)
                World.Instance.SendMessage("@wmap goto " + strRooms);

            return showed;
        }

        private bool CommandMobDB(InputData i)
        {
            World.Instance.SendMessage("@wAvailable mob db commands:", i.ClientMask);
            World.Instance.SendMessage(
                "@Y" + string.Format("{0,-20}", "mobdb add") + " @w- Add a new mob to database.", i.ClientMask);
            World.Instance.SendMessage(
                "@Y" + string.Format("{0,-20}", "mobdb count") + " @w- Shows how many mobs you have in database.",
                i.ClientMask);
            World.Instance.SendMessage(
                "@Y" + string.Format("{0,-20}", "mobdb delete") + " @w- Delete a mob from database.", i.ClientMask);
            World.Instance.SendMessage("@Y" + string.Format("{0,-20}", "mobdb find") + " @w- Find mobs in database.",
                                       i.ClientMask);
            World.Instance.SendMessage(
                "@Y" + string.Format("{0,-20}", "mobdb mobinfo") + " @w- Show information or edit a mob.", i.ClientMask);
            World.Instance.SendMessage(
                "@Y" + string.Format("{0,-20}", "mobdb save") + " @w- Save mob database to file.", i.ClientMask);
            World.Instance.SendMessage(
                "@Y" + string.Format("{0,-20}", "mobdb where") + " @w- Show mob locations.", i.ClientMask);
            return true;
        }

        private bool CommandAdd(InputData i)
        {
            string n;
            if(!i.Arguments.Success || (n = i.Arguments.Groups[1].Value.Trim()).Length == 0)
            {
                World.Instance.SendMessage("@wSyntax: mobdb add <keywords>", i.ClientMask);
                return true;
            }

            if(UnknownMobs.Count == 0)
            {
                World.Instance.SendMessage("@wWe have no mobs with unknown longname in roomlist.", i.ClientMask);
                return true;
            }

            AddMobKeywords = n;
            ChooseFromUnknown = 1;
            World.Instance.Execute("lastkills 1", false);
            return true;
        }

        private bool CommandMobInfo(InputData i)
        {
            if(!i.Arguments.Success || i.Arguments.Groups[1].Length == 0)
            {
                World.Instance.SendMessage("@wSyntax: mobdb mobinfo <ID> [option] [value]", i.ClientMask);
                World.Instance.SendMessage("", i.ClientMask);
                World.Instance.SendMessage("@wAvailable options for mob:", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "addflag") + " @w- Add a flag to mob.",
                                           i.ClientMask);
                World.Instance.SendMessage(
                    "@W" + string.Format("{0,-15}", "removeflag") + " @w- Remove a flag from mob.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "name1") + " @w- Change (short)name of mob.",
                                           i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "name2") + " @w- Change second (short)name of mob.",
                                           i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "name3") + " @w- Change third (short)name of mob. (This goes all the way up to 999).",
                                           i.ClientMask);
                World.Instance.SendMessage(
                    "@W" + string.Format("{0,-15}", "areas") +
                    " @w- Set allowed areas for mob. Separate with space, enter all to allow all areas.", i.ClientMask);
                World.Instance.SendMessage(
                    "@W" + string.Format("{0,-15}", "color") + " @w- Change default color of mob name. Enter default as value to set default color from config.", i.ClientMask);
                World.Instance.SendMessage(
                    "@W" + string.Format("{0,-15}", "keywords") + " @w- Change keywords for mob.", i.ClientMask);
                World.Instance.SendMessage(
                    "@W" + string.Format("{0,-15}", "longname") + " @w- Change longname (roomname) of mob.",
                    i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "level") + " @w- Change level of mob.",
                                           i.ClientMask);
                World.Instance.SendMessage("@wEnter '@Wclear@w' as a shortname to remove that name from mob.", i.ClientMask);
                return true;
            }

            uint mobId;
            if(!uint.TryParse(i.Arguments.Groups[1].Value, out mobId))
            {
                World.Instance.SendMessage("@wInvalid mob ID given (@R" + i.Arguments.Groups[1].Value + "@w).",
                                           i.ClientMask);
                return true;
            }

            Mob r = GetMob(mobId);
            if(r == null)
            {
                World.Instance.SendMessage("@wNo such mob in database (@R" + mobId + "@w).", i.ClientMask);
                return true;
            }

            if(i.Arguments.Groups[2].Length != 0)
            {
                string key, val;
                string str = i.Arguments.Groups[2].Value.Trim();
                if(str.Contains(' '))
                {
                    key = str.Substring(0, str.IndexOf(' ')).ToLower();
                    val = str.Substring(key.Length).Trim();

                    switch(key)
                    {
                        case "addflag":
                            r.AddFlag(val);
                            break;

                        case "removeflag":
                            r.RemoveFlag(val);
                            break;

                        case "name":
                            if(r.Name.Length == 0)
                                r.Name = new[] { val };
                            else
                                r.Name[0] = val;
                            break;

                        case "level":
                            {
                                int lvl;
                                if(int.TryParse(val, out lvl))
                                    r.Level = lvl;
                            } break;

                        case "areas":
                            {
                                string[] spl = val.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if(spl.Length > 0)
                                    r.Areas = spl;
                            } break;

                        case "keywords":
                            {
                                if(val.Length > 0)
                                    r.Keyword = val;
                            } break;

                        case "longname":
                            {
                                if(val == "clear")
                                    r.Longname = "";
                                else
                                    r.Longname = val;
                            } break;

                        case "color":
                            {
                                if(val == "clear" || val == "default" || val == "none")
                                    r.DefaultColor = null;
                                else if(val.Length > 0)
                                    r.DefaultColor = val;
                            } break;
                    }

                    if(key.StartsWith("name") && key.Length > 4)
                    {
                        key = key.Substring(4);
                        int nth;
                        if(int.TryParse(key, out nth) && nth >= 1)
                        {
                            nth--;
                            List<string> Names = r.Name.ToList();
                            if(nth >= r.Name.Length)
                            {
                                if(val != "clear")
                                    Names.Add(val);
                            }
                            else if(val != "clear")
                                Names[nth] = val;
                            else
                                Names.RemoveAt(nth);

                            r.Name = Names.ToArray();
                        }
                    }
                }
            }

            World.Instance.SendMessage("@w+----------------------------------------------------------------------+",
                                       i.ClientMask);
            World.Instance.SendMessage("@w| @WEntry       @w: @Y" + string.Format("{0,-55}", r.Entry) + "@w|",
                                       i.ClientMask);
            int k = 0;
            foreach(string z in r.Name)
            {
                ++k;
                string tag = "Name (" + k + ")";
                World.Instance.SendMessage(
                    "@w| @W" + string.Format("{0,-11}", tag) + " @w: @G" +
                    string.Format("{0,-55}", !string.IsNullOrEmpty(z) ? z : "Unknown") + "@w|", i.ClientMask);
            }
            string area = "";
            if(r.Areas != null)
            {
                foreach(string y in r.Areas)
                {
                    if(area.Length > 0)
                        area += " ";
                    area += y;
                }
            }
            World.Instance.SendMessage("@w| @WArea        @w: @M" + string.Format("{0,-55}", area) + "@w|", i.ClientMask);
            {
                StringBuilder strFlags = new StringBuilder();
                if(r.Flags != null)
                {
                    foreach(string x in r.Flags)
                    {
                        if(strFlags.Length > 0)
                            strFlags.Append(", ");
                        strFlags.Append(x);
                    }
                }

                string[] spl = Utility.WrapColored(strFlags.ToString(), 54, 0);
                for(int j = 0; j < spl.Length; j++)
                {
                    if(j == 0)
                        World.Instance.SendMessage("@w| @WFlags       @w: " + string.Format("{0,-55}", spl[j]) + "@w|",
                                                   i.ClientMask);
                    else
                        World.Instance.SendMessage("@w|             : " + string.Format("{0,-55}", spl[j]) + "@w|",
                                                   i.ClientMask);
                }
            }

            World.Instance.SendMessage("@w| @WLongname    @w: " + Utility.FormatColoredString(r.Longname, -55) + "@w|",
                                       i.ClientMask);
            World.Instance.SendMessage("@w| @WKeywords    @w: " + string.Format("{0,-55}", r.Keyword) + "@w|",
                                       i.ClientMask);
            World.Instance.SendMessage("@w| @WLevel       @w: @Y" + string.Format("{0,-55}", r.Level) + "@w|",
                                       i.ClientMask);

            string defColor = Config.GetString("Mob.DefaultColor", "@y");
            if(string.IsNullOrEmpty(r.DefaultColor))
                defColor += "Default";
            else
                defColor = r.DefaultColor + r.DefaultColor.Replace("@", "@@");
            World.Instance.SendMessage("@w| @WColor       @w: " + Utility.FormatColoredString(defColor, -55) + "@w|",
                                       i.ClientMask);

            World.Instance.SendMessage("@w+----------------------------------------------------------------------+",
                                       i.ClientMask);
            return true;
        }

        private bool CommandFind(InputData i)
        {
            if(!i.Arguments.Success)
            {
                World.Instance.SendMessage("@wSyntax: mobdb find [exact] [case] <mob name>", i.ClientMask);
                return true;
            }

            string name = i.Arguments.Groups[3].Value;
            if(i.Arguments.Groups[2].Length == 0)
                name = name.ToLower();
            else if(name.Length >= 1)
                name = NormalizeName(name);

            World.Instance.SendMessage("@wSearched for '@W" + name + "@w'.", i.ClientMask);
            List<Mob> Found = new List<Mob>();
            foreach(KeyValuePair<uint, Mob> x in IMobs)
            {
                if(i.Arguments.Groups[1].Length != 0)
                {
                    if(i.Arguments.Groups[2].Length != 0)
                    {
                        if(x.Value.Name.Contains(name))
                            Found.Add(x.Value);
                    }
                    else
                    {
                        foreach(string z in x.Value.Name)
                        {
                            if(z.ToLower() == name)
                            {
                                Found.Add(x.Value);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if(i.Arguments.Groups[2].Length != 0)
                    {
                        if(x.Value.Name.Contains(name))
                            Found.Add(x.Value);
                    }
                    else
                    {
                        foreach(string z in x.Value.Name)
                        {
                            if(z.ToLower().Contains(name))
                            {
                                Found.Add(x.Value);
                                break;
                            }
                        }
                    }
                }
            }

            if(Found.Count != 0)
            {
                World.Instance.SendMessage("@WEntry  Name                                          Area         Room", i.ClientMask);
                World.Instance.SendMessage("@G====== ============================================= ============ ======",
                                           i.ClientMask);
                foreach(Mob x in Found)
                {
                    string area = "";
                    if(x.Areas != null)
                    {
                        foreach(string y in x.Areas)
                        {
                            if(area.Length != 0)
                                area += " ";
                            area += y;
                        }
                    }
                    string room = "";
                    uint ro = x.GetBestRoom();
                    if(ro != uint.MaxValue)
                        room = ro.ToString();
                    World.Instance.SendMessage("@Y" + string.Format("{0,-6}", x.Entry) + " @c" +
                                               string.Format(
                                                   "{0,-" + "=============================================".Length + "}",
                                                   x.Names) + " @g" + string.Format("{0,-12}", area) + " @Y" + room);
                }
            }

            World.Instance.SendMessage("@wFound @C" + Found.Count + " @wmob" + (Found.Count != 1 ? "s" : "") + ".",
                                       i.ClientMask);
            return true;
        }

        private bool CommandDelete(InputData i)
        {
            if(!i.Arguments.Success)
            {
                World.Instance.SendMessage("@wSyntax: mobdb delete <mobid>", i.ClientMask);
                World.Instance.SendMessage("        @wmobdb delete <area keyword>", i.ClientMask);
                World.Instance.SendMessage("        @wmobdb delete locations", i.ClientMask);
                World.Instance.SendMessage("        @wmobdb delete all", i.ClientMask);
                return true;
            }

            if(i.Arguments.Groups[1].Value.ToLower().Trim() == "locations")
            {
                foreach(KeyValuePair<uint, Mob> x in IMobs)
                {
                    x.Value.Locations.Clear();
                }

                World.Instance.SendMessage("@wDeleted all mob locations.", i.ClientMask);
                return true;
            }

            uint id;
            if(uint.TryParse(i.Arguments.Groups[1].Value, out id))
            {
                if(!IMobs.ContainsKey(id))
                    World.Instance.SendMessage("@wNo such mob in database (@R" + id + "@w).", i.ClientMask);
                else
                {
                    World.Instance.SendMessage("@wDeleted '@G" + IMobs[id].Names + "@w'.", i.ClientMask);
                    foreach(MobLocation ml in IMobs[id].Locations)
                    {
                        RoomLocations[ml.RoomEntry].Remove(ml);
                        if(RoomLocations[ml.RoomEntry].Count == 0)
                            RoomLocations.Remove(ml.RoomEntry);
                    }
                    IMobs.Remove(id);
                }
            }
            else
            {
                string key = i.Arguments.Groups[1].Value.ToLower().Trim();
                bool isConfirm = false;
                if(key.Contains(' '))
                {
                    isConfirm = key.Substring(key.IndexOf(' ') + 1).Trim() == "confirm";
                    key = key.Substring(0, key.IndexOf(' '));
                }

                if(key == "all" && !isConfirm)
                {
                    World.Instance.SendMessage("@wAre you sure you wish to delete all mobs? Enter '@Wmobdb delete all confirm@w' to confirm.", i.ClientMask);
                    return true;
                }

                List<uint> del = new List<uint>();
                foreach(KeyValuePair<uint, Mob> x in IMobs)
                {
                    if(key == "all" || x.Value.Areas.Contains(key))
                        del.Add(x.Key);
                }

                foreach(uint x in del)
                    IMobs.Remove(x);

                World.Instance.SendMessage("@wDeleted @C" + del.Count + " @wmob" + (del.Count != 1 ? "s" : "") + ".", i.ClientMask);
            }
            return true;
        }

        private bool CommandCount(InputData i)
        {
            int thisArea = 0;
            int total = IMobs.Count;
            if(!string.IsNullOrEmpty(RoomInfoArea))
            {
                foreach(KeyValuePair<uint, Mob> x in IMobs)
                {
                    if(x.Value.Areas == null || x.Value.Areas.Length == 0)
                        continue;
                    if(x.Value.Areas.Contains(RoomInfoArea))
                        thisArea++;
                }
            }
            World.Instance.SendMessage("@wYou have added @G" + thisArea + " @wmobs to database in this area.",
                                       i.ClientMask);
            World.Instance.SendMessage("@wYou have added @G" + total + " @wmobs to database in all areas.", i.ClientMask);
            return true;
        }

        private bool CommandSave(InputData i)
        {
            string fileName = DBFileName;
            if(i.Arguments.Success)
                fileName = i.Arguments.Groups[0].Value;

            Save(fileName);
            World.Instance.SendMessage("@wSaved mob database to '@W" + fileName + "@w'.", i.ClientMask);
            return true;
        }

        #endregion

        #region Triggers

        private bool TriggerLastKills(TriggerData t)
        {
            if(ChooseFromUnknown == 1)
                ChooseFromUnknown = 2;
            return false;
        }

        private bool TriggerLastKills2(TriggerData t)
        {
            if(ChooseFromUnknown == 0)
                return false;

            AddMobName = t.Match.Groups[1].Value.Trim();
            if(!int.TryParse(t.Match.Groups[2].Value, out AddMobLevel) || UnknownMobs.Count == 0)
            {
                ChooseFromUnknown = 0;
                return false;
            }
            AddMobArea = RoomInfoArea;

            int i = 1;
            World.Instance.SendMessage("@wSelect mob longname from list (type number):");
            foreach(string x in UnknownMobs)
            {
                World.Instance.SendMessage("@W" + i + ". " + x);
                i++;
            }
            return false;
        }

        private bool TriggerRoomChars1(TriggerData t)
        {
            foundMobs.Clear();
            UnknownMobs.Clear();
            ListeningRoomChars = true;
            if(Config.GetInt32("Tags.Gag", 1) != 0)
                t.Msg.AuthMask = 0;

            CurrentTime = (uint)(DateTime.UtcNow - new DateTime(2012, 1, 1)).TotalSeconds;
            return false;
        }

        private bool TriggerRoomChars2(TriggerData t)
        {
            ListeningRoomChars = false;
            if(Config.GetInt32("Tags.Gag", 1) != 0)
                t.Msg.AuthMask = 0;
            if(RoomInfoEntry != uint.MaxValue && RoomLocations.ContainsKey(RoomInfoEntry))
            {
                foreach(MobLocation x in RoomLocations[RoomInfoEntry])
                {
                    Mob m = GetMob(x.MobEntry);
                    if(m != null && CurrentTime - x.LastVisited >= 600)
                    {
                        x.TimesVisited++;
                        if(foundMobs.Contains(m))
                            x.TimesSeen++;
                        x.LastVisited = CurrentTime;
                    }
                }
            }
            return false;
        }

        private List<Mob> foundMobs = new List<Mob>();
        private bool TriggerRoomChars3(TriggerData t)
        {
            if(!ListeningRoomChars)
                return false;

            string n = t.Msg.Msg;
            Mob m = GetMob(ref n);
            t.Msg.Msg = n;

            if(m == null)
                return false;

            if(RoomInfoEntry != uint.MaxValue)
            {
                if(!foundMobs.Contains(m))
                    foundMobs.Add(m);

                bool f = false;
                foreach(MobLocation x in m.Locations)
                {
                    if(x.RoomEntry != RoomInfoEntry)
                        continue;

                    f = true;
                    if(CurrentTime - x.LastVisited >= 600)
                        x.CountSeen++;
                    break;
                }

                if(!f)
                {
                    MobLocation ml = new MobLocation();
                    ml.RoomEntry = RoomInfoEntry;
                    ml.MobEntry = m.Entry;
                    ml.LastVisited = CurrentTime;
                    ml.TimesSeen = 0;
                    ml.TimesVisited = 0;
                    ml.CountSeen = 1;
                    m.Locations.Add(ml);
                    if(!RoomLocations.ContainsKey(RoomInfoEntry))
                        RoomLocations[RoomInfoEntry] = new List<MobLocation>();
                    RoomLocations[RoomInfoEntry].Add(ml);
                }
            }
            return false;
        }

        private bool TriggerRoomInfoNum(TriggerData t)
        {
            if(ChooseFromUnknown != 0)
            {
                ChooseFromUnknown = 0;
                World.Instance.SendMessage("@wChanged room. Mob adding was cancelled.");
            }
            UnknownMobs.Clear();
            uint i;
            if(!uint.TryParse(t.Match.Groups[1].Value, out i))
            {
                RoomInfoEntry = uint.MaxValue;
                return false;
            }

            RoomInfoEntry = i;
            return false;
        }

        private bool TriggerRoomInfoArea(TriggerData t)
        {
            RoomInfoArea = t.Match.Groups[1].Value.Trim();
            return false;
        }

        #endregion

        #region Saving & Loading

        public override void Shutdown()
        {
            base.Shutdown();

            Save(DBFileName);
        }

        private void Load()
        {
            StreamReader f;
            try
            {
                f = new StreamReader(DBFileName);
            }
            catch
            {
                // No database exists or we aren't allowed to read it. Make a new database.
                return;
            }

            Mob[] data;
            try
            {
                DataContractSerializer x = new DataContractSerializer(typeof(Mob[]));
                data = x.ReadObject(f.BaseStream) as Mob[];
            }
            catch
            {
                f.Close();
                Log.Error("Failed loading mob database! File corrupted?");
                return;
            }

            f.Close();

            if(data == null)
                return;

            foreach(Mob a in data)
            {
                if(a == null)
                    continue;

                IMobs[a.Entry] = a;
                if(a.Entry > _guidMob && a.Entry != uint.MaxValue)
                    _guidMob = a.Entry;

                foreach(MobLocation m in a.Locations)
                {
                    if(!RoomLocations.ContainsKey(m.RoomEntry))
                        RoomLocations[m.RoomEntry] = new List<MobLocation>();
                    RoomLocations[m.RoomEntry].Add(m);
                }
            }

            // Successfully loaded a database. Now make a backup because we have a working copy at the moment.
            File.Delete(DBFileBackup);
            File.Copy(DBFileName, DBFileBackup);
        }

        private void Save(string fileName)
        {
            if(IMobs.Count == 0)
                return;
            StreamWriter f = new StreamWriter(fileName, false);

            try
            {
                DataContractSerializer x = new DataContractSerializer(typeof(Mob[]));
                x.WriteObject(f.BaseStream, IMobs.Values.ToArray());
            }
            catch(Exception e)
            {
                f.Close();
                throw e;
            }

            f.Close();
            if(Config.GetInt32("AutoSave", 0) != 0)
                WhenSave = World.Instance.MSTime + Config.GetInt32("AutoSave", 0) * 1000;
        }

        #endregion

        public override void Update(long msTime)
        {
            base.Update(msTime);

            if(WhenSave == 0 && Config.GetInt32("AutoSave", 0) != 0)
                WhenSave = Config.GetInt32("AutoSave", 0) * 1000 + msTime;
            else if(WhenSave > 0 && WhenSave <= msTime)
                Save(DBFileName);
        }
    }

    [DataContract]
    internal class MobLocation
    {
        [DataMember]
        internal uint MobEntry;

        [DataMember]
        internal uint RoomEntry;

        [DataMember]
        internal uint TimesSeen;

        [DataMember]
        internal uint CountSeen;

        [DataMember]
        internal uint TimesVisited;

        [DataMember]
        internal uint LastVisited;
    }

    public class MobDBConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("Mob.DefaultColor", "@y", "Default color for the mob. Color can be changed by other plugins");
            CreateSetting("Mob.Longname", "@w[@G$mob.level@w] $mob.color$mob.name @D($mob.keywords)", "Replace mob room name with this format. Set this to \"\" to disable mob replacing. You can have variables in the name such as:\n$mob.entry - This is the unique ID of mob (our assigned not mud).\n$mob.level - Level of the mob.\n$mob.name - Mob shortname, what appears when you are fighting it.\n$mob.longname - Mob room name.\n$mob.keywords - These are mob keywords.\n$mob.color - This is the color of the mob name. Can be set from other plugins or manually changed.\nEnter $$ to escape the $ character.");
            CreateSetting("AutoSave", 0, "Save mob database every X seconds. For example enter 600 to save mob database every 10 minutes. Enter 0 to disable this feature. The database is also saved on shutdown of program. You can also type \"mobdb save\" to manually save the database.");
            CreateSetting("Tags.Gag", 1, "Gag {roomchars} tags for clients.");
        }
    }
}
