using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore;
using ProxyCore.Input;

namespace Mapper
{
    public partial class Mapper
    {
        private bool CommandExit(InputData i)
        {
            //      1        2          3      4
            // @"^(help)?(room\s+\d+)?(\d+)?(\s+.+)?"
            if(i.Arguments.Groups[1].Length != 0)
            {
                World.Instance.SendMessage("@wSyntax:", i.ClientMask);
                World.Instance.SendMessage(string.Format("{0,-20}", "map exit") + " - show exits in current room.", i.ClientMask);
                World.Instance.SendMessage(string.Format("{0,-20}", "map exit help") + " - show this message.", i.ClientMask);
                World.Instance.SendMessage(string.Format("{0,-20}", "map exit room <room ID>") + " - show exits in room.", i.ClientMask);
                World.Instance.SendMessage(string.Format("{0,-20}", "map exit <ID> [option] [value]") + " - change exit options / show information by exit ID.", i.ClientMask);
                World.Instance.SendMessage("", i.ClientMask);
                World.Instance.SendMessage("@wAvailable options for exit:", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "addflag") + " @w- Add a flag to exit.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "removeflag") + " @w- Remove a flag from exit.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "command") + " @w- Change command that activates exit.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "doorcommand") + " @w- Change command that we use to open door, for example '@Wopen altar@w'. Use clear to remove this field.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "cost") + " @w- Change cost of using exit.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "minlevel") + " @w- Minimum level required to use exit.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "maxlevel") + " @w- Maximum level allowed to use exit.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "delete confirm") + " @w- Delete the exit.", i.ClientMask);
                World.Instance.SendMessage("", i.ClientMask);
                World.Instance.SendMessage("@wAvailable default flags:", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "recallmechanic") + " @w- This exit uses recall mechanic, can't use in norecall (for portals).", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "door") + " @w- This exit has a door.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "nogq") + " @w- Don't use this exit on a global quest (for regular chaos portals).", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "disabled") + " @w- Don't use this exit ever.", i.ClientMask);
                //World.Instance.SendMessage("@W" + string.Format("{0,-15}", "nopass") + " @w- The door is nopass.", i.ClientMask);
                //World.Instance.SendMessage("@W" + string.Format("{0,-15}", "locked") + " @w- The door is locked.", i.ClientMask);
                //World.Instance.SendMessage("@W" + string.Format("{0,-15}", "pickable") + " @w- Door can be pick locked or bashdoor.", i.ClientMask);
                World.Instance.SendMessage("@wNote: there may be custom flags that are implemented by other plugins / scripts that aren't listed here.", i.ClientMask);
                return true;
            }

            if(i.Arguments.Groups[2].Length != 0 || (i.Arguments.Groups[2].Length == 0 && i.Arguments.Groups[1].Length == 0 && i.Arguments.Groups[3].Length == 0))
            {
                uint roomId;
                if(i.Arguments.Groups[2].Length != 0)
                {
                    string str = i.Arguments.Groups[2].Value.Substring(4).Trim();
                    if(!uint.TryParse(str, out roomId))
                    {
                        World.Instance.SendMessage("@wInvalid room ID given. Type '@Wmap exit help@w' for syntax.",
                                                   i.ClientMask);
                        return true;
                    }
                }
                else
                {
                    roomId = CurrentRoomId;
                    if(roomId == uint.MaxValue)
                    {
                        World.Instance.SendMessage("@wYou are in an unknown room.", i.ClientMask);
                        return true;
                    }
                }

                Room r = GetRoom(roomId);
                if(r == null)
                {
                    World.Instance.SendMessage("@wNo such room in database (@R" + roomId + "@w).", i.ClientMask);
                    return true;
                }

                World.Instance.SendMessage("@wExits in '@G" + r.Name + "@w' [@Y" + r.Entry + "@w]:", i.ClientMask);
                if(r.exits.Count == 0)
                    World.Instance.SendMessage("@wNo exits found.", i.ClientMask);
                else
                {
                    World.Instance.SendMessage("@WEntry  Command              To", i.ClientMask);
                    World.Instance.SendMessage("@G====== ==================== ========================", i.ClientMask);
                    foreach(Exit e in r.exits)
                        World.Instance.SendMessage("@Y" + string.Format("{0,-6}", e.Entry) + " @W" + string.Format("{0,-20}", e.Command) + " @w[@Y" + string.Format("{0,6}", e.To.Entry) + "@w] @G" + e.To.Name, i.ClientMask);
                    World.Instance.SendMessage("", i.ClientMask);
                    World.Instance.SendMessage("@wType '@Wmap exit <exit ID>@w' for more information about an exit.", i.ClientMask);
                    World.Instance.SendMessage("@wOr '@Wmap exit help@w' for syntax.", i.ClientMask);
                }
                return true;
            }

            if(i.Arguments.Groups[3].Length != 0)
            {
                uint exitId;
                if(!uint.TryParse(i.Arguments.Groups[3].Value, out exitId))
                {
                    World.Instance.SendMessage("@wInvalid exit ID given. Type '@Wmap exit help@w' for syntax.", i.ClientMask);
                    return true;
                }

                Exit e = GetExit(exitId);
                if(e == null && IPortals.ContainsKey(exitId))
                    e = IPortals[exitId];
                if(e == null)
                {
                    World.Instance.SendMessage("@wNo such exit in database (@R" + exitId + "@w).", i.ClientMask);
                    return true;
                }

                if(i.Arguments.Groups[4].Length != 0)
                {
                    string key, val;
                    string str = i.Arguments.Groups[4].Value.Trim();
                    if(str.Contains(' '))
                    {
                        key = str.Substring(0, str.IndexOf(' ')).ToLower();
                        val = str.Substring(key.Length).Trim();

                        switch(key)
                        {
                            case "addflag":
                                e.AddFlag(val);
                                break;

                            case "removeflag":
                                e.RemoveFlag(val);
                                break;

                            case "command":
                                val = val.ToLower();
                                e.Command = PathfindResult.IsDirectionCommand(val) != 'x' ?
                                    PathfindResult.IsDirectionCommand(val).ToString() :
                                    val;
                                break;

                            case "doorcommand":
                                val = val.ToLower();
                                e.DoorCommand = val != "clear" ? val : null;
                                break;

                            case "cost":
                                {
                                    uint u;
                                    if(uint.TryParse(val, out u))
                                        e.Cost = u;
                                } break;

                            case "minlevel":
                                {
                                    int lvl;
                                    if(int.TryParse(val, out lvl))
                                        e.MinLevel = lvl;
                                } break;

                            case "maxlevel":
                                {
                                    int lvl;
                                    if(int.TryParse(val, out lvl))
                                        e.MaxLevel = lvl;
                                } break;

                            case "delete":
                                {
                                    if(val == "confirm")
                                    {
                                        if(e.HasFlag("portal"))
                                        {
                                            IPortals.Remove(e.Entry);
                                            e.To.Area.Portals.Remove(e);
                                        }
                                        else
                                        {
                                            IExits.Remove(e.Entry);
                                            e.From.exits.Remove(e);
                                        }
                                        World.Instance.SendMessage("@wDeleted exit or portal (@R" + e.Entry + "@w).",
                                                                   i.ClientMask);
                                        return true;
                                    }
                                    World.Instance.SendMessage("@wEnter '@Wmap exit <num> delete confirm@w' to remove the exit.", i.ClientMask);
                                } break;

                            default:
                                World.Instance.SendMessage("@wInvalid key value pair entered '@R" + key + " " + val + "@w'.", i.ClientMask);
                                World.Instance.SendMessage("@wUse '@Wmap exit help@w' to see syntax.", i.ClientMask);
                                break;
                        }
                    }
                }

                World.Instance.SendMessage("@w+----------------------------------------------------------------------+", i.ClientMask);
                World.Instance.SendMessage("@w| @WEntry       @w: @Y" + string.Format("{0,-55}", e.Entry) + "@w|", i.ClientMask);
                if(!e.HasFlag("portal"))
                    World.Instance.SendMessage("@w| @WFrom        @w: @w[@Y" + string.Format("{0,6}", e.From.Entry) + "@w] @G" + Utility.FormatColoredString(!string.IsNullOrEmpty(e.From.Name) ? e.From.Name : "Unknown", -46) + "@w|", i.ClientMask);
                World.Instance.SendMessage("@w| @WTo          @w: @w[@Y" + string.Format("{0,6}", e.To.Entry) + "@w] @G" + Utility.FormatColoredString(!string.IsNullOrEmpty(e.To.Name) ? e.To.Name : "Unknown", -46) + "@w|", i.ClientMask);
                World.Instance.SendMessage("@w| @WCommand     @w: @c" + string.Format("{0,-55}", e.Command) + "@w|", i.ClientMask);
                World.Instance.SendMessage("@w| @WDoor command@w: @c" + string.Format("{0,-55}", !string.IsNullOrEmpty(e.DoorCommand) ? e.DoorCommand : "none") + "@w|", i.ClientMask);
                World.Instance.SendMessage("@w| @WCost        @w: @C" + string.Format("{0,-55}", e.Cost) + "@w|", i.ClientMask);
                StringBuilder strFlags = new StringBuilder();
                if(e.IFlags != null)
                {
                    foreach(string x in e.IFlags)
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
                        World.Instance.SendMessage("@w| @WFlags       @w: " + string.Format("{0,-55}", spl[j]) + "@w|", i.ClientMask);
                    else
                        World.Instance.SendMessage("@w|             : " + string.Format("{0,-55}", spl[j]) + "@w|", i.ClientMask);
                }

                World.Instance.SendMessage("@w| @WMin level   @w: @W" + string.Format("{0,-55}", e.MinLevel) + "@w|", i.ClientMask);
                World.Instance.SendMessage("@w| @WMax level   @w: @W" + string.Format("{0,-55}", e.MaxLevel) + "@w|", i.ClientMask);
                World.Instance.SendMessage("@w+----------------------------------------------------------------------+", i.ClientMask);
                if(i.Arguments.Groups[4].Length == 0)
                    World.Instance.SendMessage("@wSee '@Wmap exit help@w' for information on how to edit this exit.", i.ClientMask);
                return true;
            }

            World.Instance.SendMessage("0", i.ClientMask);
            return true;
        }

        private bool CommandPortal(InputData i)
        {
            if(IPortals.Count == 0)
            {
                World.Instance.SendMessage("@wYou have no portals set.", i.ClientMask);
                return true;
            }

            int count = 0;
            World.Instance.SendMessage("@WEntry    Command          To", i.ClientMask);
            World.Instance.SendMessage("@G======== ================ =====================================", i.ClientMask);
            foreach(KeyValuePair<uint, Exit> x in IPortals)
            {
                World.Instance.SendMessage("@w[@Y" + string.Format("{0,6}", x.Key) + "@w] @w" + string.Format("{0,-16}", x.Value.Command) + " @M" + x.Value.To.Area.Name + " @wroom [@Y" + x.Value.To.Entry + "@w]", i.ClientMask);
                count++;
            }

            World.Instance.SendMessage("@C" + count + " @wportals found.", i.ClientMask);
            return true;
        }

        private bool CommandCreateExit(InputData i)
        {
            if(!i.Arguments.Success)
            {
                World.Instance.SendMessage("@wSyntax: map createexit [from room ID = current] <to room ID> \"<command>\"", i.ClientMask);
                return true;
            }

            uint fromId;
            uint toId;
            if(i.Arguments.Groups[2].Length > 0)
            {
                if(!uint.TryParse(i.Arguments.Groups[1].Value, out fromId) ||
                    !uint.TryParse(i.Arguments.Groups[2].Value.Trim(), out toId))
                {
                    World.Instance.SendMessage("@wSyntax: map createexit [from room ID = current] <to room ID> \"<command>\"", i.ClientMask);
                    return true;
                }
            }
            else
            {
                fromId = CurrentRoomId;
                if(!uint.TryParse(i.Arguments.Groups[1].Value, out toId))
                {
                    World.Instance.SendMessage("@wSyntax: map createexit [from room ID = current] <to room ID> \"<command>\"", i.ClientMask);
                    return true;
                }
            }

            Room from = GetRoom(fromId);
            Room to = GetRoom(toId);
            if(from == null || to == null)
            {
                World.Instance.SendMessage("@wNo such room exists in mapper database.", i.ClientMask);
                return true;
            }

            char dir = PathfindResult.IsDirectionCommand(i.Arguments.Groups[3].Value);
            Exit e = new Exit(++_guidExit);
            e.Command = dir != 'x' ? char.ToLower(dir).ToString() : i.Arguments.Groups[3].Value.ToLower();
            e.From = from;
            e.To = to;
            e.From.exits.Add(e);
            e.From.UpdateExits();
            IExits[e.Entry] = e;
            World.Instance.SendMessage("@wCreated a new exit (@R" + e.Entry + "@w).", i.ClientMask);
            World.Instance.SendMessage("@wType '@Wmap exit " + e.Entry + "@w' for more information or to edit.", i.ClientMask);
            return true;
        }

        private bool CommandCreatePortal(InputData i)
        {
            if(!i.Arguments.Success)
            {
                World.Instance.SendMessage("@wSyntax: map createportal <to room ID> \"<command>\"", i.ClientMask);
                return true;
            }

            uint toId;
            if(!uint.TryParse(i.Arguments.Groups[1].Value, out toId))
            {
                World.Instance.SendMessage("@wSyntax: map createportal <to room ID> \"<command>\"", i.ClientMask);
                return true;
            }

            Room to = GetRoom(toId);
            if(to == null)
            {
                World.Instance.SendMessage("@wNo such room exists in mapper database.", i.ClientMask);
                return true;
            }

            Exit e = new Exit(++_guidExit);
            e.Command = i.Arguments.Groups[2].Value.ToLower();
            e.To = to;
            e.Cost = 5;
            if(e.IFlags == null)
                e.IFlags = new List<string>();
            e.IFlags.Add("portal");
            IPortals[e.Entry] = e;
            to.Area.Portals.Add(e);
            World.Instance.SendMessage("@wCreated a new portal (@R" + e.Entry + "@w).", i.ClientMask);
            World.Instance.SendMessage("@wType '@Wmap exit " + e.Entry + "@w' for more information or to edit.", i.ClientMask);
            return true;
        }

        private bool CommandRoomInfo(InputData i)
        {
            //     1      2      3
            // @"(help)?(\d+)?(\s+.+)?"
            if(i.Arguments.Success && i.Arguments.Groups[1].Length != 0)
            {
                World.Instance.SendMessage("@wSyntax:", i.ClientMask);
                World.Instance.SendMessage(string.Format("{0,-20}", "map room") + " - show info about current room.", i.ClientMask);
                World.Instance.SendMessage(string.Format("{0,-20}", "map room help") + " - show this message.", i.ClientMask);
                World.Instance.SendMessage(string.Format("{0,-20}", "map room <ID> [option] [value]") + " - show info about room by ID or change it.", i.ClientMask);
                World.Instance.SendMessage("", i.ClientMask);
                World.Instance.SendMessage("@wAvailable options for room:", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "addflag") + " @w- Add a flag to room.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "removeflag") + " @w- Remove a flag from room.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "addcflag") + " @w- Add a custom flag to room.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "removecflag") + " @w- Remove a custom flag from room.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "name") + " @w- Change name of room.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "entrycost") + " @w- Change cost of entering room.", i.ClientMask);
                World.Instance.SendMessage("@W" + string.Format("{0,-15}", "leavecost") + " @w- Change cost of leaving room.", i.ClientMask);
                //World.Instance.SendMessage("@W" + string.Format("{0,-15}", "healrate") + " @w- Change heal rate.", i.ClientMask);
                //World.Instance.SendMessage("@W" + string.Format("{0,-15}", "manarate") + " @w- Change mana rate.", i.ClientMask);
                //World.Instance.SendMessage("@W" + string.Format("{0,-15}", "sector") + " @w- Change sector.", i.ClientMask);
                return true;
            }

            uint roomId;
            if(i.Arguments.Success && i.Arguments.Groups[2].Length != 0)
            {
                if(!uint.TryParse(i.Arguments.Groups[2].Value, out roomId))
                {
                    World.Instance.SendMessage("@wInvalid room ID given (@R" + i.Arguments.Groups[2].Value + "@w).", i.ClientMask);
                    World.Instance.SendMessage("@wUse '@Wmap roominfo help@w' to see syntax.", i.ClientMask);
                    return true;
                }
            }
            else
            {
                roomId = CurrentRoomId;
                if(roomId == uint.MaxValue)
                {
                    World.Instance.SendMessage("@wYou are in an invalid room.", i.ClientMask);
                    World.Instance.SendMessage("@wUse '@Wmap roominfo help@w' to see syntax.", i.ClientMask);
                    return true;
                }
            }

            Room r = GetRoom(roomId);
            if(r == null)
            {
                World.Instance.SendMessage("@wNo such room in database (@R" + roomId + "@w).", i.ClientMask);
                return true;
            }

            if(i.Arguments.Success && i.Arguments.Groups[2].Length != 0 && i.Arguments.Groups[3].Length != 0)
            {
                string key, val;
                string str = i.Arguments.Groups[3].Value.Trim();
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

                        case "addcflag":
                            r.AddCustomFlag(val);
                            break;

                        case "removecflag":
                            r.RemoveCustomFlag(val);
                            break;

                        case "name":
                            r.Name = val;
                            break;

                        case "entrycost":
                            {
                                uint u;
                                if(uint.TryParse(val, out u))
                                    r.EntryCost = u;
                            } break;

                        case "leavecost":
                            {
                                uint u;
                                if(uint.TryParse(val, out u))
                                    r.LeaveCost = u;
                            } break;
                    }
                }
            }

            World.Instance.SendMessage("@w+----------------------------------------------------------------------+", i.ClientMask);
            World.Instance.SendMessage("@w| @WEntry       @w: @Y" + string.Format("{0,-55}", r.Entry) + "@w|", i.ClientMask);
            World.Instance.SendMessage("@w| @WName        @w: @G" + Utility.FormatColoredString(!string.IsNullOrEmpty(r.Name) ? r.Name : "Unknown", -55) + "@w|", i.ClientMask);
            World.Instance.SendMessage("@w| @WArea        @w: @M" + string.Format("{0,-55}", !string.IsNullOrEmpty(r.Area.Name) ? r.Area.Name : "Unknown") + "@w|", i.ClientMask);
            {
                StringBuilder strFlags = new StringBuilder();
                if(r.IFlags != null)
                {
                    foreach(string x in r.IFlags)
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

            {
                StringBuilder strFlags = new StringBuilder();
                if(r.CFlags != null)
                {
                    foreach(string x in r.CFlags)
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
                        World.Instance.SendMessage("@w| @WCustom flags@w: " + string.Format("{0,-55}", spl[j]) + "@w|", i.ClientMask);
                    else
                        World.Instance.SendMessage("@w|             : " + string.Format("{0,-55}", spl[j]) + "@w|", i.ClientMask);
                }
            }

            // Healrate, manarate, sector

            World.Instance.SendMessage("@w+----------------------------------------------------------------------+", i.ClientMask);
            if(!i.Arguments.Success || (i.Arguments.Groups[1].Length == 0 && i.Arguments.Groups[2].Length == 0 && i.Arguments.Groups[3].Length == 0))
                World.Instance.SendMessage("@wUse '@Wmap roominfo help@w' to see syntax.", i.ClientMask);
            return true;
        }
    }
}
