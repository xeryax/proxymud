using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore.Output;
using System.Text.RegularExpressions;
using ProxyCore.Input;

namespace ProxyCore.Scripting
{
    public class Plugin
    {
        protected Plugin(string keyword, string name)
        {
            Keyword = keyword;
            Name = name;
        }

        /// <summary>
        /// Set this to be your configuration file if you want your plugin to have one. This is optional.
        /// </summary>
        public ConfigFile Config
        {
            get;
            protected set;
        }

        /// <summary>
        /// Name of your plugin. You must set this or your plugin will not be loaded.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// This is the keyword for your plugin. You must set this or your plugin will not be loaded.
        /// This must be unique. If two or more plugins with the same keyword are found then the plugin
        /// with the highest version number is loaded.
        /// </summary>
        public readonly string Keyword;

        /// <summary>
        /// Creator of the plugin, this is your name / character's name. This is optional.
        /// </summary>
        public string Author
        {
            get;
            protected set;
        }

        /// <summary>
        /// Version of your plugin. This is optional.
        /// </summary>
        public int Version
        {
            get;
            protected set;
        }

        /// <summary>
        /// Description about your plugin. You should explain here what it does and how to handle it.
        /// This will be displayed if user requests information about your plugin. This is optional.
        /// </summary>
        public string Description
        {
            get;
            protected set;
        }

        /// <summary>
        /// Enter a website for this plugin if you wish. Mostly used to see documentation and updates.
        /// </summary>
        public string Website
        {
            get;
            protected set;
        }

        /// <summary>
        /// Enter the URL for update checking txt file. For example "www.duckbat.com/plugins/update.moons.txt".
        /// In the text file enter the number of last version. For example whole contents of the txt file can be "3".
        /// Indicating that the last version for this plugin is 3. If version is greater than user's version and
        /// they have update checking on then they will be notified that there is a more up to date version out there.
        /// </summary>
        public string UpdateUrl
        {
            get;
            protected set;
        }

        /// <summary>
        /// Does this plugin require a certain version of core? Set this if there was an update and your plugin requires it.
        /// Plugin is not loaded if an older version core than this is used and user will be notified.
        /// </summary>
        public int RequiredCoreVersion
        {
            get;
            protected set;
        }

        /// <summary>
        /// Register a new trigger.
        /// </summary>
        /// <param name="Name">Unique identifier for the trigger.</param>
        /// <param name="Pattern">Regex pattern for the trigger.</param>
        /// <param name="Function">Function that will be called if this trigger fires.</param>
        protected void RegisterTrigger(string Name, string Pattern, TriggerFunction Function)
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
        protected void RegisterTrigger(string Name, string Pattern, TriggerFunction Function, TriggerFlags Flags)
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
        /// <param name="Priority">Lower priority triggers get matched first.</param>
        protected void RegisterTrigger(string Name, string Pattern, TriggerFunction Function, TriggerFlags Flags, int Priority)
        {
            RegisterTrigger(Name, Pattern, Function, Flags, Priority, 0);
        }

        /// <summary>
        /// Register a new trigger.
        /// </summary>
        /// <param name="Name">Unique identifier for the trigger.</param>
        /// <param name="Pattern">Regex pattern for the trigger.</param>
        /// <param name="Function">Function that will be called if this trigger fires.</param>
        /// <param name="Flags">Options for the trigger.</param>
        /// <param name="Priority">Lower priority triggers get matched first.</param>
        /// <param name="Arg">Custom argument that will be passed to trigger data.</param>
        protected void RegisterTrigger(string Name, string Pattern, TriggerFunction Function, TriggerFlags Flags, int Priority, int Arg)
        {
            TriggerHandler.RegisterTrigger(Keyword.ToLower().Trim() + "." + Name, Pattern, Function, Flags, Priority, Arg, Keyword.ToLower().Trim());
        }

        /// <summary>
        /// Unregister a trigger by name.
        /// </summary>
        /// <param name="Name">Name of the trigger you wish to unregister.</param>
        protected void UnregisterTrigger(string Name)
        {
            TriggerHandler.UnregisterTrigger(Keyword.ToLower().Trim() + "." + Name);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        protected void RegisterCommand(string Cmd, string Args, CmdFunction f)
        {
            RegisterCommand(Cmd, Args, f, 0);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="MinLength">Minimum length of command typed required to activate. For example if command is "plugins" and this is 6 then "plugin" and "plugins" both activate this command but "plugi" won't. Enter 0 to disable this behaviour.</param>
        protected void RegisterCommand(string Cmd, string Args, CmdFunction f, int MinLength)
        {
            RegisterCommand(Cmd, Args, f, MinLength, CMDFlags.None);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="MinLength">Minimum length of command typed required to activate. For example if command is "plugins" and this is 6 then "plugin" and "plugins" both activate this command but "plugi" won't. Enter 0 to disable this behaviour.</param>
        /// <param name="flags">Options for command.</param>
        protected void RegisterCommand(string Cmd, string Args, CmdFunction f, int MinLength, CMDFlags flags)
        {
            RegisterCommand(Cmd, Args, f, MinLength, flags, null);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="MinLength">Minimum length of command typed required to activate. For example if command is "plugins" and this is 6 then "plugin" and "plugins" both activate this command but "plugi" won't. Enter 0 to disable this behaviour.</param>
        /// <param name="flags">Options for command.</param>
        /// <param name="parent">Parent command (if you want to create a subcommand). You can enter commands separated with space if it's nested.</param>
        protected void RegisterCommand(string Cmd, string Args, CmdFunction f, int MinLength, CMDFlags flags, string parent)
        {
            RegisterCommand(Cmd, Args, f, MinLength, flags, parent, 0);
        }

        /// <summary>
        /// Register a new command or overwrite a previous one.
        /// </summary>
        /// <param name="Cmd">Command to register.</param>
        /// <param name="Args">Arguments to match (regex pattern). This can be set to null or empty string
        /// if you don't want to capture anything or plan to do so in the function yourself.</param>
        /// <param name="f">Function of command.</param>
        /// <param name="MinLength">Minimum length of command typed required to activate. For example if command is "plugins" and this is 6 then "plugin" and "plugins" both activate this command but "plugi" won't. Enter 0 to disable this behaviour.</param>
        /// <param name="flags">Options for command.</param>
        /// <param name="parent">Parent command (if you want to create a subcommand). You can enter commands separated with space if it's nested.</param>
        /// <param name="Arg">Custom argument to pass to function handler. This way you can register multiple commands to a same
        /// function handler only differentiating them with this custom argument.</param>
        protected void RegisterCommand(string Cmd, string Args, CmdFunction f, int MinLength, CMDFlags flags, string parent, int Arg)
        {
            RegisterCommand(Cmd, Args, f, MinLength, flags, parent, Arg, ulong.MaxValue);
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
        /// <param name="MinLength">Minimum length of command typed required to activate. For example if command is "plugins" and this is 6 then "plugin" and "plugins" both activate this command but "plugi" won't. Enter 0 to disable this behaviour.</param>
        protected void RegisterCommand(string Cmd, string Args, CmdFunction f, int MinLength, CMDFlags flags, string parent, int Arg, ulong AuthMask)
        {
            InputHandler.RegisterCommand(Cmd, Args, f, flags, parent, Arg, AuthMask, Keyword.ToLower().Trim(), MinLength);
        }

        /// <summary>
        /// Unregister a command.
        /// </summary>
        /// <param name="Cmd">Command to unregister. If you want to unregister a nested command
        /// separate commands with a space.</param>
        protected void UnregisterCommand(string Cmd)
        {
            InputHandler.UnregisterCommand(Cmd);
        }

        /// <summary>
        /// This will be called when character enters the game. Either by log in or reconnect.
        /// </summary>
        public virtual void OnLogin()
        {
        }

        /// <summary>
        /// This will be called when we disconnect from Aardwolf.
        /// </summary>
        public virtual void OnDisconnect()
        {
        }

        /// <summary>
        /// This will be called when we connect to Aardwolf.
        /// </summary>
        public virtual void OnConnect()
        {
        }

        /// <summary>
        /// This is called when program shuts down. Write any code you need to shut down your plugin.
        /// </summary>
        public virtual void Shutdown()
        {
        }

        /// <summary>
        /// This is called on every loop of world update. You can use it as your main loop for
        /// the plugin if you need one.
        /// </summary>
        /// <param name="msTime">Current time since program startup.</param>
        public virtual void Update(long msTime)
        {
        }

        /// <summary>
        /// This is called when we receive a line from MUD. It is called AFTER triggers are done with it. If
        /// a trigger gagged the line this will not be called.
        /// </summary>
        /// <param name="Msg"></param>
        public virtual void OnReceivedLineAfter(Messages.Message Msg)
        {
        }

        /// <summary>
        /// This is called when we receive a line from MUD. It is called BEFORE triggers are done with it.
        /// </summary>
        /// <param name="Msg"></param>
        public virtual void OnReceivedLineBefore(Messages.Message Msg)
        {
        }

        /// <summary>
        /// This is called when user enters a command and inputhandler did not handle the command. So it
        /// is called AFTER we check for aliases and commands and we are about to send command to MUD.
        /// </summary>
        /// <param name="Msg">Command that was entered. You can change this in the function. If you set null
        /// then nothing will be sent to MUD.</param>
        /// <param name="ClientId">Client who entered the command. If this is 0 it was executed from a plugin.</param>
        /// <param name="AuthLevel">Auth level of who entered the command.</param>
        public virtual void OnEnteredCommandAfter(ref string Msg, uint ClientId, int AuthLevel)
        {
        }

        /// <summary>
        /// This is called when user enters a command. It is called BEFORE we check for aliases and commands.
        /// </summary>
        /// <param name="Msg">Command that was entered. You can change this in the function. If you set null
        /// then nothing will be sent to MUD and nothing will be checked for aliases or commands.</param>
        /// <param name="ClientId">Client who entered the command. If this is 0 it was executed from a plugin.</param>
        /// <param name="AuthLevel">Auth level of who entered the command.</param>
        public virtual void OnEnteredCommandBefore(ref string Msg, uint ClientId, int AuthLevel)
        {
        }

        /// <summary>
        /// Enter required player config options here. This will be displayed if user requests info about a plugin.
        /// For example you may enter here "echocommands ON" and "statmon ON" etc. Whatever your plugin requires.
        /// This doesn't actually change the settings in game it is only for plugin info command.
        /// </summary>
        public readonly List<string> RequiredPlayerConfig = new List<string>();

        /// <summary>
        /// This is the class name of script. For example moons has this set to "MoonScript.MoonScript".
        /// Only needed by developers who want to use another plugin in their plugin.
        /// </summary>
        internal string ClassName;

        /// <summary>
        /// Called when we load a configuration file.
        /// </summary>
        /// <param name="Success">Did the loading succeed? If not then the config file wasn't present and we created a new one.</param>
        public virtual void OnLoadedConfig(bool Success)
        {
        }

        /// <summary>
        /// Disable all triggers with this priority. Disabling triggers from a plugin will make them not work until
        /// you enable them from the same plugin again. If triggers have been disabled from multiple plugins then
        /// all plugins will have to enable them again until they start working. Disabling triggers will make all
        /// triggers with this priority to not work not only triggers in current plugin!
        /// </summary>
        /// <param name="Priority">Priority of triggers to disable.</param>
        protected void DisableTriggers(int Priority)
        {
            DisableTriggers(Priority, Priority);
        }

        /// <summary>
        /// Disable all triggers with this priority. Disabling triggers from a plugin will make them not work until
        /// you enable them from the same plugin again. If triggers have been disabled from multiple plugins then
        /// all plugins will have to enable them again until they start working. Disabling triggers will make all
        /// triggers with this priority to not work not only triggers in current plugin!
        /// </summary>
        /// <param name="MinPriority">Minimum priority of triggers to disable.</param>
        /// <param name="MaxPriority">Maximum priority of triggers to disable.</param>
        protected void DisableTriggers(int MinPriority, int MaxPriority)
        {
            TriggerHandler.DisableTriggers(Keyword, MinPriority, MaxPriority);
        }

        /// <summary>
        /// Enable all previously disabled triggers with this priority.
        /// </summary>
        /// <param name="Priority">Priority of triggers to enable.</param>
        protected void EnableTriggers(int Priority)
        {
            EnableTriggers(Priority, Priority);
        }

        /// <summary>
        /// Enable all previously disabled triggers with this priority.
        /// </summary>
        /// <param name="MinPriority">Minimum priority of triggers to enable.</param>
        /// <param name="MaxPriority">Maximum priority of triggers to enable.</param>
        protected void EnableTriggers(int MinPriority, int MaxPriority)
        {
            TriggerHandler.EnableTriggers(Keyword, MinPriority, MaxPriority);
        }
    }
}
