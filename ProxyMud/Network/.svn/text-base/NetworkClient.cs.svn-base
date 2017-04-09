using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using ComponentAce.Compression.Libs.zlib;
using System.IO;

namespace ProxyMud.Network
{
    internal class NetworkClient : NetworkBase
    {
        internal NetworkClient(Socket socket, NetworkServer server)
            : base(socket, server, 4096)
        {
            Id = ++_highId;
        }

        #region Variables
        private static uint _highId = 0;
        internal readonly uint Id;
        internal List<string> GMCPModules = new List<string>();
        internal int AuthLevel = -1;
        private TelnetOpcodes CompressionType = TelnetOpcodes.IAC;
        #endregion

        #region Packet
        protected override void HandlePacket()
        {
            if(inIndex >= inMaxIndex)
                return;

            inIndex = HandlePacket(inData, inIndex, inMaxIndex);
            HandlePacket();
        }
        protected override void WriteInStream(byte[] Buf, int Index, int MaxIndex)
        {
            for(int i = Index; i < MaxIndex; i++)
            {
                if(Buf[i] == 0x8)
                {
                    if(inStream.Length > 0)
                        inStream.SetLength(inStream.Length - 1);
                }
                else if(Buf[i] == 0xA)
                {
                    continue;
                }
                else if(Buf[i] == 0xD)
                {
                    OnReceived(Encoding.Default.GetString(inStream.ToArray()), false);
                    inStream.SetLength(0);
                }
                else
                    inStream.WriteByte(Buf[i]);
            }
        }
        #endregion

        protected override void OnReceived(string Msg, bool bigPacket)
        {
            if(AuthLevel >= 1)
            {
                Server.World._OnSent(Msg, Id, AuthLevel);
                return;
            }

            if(Server.Passwords.ContainsKey(Msg))
            {
                AuthLevel = Server.Passwords[Msg];
                OnAuthed();
            }
        }

        internal bool HasGMCPModule(string mod)
        {
            if(GMCPModules.Contains(mod))
                return true;

            string[] m = mod.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            string n = string.Empty;
            for(int i = 0; i < m.Length; i++)
            {
                n += (i != 0 ? "." : "") + m[i];
                if(GMCPModules.Contains(n))
                    return true;
            }

            return false;
        }

        internal void OnAuthed()
        {
            Send(new[] { (byte)TelnetOpcodes.IAC,
                    (byte)TelnetOpcodes.WILL,
                    (byte)TelnetOpcodes.GMCP });
            Send(Encoding.Default.GetBytes("You are now connected to ProxyMud.\n\r"));
        }

        protected override void HandlePacket(TelnetPacket pkt)
        {
            base.HandlePacket(pkt);

            if(pkt.Header == TelnetOpcodes.SB && pkt.Type == TelnetOpcodes.GMCP && pkt.Data.Length > 0)
            {
                string Msg = Encoding.Default.GetString(pkt.Data.ToArray()).ToLower();
                if(!Msg.StartsWith("core.supports.") && !Msg.StartsWith("core.hello"))
                {
                    if(Server.Aardwolf != null)
                        Server.Aardwolf.SendGMCP(pkt.Data.ToArray());
                }
                else if(Msg.StartsWith("core.supports."))
                {
                    try
                    {
                        Msg = Msg.Substring("core.supports.".Length);
                        if(Msg.StartsWith("add "))
                        {
                            string[] v =
                                Msg.ToLower().Substring(Msg.IndexOf(' ') + 1).Replace("[", "").Replace("]", "").Replace(
                                    "\"", "").Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach(string x in v)
                            {
                                if(!x.EndsWith("0"))
                                {
                                    if(!GMCPModules.Contains(x.Substring(0, x.Length - 1)))
                                    {
                                        GMCPModules.Add(x.Substring(0, x.Length - 1));
                                        string b = x.Substring(0, x.Length - 1);
                                        if(b.Contains('.'))
                                            b = b.Substring(0, b.IndexOf('.'));
                                        if(!Server.GMCPModules.ContainsKey(b) || Server.GMCPModules[b] == 0)
                                        {
                                            if(Server.Aardwolf != null)
                                                Server.Aardwolf.SendGMCP(Encoding.Default.GetBytes("Core.Supports.Add [ \"" + b + " " + x[x.Length - 1].ToString() + "\" ]"));
                                        }
                                    }
                                }
                                else
                                    GMCPModules.Remove(x.Substring(0, x.Length - 1));
                            }
                        }
                        else if(Msg.StartsWith("set "))
                        {
                            string[] v =
                                Msg.ToLower().Substring(Msg.IndexOf(' ') + 1).Replace("[", "").Replace("]", "").Replace(
                                    "\"", "").Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach(string x in v)
                            {
                                if(!x.EndsWith("0"))
                                {
                                    if(!GMCPModules.Contains(x.Substring(0, x.Length - 1)))
                                    {
                                        GMCPModules.Add(x.Substring(0, x.Length - 1));
                                        string b = x.Substring(0, x.Length - 1);
                                        if(b.Contains('.'))
                                            b = b.Substring(0, b.IndexOf('.'));
                                        if(!Server.GMCPModules.ContainsKey(b) || Server.GMCPModules[b] == 0)
                                        {
                                            if(Server.Aardwolf != null)
                                                Server.Aardwolf.SendGMCP(Encoding.Default.GetBytes("Core.Supports.Add [ \"" + b + " " + x[x.Length - 1].ToString() + "\" ]"));
                                        }
                                    }
                                }
                                else
                                    GMCPModules.Remove(x.Substring(0, x.Length - 1));
                            }
                        }
                        else if(Msg.StartsWith("remove "))
                        {
                            string[] v =
                                Msg.ToLower().Substring(Msg.IndexOf(' ') + 1).Replace("[", "").Replace("]", "").Replace(
                                    "\"", "").Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach(string x in v)
                                GMCPModules.Remove(x);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if(pkt.Type == TelnetOpcodes.GMCP)
                return;

            if(pkt.Type == TelnetOpcodes.MCCP_V2)
            {
                if(pkt.Header == TelnetOpcodes.DO)
                {
                    if(StartCompression(TelnetOpcodes.MCCP_V2))
                        CompressionType = TelnetOpcodes.MCCP_V2;
                }
                else if(pkt.Header == TelnetOpcodes.DONT)
                {
                    if(CompressionType == TelnetOpcodes.MCCP_V2)
                    {
                        EndCompression();
                        CompressionType = TelnetOpcodes.IAC;
                    }
                }
                return;
            }

            if(pkt.Type == TelnetOpcodes.MCCP_V1)
            {
                if(pkt.Header == TelnetOpcodes.DO)
                {
                    if(StartCompression(TelnetOpcodes.MCCP_V1))
                        CompressionType = TelnetOpcodes.MCCP_V1;
                }
                else if(pkt.Header == TelnetOpcodes.DONT)
                {
                    if(CompressionType == TelnetOpcodes.MCCP_V1)
                    {
                        EndCompression();
                        CompressionType = TelnetOpcodes.IAC;
                    }
                }
                return;
            }

            if(Server.Aardwolf != null)
                Server.Aardwolf.Send(pkt);
        }
    }
}
