using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using ComponentAce.Compression.Libs.zlib;
using ProxyCore;

namespace ProxyMud.Network
{
    internal class NetworkBase
    {
        protected NetworkBase(Socket socket, NetworkServer server, int inBufferSize)
        {
            Server = server;
            Socket = socket;
            inStream = new MemoryStream(inBufferSize);
            Socket.Blocking = false;
        }

        #region Variables
        protected readonly NetworkServer Server;
        protected Socket Socket;
        protected MemoryStream inStream;
        protected TelnetPacket telnetPacket;
        protected ZStream zStream;
        protected static int zStream_Length;
        protected static byte[] zStream_Out = new byte[65536];
        protected static int inIndex;
        protected static int inMaxIndex;
        protected static byte[] inData = new byte[65536];
        #endregion

        #region Networking
        internal bool Receive()
        {
            if(Socket == null)
                return false;

            SocketError err;
            inMaxIndex = Socket.Receive(inData, 0, inData.Length, SocketFlags.None, out err);
            if(err != SocketError.WouldBlock && inMaxIndex == 0)
            {
                Socket.Close();
                Socket = null;
                return false;
            }

            if(inMaxIndex == 0)
                return true;

            inIndex = 0;
            HandlePacket();
            return true;
        }

        internal void Send(byte[] Data)
        {
            if(Socket == null)
                return;

            if(zStream != null && this is NetworkClient)
            {
                Compress(Data, zlibConst.Z_FULL_FLUSH);
                if(zStream_Length > 0)
                    Socket.Send(zStream_Out, zStream_Length, SocketFlags.None);
            }
            else
                Socket.Send(Data, SocketFlags.None);
        }

        internal void Send(TelnetPacket pkt)
        {
            if(Socket == null)
                return;

            if(pkt.Header == TelnetOpcodes.SB && pkt.Data.Length != 0)
                Send(new[] { (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.SB, (byte)pkt.Type }.Concat(
                    pkt.Data.ToArray()).Concat(new[] { (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.SE }).ToArray());
            else
                Send(new[] { (byte)TelnetOpcodes.IAC, (byte)pkt.Header, (byte)pkt.Type });
        }

        internal void Disconnect()
        {
            if(Socket == null)
                return;

            Socket.Close();
            Socket = null;
        }
        #endregion

        #region Compression
        protected bool StartCompression(TelnetOpcodes type)
        {
            if(this is NetworkAardwolf)
                throw new Exception("Trying to start compression on Aardwolf connection!");

            if(zStream != null)
                return false;

            if(type == TelnetOpcodes.MCCP_V1)
                Send(new[] { (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.SB, (byte)TelnetOpcodes.MCCP_V1,
                    (byte)TelnetOpcodes.WILL, (byte)TelnetOpcodes.SE });
            else
                Send(new[] { (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.SB, (byte)TelnetOpcodes.MCCP_V2,
                    (byte)TelnetOpcodes.IAC, (byte)TelnetOpcodes.SE });

            zStream = new ZStream();
            zStream.deflateInit(6);
            return true;
        }

        protected void EndCompression()
        {
            if(this is NetworkAardwolf)
                throw new Exception("Trying to end compression on Aardwolf connection!");

            if(zStream == null)
                return;

            byte[] d = new byte[0];
            Compress(d, zlibConst.Z_FINISH);
            if(zStream_Length > 0)
                Socket.Send(zStream_Out, zStream_Length, SocketFlags.None);

            zStream.deflateEnd();
            zStream.free();
            zStream = null;
        }

        private void StartDecompression()
        {
            if(this is NetworkClient)
                throw new Exception("Trying to start decompression on Client connection!");

            if(zStream != null)
                return;

            zStream = new ZStream();
            zStream.inflateInit();
        }

        internal void Compress(byte[] Data, int type)
        {
            if(this is NetworkAardwolf)
                throw new Exception("Trying to compress data on Aardwolf connection!");

            zStream_Length = 0;

            zStream.avail_in = Data.Length;
            zStream.next_in = Data;
            zStream.next_in_index = 0;

            zStream.next_out = zStream_Out;
            zStream.next_out_index = 0;
            zStream.avail_out = zStream_Out.Length;

            switch(zStream.deflate(type))
            {
                case zlibConst.Z_OK:
                    //inIndex = zStream.next_in_index;
                    zStream_Length = (int)zStream.total_out;
                    zStream.total_out = 0;
                    zStream.next_in = null;
                    break;

                case zlibConst.Z_STREAM_END:
                    //inIndex = zStream.next_in_index;
                    zStream_Length = (int)zStream.total_out;
                    zStream.deflateEnd();
                    zStream.free();
                    zStream = null;
                    break;

                default:
                    //case zlibConst.Z_STREAM_ERROR:
                    throw new Exception("Error while compressing: " + (zStream.msg ?? "unknown error") + "!");
            }
        }

        internal void Decompress(int type)
        {
            if(this is NetworkClient)
                throw new Exception("Trying to decompress data on Client connection!");

            zStream_Length = 0;

            zStream.avail_in = inMaxIndex - inIndex;
            zStream.next_in = inData;
            zStream.next_in_index = inIndex;

            zStream.next_out = zStream_Out;
            zStream.next_out_index = 0;
            zStream.avail_out = zStream_Out.Length;

            switch(zStream.inflate(type))
            {
                case zlibConst.Z_OK:
                    inIndex = zStream.next_in_index;
                    zStream_Length = (int)zStream.total_out;
                    zStream.total_out = 0;
                    break;

                case zlibConst.Z_STREAM_END:
                    inIndex = zStream.next_in_index;
                    zStream_Length = (int)zStream.total_out;
                    zStream.inflateEnd();
                    zStream.free();
                    zStream = null;
                    break;

                default:
                    //case zlibConst.Z_STREAM_ERROR:
                    throw new Exception("Error while decompressing: " + (zStream.msg ?? "unknown error") + "!");
            }
        }
        #endregion

        #region Packet
        protected virtual void HandlePacket()
        {

        }

        protected virtual void HandlePacket(TelnetPacket pkt)
        {

        }

        protected int HandlePacket(byte[] Buf, int Index, int MaxIndex)
        {
            if(Index >= MaxIndex)
                return MaxIndex;

            if(telnetPacket != null)
            {
                switch(telnetPacket.State)
                {
                    case TelnetStates.None:
                        telnetPacket.Header = (TelnetOpcodes)Buf[Index];
                        telnetPacket.State = TelnetStates.Header;
                        return HandlePacket(Buf, Index + 1, MaxIndex);

                    case TelnetStates.Header:
                        telnetPacket.Type = (TelnetOpcodes)Buf[Index];
                        telnetPacket.State = telnetPacket.Header == TelnetOpcodes.SB ? TelnetStates.Data : TelnetStates.End;
                        if(telnetPacket.State == TelnetStates.End)
                        {
                            HandlePacket(telnetPacket);
                            telnetPacket = null;
                        }
                        else
                            telnetPacket.Data = new MemoryStream(512);
                        return HandlePacket(Buf, Index + 1, MaxIndex);

                    case TelnetStates.Data:
                        {
                            for(int i = Index; i < MaxIndex; i++)
                            {
                                if(telnetPacket.HadIAC && Buf[i] == (byte)TelnetOpcodes.SE)
                                {
                                    telnetPacket.State = TelnetStates.End;
                                    HandlePacket(telnetPacket);
                                    if(zStream == null && (telnetPacket.Type == TelnetOpcodes.MCCP_V1 || telnetPacket.Type == TelnetOpcodes.MCCP_V2))
                                    {
                                        telnetPacket = null;
                                        StartDecompression();
                                        return i + 1;
                                    }
                                    telnetPacket = null;
                                    return HandlePacket(Buf, i + 1, MaxIndex);
                                }
                                if(Buf[i] == (byte)TelnetOpcodes.IAC || (Buf[i] == (byte)TelnetOpcodes.WILL && telnetPacket.Type == TelnetOpcodes.MCCP_V1))
                                {
                                    if(!telnetPacket.HadIAC)
                                    {
                                        if(i - Index > 0)
                                        {
                                            telnetPacket.Data.Write(Buf, Index, i - Index);
                                            Index = i;
                                        }
                                        telnetPacket.HadIAC = true;
                                    }
                                    else
                                        telnetPacket.Data.Write(new[] { (byte)TelnetOpcodes.IAC }, 0, 1);
                                }
                                else
                                {
                                    if(telnetPacket.HadIAC)
                                    {
                                        telnetPacket.HadIAC = false;
                                        telnetPacket.Data.Write(new[] { (byte)TelnetOpcodes.IAC }, 0, 1);
                                    }
                                }
                            }

                            if(Index < MaxIndex)
                            {
                                if(Buf[MaxIndex - 1] != (byte)TelnetOpcodes.IAC)
                                    telnetPacket.Data.Write(Buf, Index, MaxIndex - Index);
                            }
                            return MaxIndex;
                        }
                }
            }

            for(int i = Index; i < MaxIndex; i++)
            {
                if(Buf[i] == (byte)TelnetOpcodes.IAC)
                {
                    if(i - Index > 0)
                        WriteInStream(Buf, Index, i);
                    telnetPacket = new TelnetPacket();
                    return HandlePacket(Buf, i + 1, MaxIndex);
                }
            }

            if(MaxIndex - Index > 0)
                WriteInStream(Buf, Index, MaxIndex);
            return MaxIndex;
        }

        protected virtual void WriteInStream(byte[] Buf, int Index, int MaxIndex)
        {
            
        }

        protected virtual void OnReceived(string Msg, bool bigPacket)
        {

        }
        #endregion

        protected long LastMSTime;

        internal virtual void Update(long msTime)
        {
            LastMSTime = msTime;
        }
    }
}
