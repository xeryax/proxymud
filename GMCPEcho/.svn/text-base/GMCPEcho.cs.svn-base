using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore.Scripting;
using ProxyCore;
using ProxyCore.Output;

namespace GMCPEcho
{
    public class GMCPEcho : Plugin
    {
        public GMCPEcho()
            : base("gmcpecho", "GMCP Echo")
        {
            Author = "Duckbat";
            Version = 1;
            Description = "Echos all GMCP specified by user back. This way you can see GMCP in clients that normally don't support it.";
            UpdateUrl = "www.duckbat.com/plugins/update.gmcpecho.txt";
            Website = "www.duckbat.com/plugins/index.php?t=gmcpecho";

            Config = new GMCPEchoConfig();

            RegisterTrigger("gmcp", @"^\$gmcp\.", GMCPTrigger);
        }

        private string[] AllowedModules;

        private bool GMCPTrigger(TriggerData t)
        {
            if(AllowedModules == null)
                AllowedModules = Config.GetString("GMCP.Modules", "gmcp.*").Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            string Module = t.Msg.Msg.Substring(1);
            if(Module.Contains(' '))
                Module = Module.Substring(0, Module.IndexOf(' '));
            if(HasModule(Module))
                World.Instance.SendMessage(t.Msg.Msg, Config.GetUInt64("GMCP.AuthMask", ulong.MaxValue));
            return false;
        }

        private bool HasModule(string Msg)
        {
            foreach(string x in AllowedModules)
            {
                if(x.EndsWith("*"))
                {
                    if(Msg.StartsWith(x.Substring(0, x.Length - 1)))
                        return true;
                    continue;
                }

                if(x == Msg)
                    return true;
            }

            return false;
        }
    }

    public class GMCPEchoConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("GMCP.Modules", "gmcp.*", "Which GMCP modules would you like to echo to client. Separate with ',' sign and use '*' for wildcard but the wildcard can only be at the end not in the middle or start. For example \"gmcp.char.*, gmcp.room.*\". Use \"gmcp.*\" to echo everything.");
            CreateSetting("GMCP.AuthMask", ulong.MaxValue, "Which client security levels see GMCP echo. This is a 64 bit mask. For example if you want security level 1 and 3 to see GMCP you would enter value 5 (1 + 4). These values are 2 ^ (level - 1), for example security level 3 mask: 2 ^ (3 - 1) = 4. Then you just add these up. Default (all levels): " + ulong.MaxValue);
        }
    }
}
