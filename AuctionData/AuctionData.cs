using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore.Scripting;
using ProxyCore;
using ProxyCore.Output;

namespace AuctionData
{
    public class AuctionData : Plugin
    {
        public AuctionData()
            : base("AuctionData", "GMCP Echo")
        {
            Author = "KoopaTroopa";
            Version = 1;
            Description = "Echos all GMCP specified by user back. This way you can see GMCP in clients that normally don't support it.";
            UpdateUrl = "www.duckbat.com/plugins/update.AuctionData.txt";
            Website = "www.duckbat.com/plugins/index.php?t=AuctionData";

            Config = new AuctionDataConfig();

            RegisterTrigger("marketsalecomp", @"^\$gmcp\.comm\.channel\.msg @\wMarket: (?<item>.+) \(Level .+, Num .+\) [Ss][Oo][Ll][Dd] to (?<buyer>\w+) for (?<price>.+) (?<currency>[Gg]old|TP|QP)\.", CaptureTrigger);
            RegisterTrigger("aucsalecomp", @"^\$gmcp\.comm\.channel\.msg @\wAuction: (?<item>.+) [Ss][Oo][Ll][Dd] to (?<buyer>\w+) for (?<price>.+) (?<currency>gold)\.", CaptureTrigger);
            
        }

        private string[] AllowedModules;

        private bool CaptureTrigger(TriggerData t)
        {
            World.Instance.SendMessage(t.Msg.Msg, Config.GetUInt64("Auc.AuthMask", ulong.MaxValue));
            //World.Instance.SendMessage("@w{CommandEcho,Level=" + AuthLevel + ",Id=" + ClientId + "}" + Msg, Config.GetUInt64("Echo.To.AuthMask", ulong.MaxValue - 1));
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

    public class AuctionDataConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("GMCP.Modules", "gmcp.*", "Which GMCP modules would you like to echo to client. Separate with ',' sign and use '*' for wildcard but the wildcard can only be at the end not in the middle or start. For example \"gmcp.char.*, gmcp.room.*\". Use \"gmcp.*\" to echo everything.");
            CreateSetting("Auc.AuthMask", ulong.MaxValue, "Which client security levels see GMCP echo. This is a 64 bit mask. For example if you want security level 1 and 3 to see GMCP you would enter value 5 (1 + 4). These values are 2 ^ (level - 1), for example security level 3 mask: 2 ^ (3 - 1) = 4. Then you just add these up. Default (all levels): " + ulong.MaxValue);
        }
    }
}
