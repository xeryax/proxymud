using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore;

namespace ProxyCore
{
    public class ServerConfig : ConfigFile
    {
        public ServerConfig()
        {
            Load("server");
        }

        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("Listen.Address", "127.0.0.1", "This is the address that the proxy program will be listening on. You will have to connect to this address with your MUD client. Enter 127.0.0.1 to listen only on current computer, enter LAN IP to listen only in your home network or enter 0.0.0.0 to listen on all addresses (even remote).");
            CreateSetting("Listen.Port", 4000, "This is the port that the proxy program will be listening on. You will have to enter this port in your MUD client. If you want remote connections you may have to open this port (TCP) in your firewall.");
            CreateSetting("MUD.Address", "aardmud.org", "This is the address for MUD connection.");
            CreateSetting("MUD.Port", 4000, "This is the port for MUD connection.");
            CreateSetting("Passwords", "", "Passwords for the proxy and their user levels. For example: \"abc->1,def->2,ghi321->1\". If user enters def as password they will get user level 2. Enter as many passwords as you like. If you leave this empty, the proxy will not ask for a password and user will be authed with level 1. The password values should be between 1 and 64.");
            CreateSetting("GMCP.Supports", "Core=1, Char=1, Room=1, Comm=1", "What GMCP options do we have on by default? These are modules only needed for the proxy. If your client needs another module it will enable it itself - no need to change here.");
            CreateSetting("AutoConnect", 0, "Proxy program will always automatically (re)connect to MUD if there is a client online in the proxy. If there is no client online the proxy will not automatically connect. Enabling this option will force the proxy to always auto (re)connect.");
            CreateSetting("ClientCompression", 1, "Enable compression for connected clients (MUD compression will always be on, even if you disable this option). Don't disable unless you are having problems with compression and can't disable client side.");
        }
    }

    public class CoreConfig : ConfigFile
    {
        public CoreConfig()
        {
            Load("core");
        }

        protected override void OnCreated()
        {
            base.OnCreated();

            CreateSetting("Updates.Core", 1, "Automatically check updates for core.");
            CreateSetting("Updates.Plugins", 1, "Automatically check updates for plugins.");
        }
    }
}
