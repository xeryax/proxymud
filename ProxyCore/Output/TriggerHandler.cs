using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProxyCore.Messages;
using ProxyCore.Scripting;

namespace ProxyCore.Output
{
    internal static class TriggerHandler
    {
        static TriggerHandler()
        {
        }

        private static SortedDictionary<int, List<TriggerEntry>> Triggers = new SortedDictionary<int, List<TriggerEntry>>();
        internal static Dictionary<string, TriggerEntry> TriggersName = new Dictionary<string, TriggerEntry>();

        internal static void DisableTriggers(string FromPlugin, int MinPriority, int MaxPriority)
        {
            if(FromPlugin == null)
                return;
            FromPlugin = FromPlugin.ToLower().Trim();
            if(FromPlugin.Length == 0)
                return;
            foreach(KeyValuePair<int, List<TriggerEntry>> x in Triggers)
            {
                if(x.Key < MinPriority)
                    continue;
                if(x.Key > MaxPriority)
                    break;

                foreach(TriggerEntry y in x.Value)
                {
                    if(y.Disabled == null)
                        y.Disabled = new List<string>();
                    if(!y.Disabled.Contains(FromPlugin))
                        y.Disabled.Add(FromPlugin);
                }
            }
        }

        internal static void EnableTriggers(string FromPlugin, int MinPriority, int MaxPriority)
        {
            if(FromPlugin == null)
                return;
            FromPlugin = FromPlugin.ToLower().Trim();
            if(FromPlugin.Length == 0)
                return;
            foreach(KeyValuePair<int, List<TriggerEntry>> x in Triggers)
            {
                if(x.Key < MinPriority)
                    continue;
                if(x.Key > MaxPriority)
                    break;

                foreach(TriggerEntry y in x.Value)
                {
                    if(y.Disabled == null)
                        continue;
                    y.Disabled.Remove(FromPlugin);
                }
            }
        }

        /// <summary>
        /// Register a new trigger.
        /// </summary>
        /// <param name="Name">Unique identifier for the trigger.</param>
        /// <param name="Pattern">Regex pattern for the trigger.</param>
        /// <param name="Function">Function that will be called if this trigger fires.</param>
        internal static void RegisterTrigger(string Name, string Pattern, TriggerFunction Function)
        {
            RegisterTrigger(Name, Pattern, Function, TriggerFlags.None);
        }

        /// <summary>
        /// Register a new trigger.
        /// </summary>
        /// <param name="Name">Unique identifier for the trigger.</param>
        /// <param name="Pattern">Regex pattern for the trigger.</param>
        /// <param name="Function">Function that will be called if this trigger fires.</param>
        /// <param name="Flags">Options for the trigger.</param>
        internal static void RegisterTrigger(string Name, string Pattern, TriggerFunction Function, TriggerFlags Flags)
        {
            RegisterTrigger(Name, Pattern, Function, Flags, 1000);
        }

        /// <summary>
        /// Register a new trigger.
        /// </summary>
        /// <param name="Name">Unique identifier for the trigger.</param>
        /// <param name="Pattern">Regex pattern for the trigger.</param>
        /// <param name="Function">Function that will be called if this trigger fires.</param>
        /// <param name="Flags">Options for the trigger.</param>
        /// <param name="Priority">Lower priority triggers get matched first. Default: 1000</param>
        internal static void RegisterTrigger(string Name, string Pattern, TriggerFunction Function, TriggerFlags Flags, int Priority)
        {
            RegisterTrigger(Name, Pattern, Function, Flags, Priority, 0, "core");
        }

        /// <summary>
        /// Register a new trigger.
        /// </summary>
        /// <param name="Name">Unique identifier for the trigger.</param>
        /// <param name="Pattern">Regex pattern for the trigger.</param>
        /// <param name="Function">Function that will be called if this trigger fires.</param>
        /// <param name="Flags">Options for the trigger.</param>
        /// <param name="Priority">Lower priority triggers get matched first. Default: 1000</param>
        /// <param name="Arg">Custom argument to pass to trigger data.</param>
        /// <param name="Plugin">From which plugin was this registered.</param>
        internal static void RegisterTrigger(string Name, string Pattern, TriggerFunction Function, TriggerFlags Flags, int Priority, int Arg, string Plugin)
        {
            if(string.IsNullOrEmpty(Pattern) || string.IsNullOrEmpty(Name) || Function == null)
                return;

            Name = Name.ToLower().Trim();
            if(Name.Length == 0)
                return;

            Regex p = null;
            if((Flags & TriggerFlags.NotRegex) == TriggerFlags.None)
            {
                try
                {
                    RegexOptions op = RegexOptions.None;
                    if((Flags & TriggerFlags.RightToLeft) != TriggerFlags.None)
                        op |= RegexOptions.RightToLeft;
                    if((Flags & TriggerFlags.CaseInsensitive) != TriggerFlags.None)
                        op |= RegexOptions.IgnoreCase;
                    p = new Regex(Pattern, op);
                }
                catch
                {
                    return;
                }
            }

            TriggerEntry e = new TriggerEntry();
            e.Function = Function;
            e.Pattern = p;
            e.PatternStr = Pattern;
            e.Priority = Priority;
            e.Name = Name;
            e.Flags = Flags;
            e.Arg = Arg;
            e.Plugin = Plugin;

            if(TriggersName.ContainsKey(Name))
                Triggers[TriggersName[Name].Priority].Remove(TriggersName[Name]);

            TriggersName[Name] = e;
            if(!Triggers.ContainsKey(e.Priority))
                Triggers[e.Priority] = new List<TriggerEntry>();
            Triggers[e.Priority].Add(e);
        }

        /// <summary>
        /// Unregister a trigger by name.
        /// </summary>
        /// <param name="Name">Name of the trigger you wish to unregister.</param>
        internal static void UnregisterTrigger(string Name)
        {
            if(Name != null)
                Name = Name.ToLower().Trim();
            if(string.IsNullOrEmpty(Name))
                return;

            if(!TriggersName.ContainsKey(Name))
                return;

            Triggers[TriggersName[Name].Priority].Remove(TriggersName[Name]);
            TriggersName.Remove(Name);
        }

        internal static void HandleText(string Msg, World world)
        {
            if(!string.IsNullOrEmpty(lastColorCode))
                Msg = lastColorCode + Msg;
            lastColorCode = Colors.GetLastColorCode(Msg);
            Msg = Colors.RemoveDuplicateColors(Msg);

            Message m = new Message(true);
            m.Msg = Msg;
            HandleLineRaw(m);
            if(m.Msg != null)
            {
                world._SendMessage(m);
                world.lastLine.Add(m.Msg);
                while(world.lastLine.Count > 100)
                    world.lastLine.RemoveAt(0);
            }
        }

        private static string lastColorCode = "";

        private static readonly List<KeyValuePair<string, string>> gmcpData =
            new List<KeyValuePair<string, string>>();

        internal static void HandleGMCP(string Msg)
        {
            //string origMsg = Msg;
            string module;
            try
            {
                int ind = Msg.IndexOf(' ');
                if(ind == -1)
                {
                    module = Msg;
                    Msg = "";
                }
                else
                {
                    module = Msg.Substring(0, ind);
                    Msg = Msg.Substring(ind).Trim();
                }
            }
            catch
            {
                return;
            }

            if(string.IsNullOrEmpty(module))
                return;

            if(gmcpData.Count != 0)
                gmcpData.Clear();
            module = module.ToLower();
            bool res = JSON.Parse(Msg, module, gmcpData);
            if(!res)
                return;

            if(gmcpData.Count == 0)
                HandleGMCP(module, null);
            else
            {
                foreach(KeyValuePair<string, string> i in gmcpData)
                    HandleGMCP(i.Key.ToLower().Trim(), i.Value);
            }
        }

        private static void HandleGMCP(string Module, string Value)
        {
            Message m = new Message(true);
            m.Msg = "$gmcp." + Module + (Value != null ? (" " + Value) : "");
            HandleLineRaw(m);
        }

        private static void HandleLineRaw(Message Msg)
        {
            foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
            {
                try
                {
                    x.Value.OnReceivedLineBefore(Msg);
                }
                catch(Exception e)
                {
                    Log.Crash(e, x.Key);
                }
                if(Msg.Msg == null)
                    return;
            }

            foreach(KeyValuePair<int, List<TriggerEntry>> x in Triggers)
            {
                foreach(TriggerEntry y in x.Value)
                {
                    if(y.Disabled != null && y.Disabled.Count != 0)
                        continue;

                    int i = 0;
                    while(Msg.Msg != null)
                    {
                        int o = i;
                        Match m = null;
                        if((y.Flags & TriggerFlags.NotRegex) == TriggerFlags.None)
                        {
                            if((y.Flags & TriggerFlags.RightToLeft) != TriggerFlags.None)
                                m = y.Pattern.Match((y.Flags & TriggerFlags.NonAnsi) == TriggerFlags.None ? Msg.Msg : Msg.MsgNoColor);
                            else
                                m = y.Pattern.Match((y.Flags & TriggerFlags.NonAnsi) == TriggerFlags.None ? Msg.Msg : Msg.MsgNoColor, i);
                            if(!m.Success)
                                break;
                        }
                        else if(y.PatternStr != ((y.Flags & TriggerFlags.NonAnsi) == TriggerFlags.None ? Msg.Msg : Msg.MsgNoColor))
                            break;

                        TriggerData d = new TriggerData();
                        d.Match = m;
                        d.Arg = y.Arg;
                        d.Msg = Msg;
#if !DEBUG
                        try
                        {
#endif
                            if(y.Function(d))
                            {
                                Msg.Msg = null;
                                return;
                            }
#if !DEBUG
                        }
                        catch(Exception e)
                        {
                            Log.Crash(e, y.Plugin);
                        }
#endif

                        if((y.Flags & TriggerFlags.NotRegex) != TriggerFlags.None)
                            break;
                        if((y.Flags & TriggerFlags.RightToLeft) != TriggerFlags.None)
                            break;

                        i = m.Groups[0].Index + m.Groups[0].Length;
                        if((y.Flags & TriggerFlags.Repeat) == TriggerFlags.None)
                            break;

                        if(i == o)
                            i++;
                    }
                }
            }

            foreach(KeyValuePair<string, Plugin> x in PluginMgr.Plugins)
            {
#if !DEBUG
                try
                {
#endif
                    x.Value.OnReceivedLineAfter(Msg);
#if !DEBUG
                }
                catch(Exception e)
                {
                    Log.Crash(e, x.Key);
                }
#endif
                if(Msg.Msg == null)
                    return;
            }
        }
    }
}
