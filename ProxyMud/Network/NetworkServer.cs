using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore;
using System.Net.Sockets;
using System.IO;
using System.Net;
using ProxyCore.Messages;
using System.Text.RegularExpressions;

namespace ProxyMud.Network
{
    internal class NetworkServer
    {
        internal NetworkServer()
        {
            //string support = Program.Config.GetString("GMCP.Supports", "Core=1, Char=1, Room=1, Comm=1, Rawcolor=1");
            string support = Program.Config.GetString("GMCP.Supports", "Core=1, Char=1, Room=1, Comm=1");
            Match m;
            while((m = _loadSupport.Match(support)).Success)
            {
                support = support.Substring(m.Groups[0].Length);
                int i;
                if(!int.TryParse(m.Groups[2].Value, out i))
                    continue;
                GMCPModules[m.Groups[1].Value.ToLower()] = i;
            }
        }

        private static readonly Regex _loadSupport = new Regex(@"^([\w\.]+)\s*=\s*(\d+),?\s*", RegexOptions.Compiled);
        internal Dictionary<string, int> GMCPModules = new Dictionary<string, int>();
        internal World World = new World();
        internal List<NetworkClient> Clients = new List<NetworkClient>();
        internal NetworkAardwolf Aardwolf = null;
        internal TcpListener Server = null;
        private readonly MemoryStream strMessage = new MemoryStream(131072);
        internal Dictionary<string, int> Passwords = new Dictionary<string, int>();
        private static Regex _loadPw = new Regex(@"(.+?)\s*->\s*(\d+),?\s*", RegexOptions.Compiled);

        internal void Start()
        {
            IPAddress ip = IPAddress.Parse(Program.Config.GetString("Listen.Address", "127.0.0.1"));
            if(Server == null)
                Server = new TcpListener(ip, Program.Config.GetInt32("Listen.Port", 4000));
            {
                Passwords.Clear();
                string pw = Program.Config.GetString("Passwords", "");
                if(!string.IsNullOrEmpty(pw))
                {
                    Match m;
                    while((m = _loadPw.Match(pw)).Success)
                    {
                        pw = pw.Substring(m.Groups[0].Value.Length);
                        int i;
                        if(!int.TryParse(m.Groups[2].Value, out i))
                            continue;
                        if(i < 1)
                            i = 1;
                        else if(i > 64)
                            i = 64;
                        Passwords[m.Groups[1].Value] = i;
                    }
                }
            }
            Server.Start();

            Log.Write("Starting core, version " + World.Version + ".");
            Log.Write("Waiting for connections on " + Program.Config.GetString("Listen.Address", "127.0.0.1") + ":" + Program.Config.GetInt32("Listen.Port", 4000) + ".");
        }

        internal void Stop()
        {
            try
            {
                Server.Stop();
            }
            catch
            {
            }

            World.Shutdown();
        }

        internal void DisconnectAll()
        {
            if(Aardwolf != null)
            {
                Aardwolf.Disconnect();
                Aardwolf = null;
                World._OnConnected(false);
            }

            foreach(NetworkClient x in Clients)
                x.Disconnect();
            Clients.Clear();
        }

        internal bool Update(long msTime)
        {
            if(Server != null && Server.Pending())
            {
                NetworkClient c = new NetworkClient(Server.AcceptSocket(), this);
                Clients.Add(c);
                if(Program.Config.GetInt32("ClientCompression", 1) != 0)
                {
                    c.Send(new[]
                               {
                                   (byte) TelnetOpcodes.IAC,
                                   (byte) TelnetOpcodes.WILL,
                                   (byte) TelnetOpcodes.MCCP_V2,
                                   (byte) TelnetOpcodes.IAC,
                                   (byte) TelnetOpcodes.WILL,
                                   (byte) TelnetOpcodes.MCCP_V1
                               });
                }
                if(Passwords.Count != 0)
                    c.Send(Encoding.Default.GetBytes("Proxy password?\n\r"));
                else
                {
                    c.AuthLevel = 1;
                    c.OnAuthed();
                }
            }

            bool r = World.Update(msTime);

            if(Aardwolf != null)
            {
                if(!Aardwolf.Receive())
                {
                    Aardwolf = null;
                    World._OnConnected(false);
                }
                else
                    Aardwolf.Update(msTime);
            }

            bool hadAuthedClient = false;
            for(int i = Clients.Count - 1; i >= 0; i--)
            {
                if(!Clients[i].Receive())
                {
                    Clients.RemoveAt(i);
                    continue;
                }
                
                if(Clients[i].AuthLevel >= 1)
                    hadAuthedClient = true;
            }

            if(Aardwolf == null && (hadAuthedClient || Program.Config.GetInt32("AutoConnect", 0) != 0))
            {
                Log.Write("Connecting to " + Program.Config.GetString("MUD.Address", "aardmud.org") + ":" + Program.Config.GetInt32("MUD.Port", 4000).ToString());
                try
                {
                    TcpClient t = new TcpClient(Program.Config.GetString("MUD.Address", "aardmud.org"),
                                                Program.Config.GetInt32("MUD.Port", 4000));
                    Aardwolf = new NetworkAardwolf(t.Client, this);
                    World._OnConnected(true);
                }
                catch(Exception e)
                {
                    Log.Write("Failed connection to Aardwolf: " + e.Message);
                }
            }

            if(Aardwolf != null)
            {
                if(strMessage.Length > 0)
                    strMessage.SetLength(0);
                foreach(Message m in World._MessageData)
                {
                    if(m.Clients == null || !m.Clients.Contains((uint)0))
                        continue;

                    if((m.Flags & MessageFlags.GMCP) != MessageFlags.None)
                    {
                        if(m.MsgData == null || m.MsgData.Length == 0)
                            continue;

                        strMessage.Write(new byte[] {
                                (byte)TelnetOpcodes.IAC,
                                (byte)TelnetOpcodes.SB,
                                (byte)TelnetOpcodes.GMCP
                                    }, 0, 3);
                        strMessage.Write(m.MsgData, 0, m.MsgData.Length);
                        strMessage.Write(new byte[] {
                                (byte)TelnetOpcodes.IAC,
                                (byte)TelnetOpcodes.SE}, 0, 2);
                    }
                    else
                    {
                        byte[] data = m.MsgData ?? Encoding.Default.GetBytes(m.Msg + m.LineEnding);
                        strMessage.Write(data, 0, data.Length);
                    }
                }

                if(strMessage.Length != 0)
                    Aardwolf.Send(strMessage.ToArray());
            }

            for(int i = Clients.Count - 1; i >= 0; i--)
            {
                if(strMessage.Length > 0)
                    strMessage.SetLength(0);

                Clients[i].Update(msTime);

                if(Clients[i].AuthLevel < 1)
                    continue;

                foreach(Message m in World._MessageData)
                {
                    if(m.Clients != null && !m.Clients.Contains(Clients[i].Id))
                        continue;

                    if((m.AuthMask & ((ulong)1 << (Clients[i].AuthLevel - 1))) == 0)
                        continue;
//The problem is between here and next comment -- or at least this is what controls the GMCP shit that gets sent to the client.
                    if((m.Flags & MessageFlags.GMCP) != MessageFlags.None)
                    {
                        if(!Clients[i].HasGMCPModule(m.Msg.ToLower()))
                            continue;

                        if(m.MsgData == null || m.MsgData.Length == 0)
                            continue;

                        strMessage.Write(new[] {
                            (byte)TelnetOpcodes.IAC,
                            (byte)TelnetOpcodes.SB,
                            (byte)TelnetOpcodes.GMCP
                        }, 0, 3);
                        strMessage.Write(m.MsgData, 0, m.MsgData.Length);
                        strMessage.Write(new[] {
                            (byte)TelnetOpcodes.IAC,
                            (byte)TelnetOpcodes.SE}, 0, 2);
                    }
                    else
                    {
                        string msg = m.Msg;
                        msg = Colors.FixColors(msg, false, true);
                        byte[] data = Encoding.Default.GetBytes(msg + m.LineEnding);
                        strMessage.Write(data, 0, data.Length);
                    }
//BLAH
                }

                if(strMessage.Length == 0)
                    continue;

                Clients[i].Send(strMessage.ToArray());
            }

            World._MessageData.Clear();
            return r;
        }
    }
}
