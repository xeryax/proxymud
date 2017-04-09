using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProxyMud.Network
{
    internal enum TelnetOpcodes : byte
    {
        SE = 240,
        SB = 250,
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255,

        TTYPE = 24,
        MCCP_V1 = 85,
        MCCP_V2 = 86,
        GMCP = 201,
    }

    internal class TelnetPacket
    {
        internal TelnetStates State;
        internal TelnetOpcodes Header;
        internal TelnetOpcodes Type;
        internal MemoryStream Data;
        internal bool HadIAC = false;
    }

    internal enum TelnetStates
    {
        /// <summary>
        /// Just received IAC, nothing else.
        /// </summary>
        None,

        /// <summary>
        /// Have header (what packet does).
        /// </summary>
        Header,

        /// <summary>
        /// Have type of packet.
        /// </summary>
        Type,

        /// <summary>
        /// Gathering data right now waiting for IAC SE.
        /// </summary>
        Data,

        /// <summary>
        /// Finished packet.
        /// </summary>
        End,
    }
}
