using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyCore.Input
{
    internal class InputEntry
    {
        internal string Command;
        internal CmdFunction Func;
        internal CMDFlags Flags;
        internal InputEntry Parent;
        internal int CustomArg;
        internal Regex ArgumentsPattern;
        internal ulong AuthMask;
        internal string Plugin;
        internal int MinLength;
        internal SortedDictionary<string, InputEntry> Subcommands;
    }

    /// <summary>
    /// Function for handling input. Return true if we handled the command (no need to send to MUD) and false
    /// if we didn't and we must send it to MUD.
    /// </summary>
    /// <param name="cmd">Input data.</param>
    /// <returns></returns>
    public delegate bool CmdFunction(InputData cmd);

    [Flags]
    public enum CMDFlags
    {
        None = 0,

        /// <summary>
        /// Hidden from normal commands menu.
        /// </summary>
        Hidden = 2,

        /// <summary>
        /// Command is currently disabled and will be excluded in the list of valid commands.
        /// </summary>
        Disabled = 4,

        /// <summary>
        /// Dummy command, player can't type it but it will be redirected from elsewhere.
        /// </summary>
        Dummy = 8,

        /// <summary>
        /// Internal - this will be assigned automatically for commands that have subcommands.
        /// </summary>
        IsParent = 0x10,
    }
}
