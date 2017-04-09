//#define MONO
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using ProxyCore;

namespace ProxyMud
{
    class Program
    {
        internal static ServerConfig Config;

        static void Main(string[] args)
        {
#if !MONO
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
#endif

            // Set US culture so config and other formats are universal
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            Config = new ServerConfig();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // This is the main loop of the server program
            while(canContinue)
            {
                // The main loop time
                const uint sleepTime = 5;

                long time;

                // This is the main loop of the game world
                while(canContinue)
                {
                    // Get time before loop
                    time = watch.ElapsedMilliseconds;

                    // Start server if needed
                    if(Server == null)
                    {
#if !DEBUG
                        try
                        {
#endif
                            Server = new Network.NetworkServer();
                            Server.Start();
#if !DEBUG
                        }
                        catch(Exception e)
                        {
                            Log.Error("Failed creating server: " + e.Message);
                            Shutdown();
                            break;
                        }
#endif
                    }

                    // Update server and everything
#if !DEBUG
                    try
                    {
#endif
                    if(Server.Update(watch.ElapsedMilliseconds))
                        Shutdown();
#if !DEBUG
                    }
                    catch(Exception e)
                    {
                        Log.Crash(e, "server");
                        Shutdown();
                        break;
                    }
#endif

                    // Now check time after loop and see how long we should sleep to fill loop time
                    time = watch.ElapsedMilliseconds - time;

                    // Need to sleep some time until next update
                    if(time <= sleepTime)
                    {
                        time = sleepTime - time;
                        Thread.Sleep((int)time);
                    }
                    else
                        Thread.Sleep(0); // Sleep at least 0 every time so program wouldn't hog CPU
                }

                // Loop ended save and shut down
                if(Server != null)
                    Server.Stop();
            }

            isFinished = true;
        }

        private static Network.NetworkServer Server = null;

#region CloseEvent
#if !MONO
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch(sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    Shutdown();
                    break;
                default:
                    break;
            }
            while(!isFinished)
                Thread.Sleep(10);
            return false;
        }
#endif
        private static bool canContinue = true;
        private static bool isFinished = false;

        private static void Shutdown()
        {
            canContinue = false;
        }
#endregion
    }
}
