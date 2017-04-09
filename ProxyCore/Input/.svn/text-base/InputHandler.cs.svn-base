using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyCore.Input
{
    internal static class InputHandler
    {
        static InputHandler()
        {
        }

        internal static SortedDictionary<string, InputEntry> Commands = new SortedDictionary<string, InputEntry>();

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        internal static void RegisterCommand(string Cmd, string Args, CmdFunction f)
        {
            RegisterCommand(Cmd, Args, f, CMDFlags.None);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="flags">Options for command.</param>
        internal static void RegisterCommand(string Cmd, string Args, CmdFunction f, CMDFlags flags)
        {
            RegisterCommand(Cmd, Args, f, flags, null);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="flags">Options for command.</param>
        /// <param name="parent">Parent command (if you want to create a subcommand). You can enter commands separated with space if it's nested.</param>
        internal static void RegisterCommand(string Cmd, string Args, CmdFunction f, CMDFlags flags, string parent)
        {
            RegisterCommand(Cmd, Args, f, flags, parent, 0);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="flags">Options for command.</param>
        /// <param name="parent">Parent command (if you want to create a subcommand). You can enter commands separated with space if it's nested.</param>
        /// <param name="Arg">Custom argument to pass to function handler. This way you can register multiple commands to a same
        /// function handler only differentiating them with this custom argument.</param>
        internal static void RegisterCommand(string Cmd, string Args, CmdFunction f, CMDFlags flags, string parent, int Arg)
        {
            RegisterCommand(Cmd, Args, f, flags, parent, Arg, ulong.MaxValue, "core", 0);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="flags">Options for command.</param>
        /// <param name="parent">Parent command (if you want to create a subcommand). You can enter commands separated with space if it's nested.</param>
        /// <param name="Arg">Custom argument to pass to function handler. This way you can register multiple commands to a same
        /// function handler only differentiating them with this custom argument.</param>
        /// <param name="AuthMask">Mask of allowed auth levels to access this command. Default ulong.MaxValue (meaning all auth levels are allowed).
        /// Enter 3 for example to allow only auth level 1 and 2 to access this command.</param>
        /// <param name="Plugin">From which plugin did this come.</param>
        /// <param name="ReqMinLength">Minimum length of command typed required to activate. For example if command is "plugins" and this is 6 then "plugin" and "plugins" both activate this command but "plugi" won't.</param>
        internal static void RegisterCommand(string Cmd, string Args, CmdFunction f, CMDFlags flags, string parent, int Arg, ulong AuthMask, string Plugin, int ReqMinLength)
        {
            if(string.IsNullOrEmpty(Cmd))
                return;

            Cmd = Cmd.ToLower().Trim();
            if(string.IsNullOrEmpty(Cmd))
                return;

            if(Cmd.Contains(' '))
                Cmd = Cmd.Substring(0, Cmd.IndexOf(' '));

            InputEntry p = null;
            if(!string.IsNullOrEmpty(parent))
            {
                string[] pc = parent.ToLower().Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if(pc.Length == 0)
                    return;

                if(Commands.ContainsKey(pc[0]))
                {
                    p = Commands[pc[0]];
                    for(int i = 1; i < pc.Length; i++)
                    {
                        if(p.Subcommands != null && p.Subcommands.ContainsKey(pc[i]))
                            p = p.Subcommands[pc[i]];
                        else
                            return;
                    }
                }
                else
                    return;
            }

            InputEntry c = new InputEntry()
            {
                Command = Cmd,
                CustomArg = Arg,
                Flags = flags,
                Func = f,
                Parent = p,
                Plugin = Plugin,
                MinLength = ReqMinLength,
                AuthMask = AuthMask
            };

            try
            {
                c.ArgumentsPattern = new Regex(Args, RegexOptions.IgnoreCase);
            }
            catch
            {
                c.ArgumentsPattern = null;
            }

            if(p != null)
            {
                if(p.Subcommands == null)
                    p.Subcommands = new SortedDictionary<string, InputEntry>();
                p.Subcommands[Cmd] = c;
                p.Flags |= CMDFlags.IsParent;
            }
            else
                Commands[Cmd] = c;
        }

        /// <summary>
        /// Unregister a command.
        /// </summary>
        /// <param name="Cmd">Command to unregister. If you want to unregister a nested command
        /// separate commands with a space.</param>
        internal static void UnregisterCommand(string Cmd)
        {
            if(Cmd == null)
                return;

            string[] pc = Cmd.ToLower().Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if(pc.Length == 0)
                return;

            if(!Commands.ContainsKey(pc[0]))
                return;

            if(pc.Length > 1)
            {
                InputEntry p = Commands[pc[0]];
                for(int i = 1; i < pc.Length; i++)
                {
                    if(p.Subcommands == null || !p.Subcommands.ContainsKey(pc[i]))
                        return;
                    p = p.Subcommands[pc[i]];
                }

                p.Parent.Subcommands.Remove(p.Command);
                if(p.Parent.Subcommands.Count == 0)
                    p.Parent.Flags &= ~CMDFlags.IsParent;
            }
            else
                Commands.Remove(pc[0]);
        }

        internal static InputData HandleInput(string origCommand, string Msg, uint ClientId, int AuthLevel, InputEntry parent, World world)
        {
            Msg = Msg.Trim();
            string cmd = "";
            string text = "";

            if(Msg.Contains(' '))
            {
                cmd = Msg.Substring(0, Msg.IndexOf(' '));
                text = Msg.Substring(Msg.IndexOf(' ') + 1).Trim();
            }
            else
            {
                cmd = Msg;
            }

            cmd = cmd.ToLower();
            InputEntry f = null;

            if(!string.IsNullOrEmpty(cmd))
            {
                if(parent == null)
                {
                    foreach(KeyValuePair<string, InputEntry> x in Commands)
                    {
                        if(x.Key == cmd && (x.Value.Flags & (CMDFlags.Dummy | CMDFlags.Disabled)) == CMDFlags.None && (x.Value.AuthMask & ((ulong)1 << (AuthLevel - 1))) != 0)
                        {
                            f = x.Value;
                            break;
                        }
                    }

                    if(f == null)
                    {
                        foreach(KeyValuePair<string, InputEntry> x in Commands)
                        {
                            if(x.Value.MinLength <= 0)
                                continue;
                            if(cmd.Length < x.Value.Command.Length && cmd.Length >= x.Value.MinLength && x.Value.Command.StartsWith(cmd) && (x.Value.Flags & (CMDFlags.Dummy | CMDFlags.Disabled)) == CMDFlags.None && (x.Value.AuthMask & ((ulong)1 << (AuthLevel - 1))) != 0)
                            {
                                f = x.Value;
                                break;
                            }
                        }
                    }
                }
                else if(parent.Subcommands != null)
                {
                    foreach(KeyValuePair<string, InputEntry> x in parent.Subcommands)
                    {
                        if(x.Key == cmd && (x.Value.Flags & (CMDFlags.Dummy | CMDFlags.Disabled)) == CMDFlags.None && (x.Value.AuthMask & ((ulong)1 << (AuthLevel - 1))) != 0)
                        {
                            f = x.Value;
                            break;
                        }
                    }

                    if(f == null)
                    {
                        foreach(KeyValuePair<string, InputEntry> x in parent.Subcommands)
                        {
                            if(x.Value.MinLength <= 0)
                                continue;
                            if(cmd.Length < x.Value.Command.Length && cmd.Length >= x.Value.MinLength && x.Value.Command.StartsWith(cmd) && (x.Value.Flags & (CMDFlags.Dummy | CMDFlags.Disabled)) == CMDFlags.None && (x.Value.AuthMask & ((ulong)1 << (AuthLevel - 1))) != 0)
                            {
                                f = x.Value;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if(parent != null && parent.Func != null)
                    f = parent;
                else
                    return null;
            }

            if(f != null && (f.Flags & CMDFlags.IsParent) != CMDFlags.None && !string.IsNullOrEmpty(text))
                return HandleInput(origCommand, text, ClientId, AuthLevel, f, world);

            if(f == null || f.Func == null)
                return null;

            InputData data = new InputData();
            data.Command = origCommand;
            data.ClientId = ClientId;
            data.ClientAuthLevel = AuthLevel;
            data.Function = f;
            if(f.ArgumentsPattern != null)
                data.Arguments = f.ArgumentsPattern.Match(text);

            return data;
        }
    }
}
