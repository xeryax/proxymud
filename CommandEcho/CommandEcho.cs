using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore;
using ProxyCore.Scripting;

namespace CommandEcho
{
    public class CommandEcho : Plugin
    {
        public CommandEcho()
            : base("cmdecho", "Command echo")
        {
            Author = "Duckbat";
            Version = 1;
            Description = "Echos commands from certain security mask back to other clients. This way you can use your regular MUD client to execute another user's commands. For example if the user is on the phone or whereever.";
            Website = "code.google.com/p/proxymud/";
            UpdateUrl = "www.duckbat.com/plugins/update.cmdecho.txt";

            Config = new CMDEchoConfig();
        }

        public override void OnEnteredCommandAfter(ref string Msg, uint ClientId, int AuthLevel)
        {
            base.OnEnteredCommandAfter(ref Msg, ClientId, AuthLevel);

            ulong echoFrom = Config.GetUInt64("Echo.From.AuthMask", 1);
            if((echoFrom & ((ulong)1 << (AuthLevel - 1))) != 0)
            {
                World.Instance.SendMessage("@w{CommandEcho,Level=" + AuthLevel + ",Id=" + ClientId + "}" + Msg, Config.GetUInt64("Echo.To.AuthMask", ulong.MaxValue - 1));
                Msg = null;
            }
        }
    }

    public class CMDEchoConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("Echo.From.AuthMask", 1, "Whose commands do we echo? This is a security mask. Default is 1 which means users with auth level 1 will send commands to other users.");
            CreateSetting("Echo.To.AuthMask", (ulong.MaxValue - 1), "Who do we echo the commands to. Default is " + (ulong.MaxValue - 1) + " which means every security level except 1.\n\nCommands will be echoed in this format:\n@w{CommandEcho,Level=1,Id=1}say yay\nLevel means Auth level of client who entered command, Id means client id who entered command. If Id is 0 then command was entered from a plugin.");
        }
    }
}
