using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProxyCore
{
    public static class Log
    {
        /// <summary>
        /// Write this into console window.
        /// </summary>
        /// <param name="msg">Message to write to console window.</param>
        public static void Write(string msg)
        {
            Console.WriteLine(string.Format("[" + "{0:D2}" + ":" + "{1:D2}" + ":" + "{2:D2}" + "] ", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) + msg);
        }

        /// <summary>
        /// Write an error message to console window and error.log
        /// </summary>
        /// <param name="msg"></param>
        public static void Error(string msg)
        {
            Write(msg);
            try
            {
                StreamWriter f = new StreamWriter("error.log", true);
                f.WriteLine(string.Format("[" + "{0:D2}" + ":" + "{1:D2}" + ":" + "{2:D2}" + "] ", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) + msg);
                f.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Write a stacktrace when program crashes.
        /// </summary>
        /// <param name="StackTrace">Trace to write.</param>
        public static void Crash(Exception e, string Module)
        {
            string[] E = e.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            World.Instance.SendMessage("@RCRASH: Proxy crashed in module \"@w" + Module + "@R\"! Check error.log for details and send stack trace to author of module.");
            Error("Program crashed in module (" + Module + ") with exception: \"" + e.Message + "\", type: \"" + e.GetType().ToString() + "\", trace:");
            foreach(string x in E)
                Error(x);
            Error("Trace end.");
        }
    }
}
