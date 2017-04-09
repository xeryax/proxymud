using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore;
using ProxyCore.Output;
using ProxyCore.Scripting;

namespace StayAlive
{
    public class StayAlive : Plugin
    {
        public StayAlive()
            : base("stayalive", "Stay logged in")
        {
            Author = "Duckbat";
            Version = 4;
            Description = "Automatically stays logged in.";
            UpdateUrl = "www.duckbat.com/plugins/update.stayalive.txt";
            Website = "code.google.com/p/proxymud/";

            Config = new StayAliveConfig();

            RegisterTrigger("autologin.name", "What be thy name, adventurer?", TriggerUsername);
            RegisterTrigger("autologin.pass", "Existing profile loaded - please enter your password.", TriggerPassword);
        }

        public override void OnConnect()
        {
            base.OnConnect();

            CanEnterUsername = 1;
        }

        private bool TriggerUsername(TriggerData t)
        {
            if(CanEnterUsername != 1)
                return false;

            string n = Config.GetString("AutoLogin.User", "").Trim();
            if(string.IsNullOrEmpty(n))
            {
                CanEnterUsername = 0;
                return false;
            }

            string p = Config.GetString("AutoLogin.Pass", "").Trim();
            if(string.IsNullOrEmpty(p))
            {
                CanEnterUsername = 0;
                return false;
            }

            CanEnterUsername = 2;
            World.Instance.Execute(n, false);
            return false;
        }

        private bool TriggerPassword(TriggerData t)
        {
            if(CanEnterUsername != 2)
                return false;

            string p = Config.GetString("AutoLogin.Pass", "").Trim();
            if(string.IsNullOrEmpty(p))
            {
                CanEnterUsername = 0;
                return false;
            }

            CanEnterUsername = 0;
            World.Instance.Execute(p, false);

            World.Instance.Execute("", false);
            return false;
        }

        private int CanEnterUsername = 0;

        public override void OnLoadedConfig(bool Success)
        {
            base.OnLoadedConfig(Success);

            string n = Config.GetString("StayAlive.Line", "@wYour eyes glaze over.");
            if(string.IsNullOrEmpty(n))
                return;

            RegisterTrigger("line", n, TriggerLine, TriggerFlags.NotRegex);
        }

        private bool TriggerLine(TriggerData t)
        {
            World.Instance.Execute(Config.GetString("StayAlive.Command", ""), false);
            return false;
        }
    }

    public class StayAliveConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("StayAlive.Command", "", "What to send to MUD when we receive line about being idle. You can leave this blank to just send enter key.");
            CreateSetting("StayAlive.Line", "@wYour eyes glaze over.", "On what line do we send command? Default: \"@wYour eyes glaze over.\"");
            CreateSetting("AutoLogin.User", "", "User name to automatically log in with. Leave blank to disable this feature.");
            CreateSetting("AutoLogin.Pass", "", "Password to automatically log in with. Leave blank to disable this feature.");
        }
    }
}
