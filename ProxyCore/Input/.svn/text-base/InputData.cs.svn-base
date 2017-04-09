using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyCore.Input
{
    public class InputData
    {
        internal InputData()
        {
        }

        /// <summary>
        /// Who executed the command? This will be uint.MaxValue if we execute it from a plugin or other places - meaning
        /// it didn't originate from a client.
        /// </summary>
        public uint ClientId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Use this if you want to send message to whoever executed the command. If it was executed from a plugin
        /// send message to every client. Example: World.Instance.SendMessage("Test.", i.ClientMask);
        /// </summary>
        public uint[] ClientMask
        {
            get
            {
                return ClientId != uint.MaxValue ? new[] { ClientId } : null;
            }
        }

        /// <summary>
        /// The auth level of client who entered command. (1...64)
        /// </summary>
        public int ClientAuthLevel
        {
            get;
            internal set;
        }

        /// <summary>
        /// Whole command just as it was entered. You can change this to send something different to MUD. Just make
        /// sure you return false on the command handler otherwise nothing will get sent to MUD.
        /// </summary>
        public string Command;

        /// <summary>
        /// Which function will we execute with this data.
        /// </summary>
        internal InputEntry Function;

        /// <summary>
        /// This is where we capture arguments if the arguments pattern was set. Check first if Arguments.Success, otherwise there will be no Groups set.
        /// </summary>
        public Match Arguments
        {
            get;
            internal set;
        }
    }
}
