using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProxyCore;
using ProxyCore.Scripting;

namespace MudLog
{
    public class MudLog : Plugin
    {
        public MudLog()
            : base("mudlog", "MUD Logger")
        {
            Author = "Duckbat";
            Version = 1;
            Description = "This plugin will log (write down into a file) all text that comes from MUD so you can later look at it if you like.";
            UpdateUrl = "www.duckbat.com/plugins/update.mudlog.txt";
            Website = "www.duckbat.com/plugins/index.php?t=mudlog";

            Config = new MudLogConfig();
        }

        private StreamWriter f;

        public override void OnLoadedConfig(bool Success)
        {
            base.OnLoadedConfig(Success);

            if(Config.GetInt32("Log.Enabled", 1) != 0)
            {
                string fn = Config.GetString("Log.Filename", "log.{year}-{month}-{day}.{hour}-{minute}-{second}.txt");
                fn = fn.Replace("{year}", DateTime.Now.Year.ToString());
                fn = fn.Replace("{month}", DateTime.Now.Month.ToString());
                fn = fn.Replace("{day}", DateTime.Now.Day.ToString());
                fn = fn.Replace("{hour}", DateTime.Now.Hour.ToString());
                fn = fn.Replace("{minute}", DateTime.Now.Minute.ToString());
                fn = fn.Replace("{second}", DateTime.Now.Second.ToString());

                if(fn.Contains("/"))
                    fn = fn.Substring(fn.LastIndexOf("/") + 1);
                if(fn.Contains("\\"))
                    fn = fn.Substring(fn.LastIndexOf("\\") + 1);

                if(string.IsNullOrEmpty(fn))
                    return;

                if(!Directory.Exists("logs"))
                {
                    try
                    {
                        Directory.CreateDirectory("logs");
                    }
                    catch
                    {
                        return;
                    }
                }

                f = new StreamWriter("logs/" + fn, true);
            }
        }

        public override void OnReceivedLineAfter(ProxyCore.Messages.Message Msg)
        {
            base.OnReceivedLineAfter(Msg);

            if(f != null)
            {
                string m = Msg.Msg;
                if(Config.GetInt32("Log.StripColors", 0) != 0)
                    m = Colors.RemoveColors(m, false);
                f.WriteLine(
                    string.Format("[" + "{0:D2}" + ":" + "{1:D2}" + ":" + "{2:D2}" + "] ", DateTime.Now.Hour,
                                  DateTime.Now.Minute, DateTime.Now.Second) + m);
                //$gmcp.comm.channel.msg Floki MUSIC:  -- Floki = Player MUSIC = channel
            }
        }

        public override void OnEnteredCommandAfter(ref string Msg, uint ClientId, int AuthLevel)
        {
            base.OnEnteredCommandAfter(ref Msg, ClientId, AuthLevel);

            if(f != null && Config.GetInt32("Log.Commands", 0) != 0)
                f.WriteLine(string.Format("[" + "{0:D2}" + ":" + "{1:D2}" + ":" + "{2:D2}" + "] Sent to MUD: ", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) + Msg);
        }

        public override void Shutdown()
        {
            base.Shutdown();

            if(f != null)
                f.Close();
        }
    }

    public class MudLogConfig : ConfigFile
    {
        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("Log.Enabled", 1, "Enable logging of all what happens in MUD to a file.");
            CreateSetting("Log.Filename", "log.{year}-{month}-{day}.{hour}-{minute}-{second}.txt", "File name of log. You can use {year}, {month}, {day}, {hour}, {minute}, {second} in the filename. If a log file with this name already exists in logs folder then it will be appended to. Be careful however if the log file gets too big this will become slow so it's highly recommended that you differentiate them by including the timestamp keywords somehow.");
            CreateSetting("Log.Commands", 0, "Log commands sent to MUD (not ones that were handled by plugins).");
            CreateSetting("Log.StripColors", 0, "Remove all color codes when logging.");
        }
    }
}
