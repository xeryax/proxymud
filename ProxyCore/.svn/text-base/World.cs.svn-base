using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore.Input;
using ProxyCore.Output;
using ProxyCore.Messages;
using ProxyCore.Scripting;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace ProxyCore
{
    public class World
    {
        public World()
        {
            Instance = this;
            Log.Write("Loading plugins...");
            PluginMgr.LoadAll();
            TriggerHandler.RegisterTrigger("core.login", @"^\$gmcp\.char\.vitals\.hp ", _GMCPHP);
            InputHandler.RegisterCommand("commands", "", _Commands, CMDFlags.None, null, 0, ulong.MaxValue, "core", 8);
            InputHandler.RegisterCommand("lastlines", @"^(\d+)$", _LineInfo, CMDFlags.None, null, 0, ulong.MaxValue, "core", 8);
            InputHandler.RegisterCommand("plugins", @"^(\w+)(\s+full)?", _PluginInfo, CMDFlags.None, null, 0, ulong.MaxValue, "core", 6);
            InputHandler.RegisterCommand("shutdown", "", _Shutdown, CMDFlags.None, null, 0, ulong.MaxValue, "core", 8);
        }

        private bool _Shutdown(InputData i)
        {
            _doShutdown = true;
            return true;
        }

        private bool _Commands(InputData i)
        {
            SendMessage("@wCommands in @Wcore@w:", i.ClientMask);
            int c = WriteCommands(InputHandler.Commands, "core", i.ClientMask);
            SendMessage("@C" + c + " @wcommand" + (c == 1 ? "" : "s") + " found.");

            foreach(KeyValuePair<string, Plugin> p in PluginMgr.Plugins)
            {
                SendMessage("", i.ClientMask);
                SendMessage("@wCommands in @W" + p.Key + "@w:", i.ClientMask);
                c = WriteCommands(InputHandler.Commands, p.Key, i.ClientMask);
                SendMessage("@C" + c + " @wcommand" + (c == 1 ? "" : "s") + " found.");
            }

            SendMessage("", i.ClientMask);
            SendMessage("@WThese are commands for the proxy. If you want to see MUD commands type command.", i.ClientMask);
            return true;
        }

        private int WriteCommands(SortedDictionary<string, InputEntry> y, string plugin, uint[] clientMask)
        {
            if(y == null || y.Count == 0)
                return 0;

            int c = 0;
            foreach(KeyValuePair<string, InputEntry> x in y)
            {
                if(x.Value.Plugin != plugin)
                    continue;
                if((x.Value.Flags & (CMDFlags.Disabled | CMDFlags.Hidden)) != CMDFlags.None)
                    continue;
                string cmd = "";
                InputEntry p = x.Value.Parent;
                while(p != null)
                {
                    cmd = cmd.Insert(0, p.Command + " ");
                    p = p.Parent;
                }
                SendMessage("@Y" + cmd, clientMask);
                c++;

                c += WriteCommands(x.Value.Subcommands, plugin, clientMask);
            }

            return c;
        }

        /// <summary>
        /// Game world instance.
        /// </summary>
        public static World Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Handle text line as if we received it from Aardwolf.
        /// </summary>
        /// <param name="Msg"></param>
        public void _OnReceived(string Msg)
        {
            TriggerHandler.HandleText(Msg, this);
        }

        public void _OnConnected(bool connected)
        {
            isWorldReady = false;
            foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
            {
#if !DEBUG
                try
                {
#endif
                    if(connected)
                        x.Value.OnConnect();
                    else
                        x.Value.OnDisconnect();
#if !DEBUG
                }
                catch(Exception e)
                {
                    Log.Crash(e, x.Key);
                }
#endif
            }
        }

        private bool _GMCPHP(TriggerData t)
        {
            if(!isWorldReady)
            {
                isWorldReady = true;
                foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
                {
#if !DEBUG
                    try
                    {
#endif
                        x.Value.OnLogin();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
                        Log.Crash(e, x.Key);
                    }
#endif
                }
                CheckUpdatesNow = MSTime + 5000;
            }
            return false;
        }

        private long CheckUpdatesNow = 0;

        private bool _LineInfo(InputData i)
        {
            int j = 10;
            if(i.Arguments.Success)
            {
                if(!int.TryParse(i.Arguments.Groups[1].Value, out j))
                    j = 10;
            }

            if(j > 100)
                j = 100;
            else if(j < 1)
                j = 1;

            SendMessage("@wDisplaying last @Y" + j + " @wline" + (j != 1 ? "s" : "") + ":", i.ClientMask);
            j = lastLine.Count - j;
            if(j < 0)
                j = 0;
            for(; j < lastLine.Count; j++)
                SendMessage(lastLine[j].Replace("@", "@@"), i.ClientMask);
            if(j == 10)
                SendMessage("@wType '@Wlastlines <nr>@w' to see <nr> amount of lines.", i.ClientMask);
            return true;
        }

        internal List<string> lastLine = new List<string>();

        private bool _PluginInfo(InputData i)
        {
            if(!i.Arguments.Success)
            {
                SendMessage("@wYou have the following plugins installed:", i.ClientMask);
                foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
                    SendMessage("@Y" + string.Format("{0,-20}", x.Value.Keyword.Trim()) + " @w- " + x.Value.Name + ", version " + x.Value.Version.ToString(), i.ClientMask);
                if(PluginMgr.Plugins.Count == 0)
                    SendMessage("@RYou have no plugins installed.", i.ClientMask);
                else
                {
                    SendMessage("", i.ClientMask);
                    SendMessage("@C" + PluginMgr.Plugins.Count + " @wplugin" + (PluginMgr.Plugins.Count != 1 ? "s" : "") + " found.");
                    SendMessage("@wUse '@Wplugin <keyword>@w' for more information about a plugin.", i.ClientMask);
                    SendMessage("@wUse '@Wplugin <keyword> full@w' for all information about a plugin.", i.ClientMask);
                    //SendMessage("@wUse '@Wplugin <keyword> write@w' to write all information about plugin to a file.", i.ClientMask);
                }
            }
            else
            {
                Plugin p = PluginMgr.Plugins.ContainsKey(i.Arguments.Groups[1].Value.ToLower().Trim()) ? PluginMgr.Plugins[i.Arguments.Groups[1].Value.ToLower().Trim()] : null;
                if(p == null)
                {
                    SendMessage("@wNo such plugin (@W" + i.Arguments.Groups[1].Value.ToLower().Trim() + "@w).", i.ClientMask);
                    SendMessage("@wType '@Wplugins@w' for a list of installed plugins.", i.ClientMask);
                    return true;
                }

                SendMessage("@w+----------------------------------------------------------------------+", i.ClientMask);
                SendMessage("@w|@R" + p.Name.Substring(0, p.Name.Length / 2).PadLeft(35, ' ') +
                    p.Name.Substring(p.Name.Length / 2).PadRight(35, ' ') + "@w|", i.ClientMask);
                SendMessage("@w+----------------------------------------------------------------------+", i.ClientMask);
                SendMessage("@w| @WKeyword     @w: @g" + string.Format("{0,-55}", p.Keyword) + "@w|", i.ClientMask);
                SendMessage("@w| @WAuthor      @w: @g" + string.Format("{0,-55}", !string.IsNullOrEmpty(p.Author) ? p.Author : "Unknown") + "@w|", i.ClientMask);
                SendMessage("@w| @WVersion     @w: @Y" + string.Format("{0,-55}", p.Version) + "@w|", i.ClientMask);
                if(!string.IsNullOrEmpty(p.Website))
                    SendMessage("@w| @WWebsite     @w: @Y" + string.Format("{0,-55}", p.Website) + "@w|", i.ClientMask);
                SendMessage("@w| @WClass name  @w: @g" + string.Format("{0,-55}", p.ClassName) + "@w|", i.ClientMask);
                if(!string.IsNullOrEmpty(p.Description))
                {
                    string[] desc = Utility.WrapColored(p.Description, 54, 0);
                    for(int j = 0; j < desc.Length; j++)
                    {
                        SendMessage(
                            j == 0
                                ? ("@w| @WDescription @w: @C" + string.Format("{0,-55}", desc[j]) + "@w|")
                                : ("@w|             : @C" + string.Format("{0,-55}", desc[j]) + "@w|"), i.ClientMask);
                    }
                }
                if(p.RequiredPlayerConfig.Count != 0)
                {
                    for(int j = 0; j < p.RequiredPlayerConfig.Count; j++)
                    {
                        SendMessage(
                            j == 0
                                ? ("@w| @WReq. config @w: " + string.Format("{0,-55}", p.RequiredPlayerConfig[j]) +
                                   "@w|")
                                : ("@w|             : " + string.Format("{0,-55}", p.RequiredPlayerConfig[j]) + "@w|"),
                            i.ClientMask);
                    }
                }
                SendMessage("@w+----------------------------------------------------------------------+", i.ClientMask);
                if(i.Arguments.Groups[2].Length > 0 || i.Arguments.Groups[3].Length > 0)
                {
                    int j = _PluginInfoWriteCommands(0, InputHandler.Commands, p.Keyword.ToLower().Trim(), i.ClientMask);

                    int k = 0;
                    foreach(KeyValuePair<string, TriggerEntry> x in TriggerHandler.TriggersName)
                    {
                        if(x.Value.Plugin != p.Keyword.ToLower().Trim())
                            continue;

                        if(k == 0)
                        {
                            SendMessage(
                                "@w| @WTriggers    @w: \"" +
                                Utility.FormatColoredString(x.Value.PatternStr.Replace("@", "@@") + "\"", -54) + "@w|",
                                i.ClientMask);
                        }
                        else
                        {
                            SendMessage(
                                "@w|             : \"" +
                                Utility.FormatColoredString(x.Value.PatternStr.Replace("@", "@@") + "\"", -54) + "@w|",
                                i.ClientMask);
                        }
                        k++;
                    }

                    if(j != 0 || k != 0)
                    {
                        SendMessage("@w+----------------------------------------------------------------------+",
                                    i.ClientMask);
                    }
                }
            }

            return true;
        }

        private int _PluginInfoWriteCommands(int j, SortedDictionary<string, InputEntry> y, string plugin, uint[] clientMask)
        {
            foreach(KeyValuePair<string, InputEntry> x in y)
            {
                if(x.Value.Plugin != plugin)
                    continue;

                string cmd = x.Key;
                InputEntry parent = x.Value.Parent;
                while(parent != null)
                {
                    cmd = cmd.Insert(0, parent.Command + " ");
                    parent = parent.Parent;
                }

                if(j == 0)
                    SendMessage("@w| @WCommands    @w: @c" + string.Format("{0,-55}", cmd) + "@w|", clientMask);
                else
                    SendMessage("@w|             : @c" + string.Format("{0,-55}", cmd) + "@w|", clientMask);
                j++;

                if(x.Value.Subcommands != null)
                    j = _PluginInfoWriteCommands(j, x.Value.Subcommands, plugin, clientMask);
            }

            return j;
        }

        private bool isWorldReady = false;

        /// <summary>
        /// Handle GMCP data that we received from Aardwolf.
        /// </summary>
        /// <param name="Data">GMCP data received.</param>
        public void _OnReceived(byte[] Data)
        {
            string Msg = Encoding.Default.GetString(Data);
            string Module = Msg.Trim().ToLower();
            if(Module.Contains(' '))
                Module = Module.Substring(0, Module.IndexOf(' '));
            Message m = new Message(true);
            m.Clients = null;
            m.Flags |= MessageFlags.GMCP;
            m.Msg = Module;
            m.MsgData = Data;
            _SendMessage(m);
            TriggerHandler.HandleGMCP(Msg);
        }

        /// <summary>
        /// This is called from proxy when it shuts down. Do NOT call from a plugin.
        /// </summary>
        public void Shutdown()
        {
            foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
            {
#if !DEBUG
                try
                {
#endif
                    x.Value.Shutdown();
#if !DEBUG
                }
                catch(Exception e)
                {
                    Log.Crash(e, x.Key);
                }
#endif
            }

            _doShutdown = true;
        }

        /// <summary>
        /// Enter input as if a client entered it. Meaning we parse it. Consider using the Execute command instead.
        /// </summary>
        /// <param name="Msg">Input entered.</param>
        /// <param name="ClientId">Which client is this from? Enter 0 to set not from a client.</param>
        /// <param name="AuthLevel">Authlevel of client who entered command (1...64)</param>
        public void _OnSent(string Msg, uint ClientId, int AuthLevel)
        {
            foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
            {
#if !DEBUG
                try
                {
#endif
                    x.Value.OnEnteredCommandBefore(ref Msg, ClientId, AuthLevel);
#if !DEBUG
                }
                catch(Exception e)
                {
                    Log.Crash(e, x.Key);
                }
#endif
                if(Msg == null)
                    return;
            }

            InputData i = InputHandler.HandleInput(Msg, Msg, ClientId, AuthLevel, null, this);
            if(i != null)
            {
#if !DEBUG
                try
                {
#endif
                    if(i.Function.Func(i))
                        return;
#if !DEBUG
                }
                catch(Exception e)
                {
                    Log.Crash(e, i.Function.Plugin);
                }
#endif
                Msg = i.Command;
            }

            if(Msg != null)
            {
                foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
                {
#if !DEBUG
                    try
                    {
#endif
                        x.Value.OnEnteredCommandAfter(ref Msg, ClientId, AuthLevel);
#if !DEBUG
                    }
                    catch(Exception e)
                    {
                        Log.Crash(e, x.Value.Keyword);
                    }
#endif
                    if(Msg == null)
                        return;
                }
                _SendMessage(Msg, new uint[] {0}, ClientId != 0);
            }
        }

        internal void _SendMessage(string Msg, uint[] Clients, bool Natural)
        {
            _SendMessage(Msg, Clients, Natural, MessageFlags.None);
        }

        internal void _SendMessage(string Msg, uint[] Clients, bool Natural, MessageFlags Flags)
        {
            _SendMessage(Msg, Clients, Natural, Flags, ulong.MaxValue);
        }

        internal void _SendMessage(string Msg, uint[] Clients, bool Natural, MessageFlags Flags, ulong AuthMask)
        {
            Message m = new Message(Natural);
            m.Clients = Clients;
            m.Msg = Msg;
            m.Flags = Flags;
            m.AuthMask = AuthMask;
            _SendMessage(m);
        }

        internal void _SendMessage(Message msg)
        {
            _MessageData.Add(msg);
        }

        /// <summary>
        /// Send message to specified clients.
        /// </summary>
        /// <param name="Msg">Message to send.</param>
        /// <param name="Clients">Clients to send it to. Enter null to send to all connected clients.
        /// Enter 0 as client to send it as a command to Aardwolf (we don't parse it though, if you want
        /// parsed input use the Execute command).</param>
        public void SendMessage(string Msg, uint[] Clients)
        {
            _SendMessage(Msg, Clients, false);
        }

        /// <summary>
        /// Send a message to all connected authorized clients.
        /// </summary>
        /// <param name="Msg">Message to send to all authorized clients.</param>
        public void SendMessage(string Msg)
        {
            SendMessage(Msg, null);
        }

        /// <summary>
        /// Send a message to all connected authorized clients.
        /// </summary>
        /// <param name="Msg">Message to send to all authorized clients.</param>
        /// <param name="AuthMask">Authorization levels required to see this message. This is a mask.</param>
        public void SendMessage(string Msg, ulong AuthMask)
        {
            _SendMessage(Msg, null, false, MessageFlags.None, AuthMask);
        }

        /// <summary>
        /// Execute a command.
        /// </summary>
        /// <param name="Msg">Command to execute.</param>
        /// <param name="allowParse">Allow parsing it for aliases and such, or send it directly to Aardwolf?</param>
        public void Execute(string Msg, bool allowParse)
        {
            Execute(Msg, allowParse, 1);
        }

        /// <summary>
        /// Execute a command.
        /// </summary>
        /// <param name="Msg">Command to execute.</param>
        /// <param name="allowParse">Allow parsing it for aliases and such, or send it directly to Aardwolf?</param>
        /// <param name="AuthLevel">Auth level that executes this command. (1...64)</param>
        public void Execute(string Msg, bool allowParse, int AuthLevel)
        {
            if(Msg == null)
                return;

            if(allowParse)
                _OnSent(Msg, uint.MaxValue, Math.Max(1, Math.Min(64, AuthLevel)));
            else
                _SendMessage(Msg, new uint[] { 0 }, false);
        }

        /// <summary>
        /// Send raw bytes to MUD.
        /// </summary>
        /// <param name="Data">Bytes to send.</param>
        public void Send(byte[] Data)
        {
            if(Data == null)
                return;

            Message m = new Message(false);
            m.Clients = new uint[] { 0 };
            m.MsgData = Data;
            _SendMessage(m);
        }

        /// <summary>
        /// Internal command for updating the world. DO NOT CALL FROM A PLUGIN!
        /// </summary>
        /// <param name="msTime">New mstime.</param>
        public bool Update(long msTime)
        {
            MSTime = msTime;
            foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
            {
#if !DEBUG
                try
                {
#endif
                    x.Value.Update(msTime);
#if !DEBUG
                }
                catch(Exception e)
                {
                    Log.Crash(e, x.Key);
                }
#endif
            }

            if(CheckUpdatesNow != 0 && msTime > CheckUpdatesNow)
            {
                CheckUpdatesNow = 0;
                CheckUpdates(Config.GetInt32("Updates.Core", 1) != 0, Config.GetInt32("Updates.Plugins", 1) != 0, false);
            }

            return _doShutdown;
        }

        private bool _doShutdown = false;

        /// <summary>
        /// This is messages for networking to handle. Don't touch unless you know what you are doing.
        /// </summary>
        public readonly List<Message> _MessageData = new List<Message>(256);

        internal CoreConfig Config = new CoreConfig();

        /// <summary>
        /// Version of Proxy.
        /// </summary>
        public const int Version = 7;
        public const string CoreUrl = "www.duckbat.com/plugins/update.core.txt";
        public const string CoreUrl2 = "code.google.com/p/proxymud/";

        private int GetVersion(string Url)
        {
            if(!_urlRegex.Match(Url).Success)
                Url = "http://" + Url;
            WebClient w = new WebClient();
            byte[] d = w.DownloadData(Url);
            string s = Encoding.Default.GetString(d);
            s = s.Replace("\r", "");
            s = s.Replace(" ", "");
            s = s.Replace("\n", "");
            return int.Parse(s);
        }

        private static Regex _urlRegex = new Regex(@"^\w+://", RegexOptions.Compiled);

        /// <summary>
        /// Milliseconds since program startup.
        /// </summary>
        public long MSTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Check for updates and report to all connected users if there are any.
        /// </summary>
        /// <param name="Core">Check core update.</param>
        /// <param name="Plugins">Check plugin updates.</param>
        /// <param name="ReportNoUpdates">Should we report if no updates were found?</param>
        public void CheckUpdates(bool Core, bool Plugins, bool ReportNoUpdates)
        {
#if DEBUG
            return;
#endif
            bool s = false;
            if(Core)
            {
                try
                {
                    int coreVer = GetVersion(CoreUrl);
                    if(coreVer > Version)
                    {
                        SendMessage("@GUPDATE: @wThere is a newer version of @Wcore @wavailable. (@W" + coreVer + "@w)");
                        SendMessage("        @wGo to @W" + CoreUrl2 + " @wto see more.");
                        s = true;
                    }
                }
                catch
                {
                }
            }

            if(Plugins)
            {
                foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
                {
                    if(string.IsNullOrEmpty(x.Value.Website) || string.IsNullOrEmpty(x.Value.UpdateUrl))
                        continue;

                    try
                    {
                        int ver = GetVersion(x.Value.UpdateUrl);
                        if(ver > x.Value.Version)
                        {
                            if(s)
                                SendMessage("");
                            SendMessage("@GUPDATE: @wThere is a newer version of @W" + x.Value.Keyword.ToLower().Trim() + " @wavailable. (@W" + ver + "@w)");
                            SendMessage("        @wGo to @W" + x.Value.Website + " @wto see more.");
                            s = true;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            if(ReportNoUpdates && !s)
                SendMessage("@GUPDATE: @wNo updates found.");
        }
    }
}
