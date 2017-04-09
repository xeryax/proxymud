using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using ComponentAce.Compression.Libs.zlib;
using ProxyCore;
using System.Text.RegularExpressions;

namespace ProxyMud.Network
{
    internal class NetworkAardwolf : NetworkBase
    {
        internal NetworkAardwolf(Socket socket, NetworkServer server)
            : base(socket, server, 131072)
        {
            
        }

        #region Variables
        protected string LineBuffer = string.Empty;
        protected long LineBufferTimeout = 0;
        #endregion

        #region Packet
        protected override void HandlePacket()
        {
            if(inIndex >= inMaxIndex)
            {
                if(inStream.Length > 0)
                {
                    OnReceived(Encoding.Default.GetString(inStream.ToArray()), inMaxIndex >= 1024);
                    inStream.SetLength(0);
                }
                return;
            }

            if(zStream != null)
            {
                Decompress(zlibConst.Z_FULL_FLUSH);
                HandlePacket(zStream_Out, 0, zStream_Length);
                HandlePacket();
                return;
            }

            inIndex = HandlePacket(inData, inIndex, inMaxIndex);
            HandlePacket();
        }

        protected override void WriteInStream(byte[] Buf, int Index, int MaxIndex)
        {
            inStream.Write(Buf, Index, MaxIndex - Index);
        }
        #endregion

        private StringBuilder strLine = new StringBuilder(8192);

        protected override void OnReceived(string Msg, bool bigPacket)
        {
            if(LineBuffer.Length != 0)
            {
                Msg = LineBuffer + Msg;
                LineBuffer = string.Empty;
            }

            while(Msg.Length > 0)
            {
                strLine.Remove(0, strLine.Length);
                int k = 0;
                int i = 0;
                for(; i < Msg.Length; i++)
                {
                    if(Msg[i] == '\r')
                        k |= 1;
                    else if(Msg[i] == '\n')
                        k |= 2;
                    else
                        strLine.Append(Msg[i]);

                    if((k & 3) == 3)
                    {
                        i++;
                        break;
                    }
                }

                Msg = Msg.Substring(i);
                if((k & 3) != 3)
                {
                    LineBuffer = strLine.ToString();
                    LineBufferTimeout = LastMSTime + (!bigPacket ? -1 : 500);
                }
                else
                {
                    Server.World._OnReceived(Colors.FixColors(strLine.ToString(), true, true));
                }
            }
        }

        internal override void Update(long msTime)
        {
            base.Update(msTime);

            if(LineBuffer.Length != 0 && LineBufferTimeout < msTime)
            {
                string Msg = LineBuffer;
                LineBuffer = string.Empty;
                OnReceived(Msg + "\n\r", false);
            }
        }

        protected override void HandlePacket(TelnetPacket pkt)
        {
            base.HandlePacket(pkt);

            if(pkt.Type == TelnetOpcodes.GMCP && pkt.Header == TelnetOpcodes.SB && pkt.Data.Length > 0)
            {
                if(inStream.Length != 0)
                {
                    OnReceived(Encoding.Default.GetString(inStream.ToArray()), false);
                    inStream.SetLength(0);
                }
                Server.World._OnReceived(pkt.Data.ToArray());
                return;
            }

            if(pkt.Type == TelnetOpcodes.GMCP && pkt.Header == TelnetOpcodes.WILL)
            {
                Send(new[] { (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.DO, (byte)TelnetOpcodes.GMCP });
                SendGMCP(Encoding.Default.GetBytes("Core.Hello { \"client\": \"ProxyMud\", \"version\": \"" + World.Version + "\" }"));
                {
                    if(Server.GMCPModules.Count > 0)
                    {
                        StringBuilder strModule = new StringBuilder();
                        foreach(KeyValuePair<string, int> x in Server.GMCPModules)
                        {
                            if(strModule.Length > 0)
                                strModule.Append(", ");
                            strModule.Append("\"" + x.Key + " " + x.Value.ToString() + "\"");
                        }
                        SendGMCP(Encoding.Default.GetBytes("Core.Supports.Set [ " + strModule.ToString() + " ]"));
                    }
                }
                return;
            }

            if(pkt.Type == TelnetOpcodes.TTYPE)
            {
                NetworkClient c = null;
                foreach(NetworkClient x in Server.Clients)
                {
                    if(x.AuthLevel >= 1 && (c == null || x.AuthLevel > c.AuthLevel))
                        c = x;
                }
                if(c != null)
                    c.Send(pkt);
                return;
            }

            if(pkt.Type == TelnetOpcodes.MCCP_V2)
            {
                if(pkt.Header == TelnetOpcodes.WILL)
                    Send(new[] { (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.DO, (byte)TelnetOpcodes.MCCP_V2 });
                return;
            }

            if(pkt.Type == TelnetOpcodes.MCCP_V1)
            {
                if(pkt.Header == TelnetOpcodes.WILL)
                    Send(new[] { (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.DONT, (byte)TelnetOpcodes.MCCP_V1 });
                return;
            }

            foreach(NetworkClient x in Server.Clients)
            {
                if(x.AuthLevel >= 1)
                    x.Send(pkt);
            }
        }

        internal void SendGMCP(byte[] Data)
        {
            byte[] b = new[] { (byte)TelnetOpcodes.IAC,
                (byte)TelnetOpcodes.SB,
                (byte)TelnetOpcodes.GMCP };
            byte[] c = new[] { (byte)TelnetOpcodes.IAC,
                (byte)TelnetOpcodes.SE };
            Send(b.Concat(Data).Concat(c).ToArray());
        }
    }
}
