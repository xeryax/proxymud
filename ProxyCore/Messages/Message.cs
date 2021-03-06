﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyCore.Messages
{
    public class Message
    {
        internal Message(bool isNaturalMessage)
        {
            IsNaturalMessage = isNaturalMessage;
        }

        /// <summary>
        /// Contents of the message.
        /// </summary>
        public string Msg
        {
            get
            {
                return _msg;
            }
            set
            {
                _msg = value;
                MsgNoColor = _msg == null ? null : Colors.RemoveColors(_msg, false);
            }
        }

        private string _msg;

        /// <summary>
        /// If this is set ignore message and send this byte data instead. Color codes
        /// will not be parsed by server and this is sent as is.
        /// </summary>
        public byte[] MsgData = null;

        /// <summary>
        /// This is message but without colors. Used for triggering non-ansi triggers.
        /// </summary>
        internal string MsgNoColor = null;

        /// <summary>
        /// Which clients should receive the message. This is a mask for security levels.
        /// For example value "3" would only send this message to security level 1 and 2.
        /// Set ulong.MaxValue (default) to send to all clients or 0 to send to noone.
        /// This field is ignored when sending message to Aardwolf (as a command).
        /// </summary>
        public ulong AuthMask = ulong.MaxValue;

        /// <summary>
        /// This setting is used to send the message to specific clients (using client ID).
        /// Enter new uint[] { 0 } to send to Aardwolf and null to send to all clients (default).
        /// </summary>
        public uint[] Clients = null;

        /// <summary>
        /// Is this message natural (client entered command or Aardwolf sent message) or
        /// is it generated by us, meaning Aardwolf did not send this and client did not enter
        /// it as a command.
        /// </summary>
        public readonly bool IsNaturalMessage;

        /// <summary>
        /// What kind of line ending do we send with this message (default \n\r).
        /// </summary>
        public string LineEnding = "\n\r";

        /// <summary>
        /// When was message generated (send time may vary by some milliseconds)
        /// </summary>
        public readonly DateTime Timestamp = DateTime.Now;

        /// <summary>
        /// Options for message.
        /// </summary>
        public MessageFlags Flags = MessageFlags.None;
    }

    [Flags]
    public enum MessageFlags
    {
        None = 0,

        /// <summary>
        /// This is a GMCP message the main module will be in Msg and the data will be in MsgData.
        /// </summary>
        GMCP = 0x1,
    }
}
