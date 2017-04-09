using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ProxyCore.Scripting
{
    public static class PluginMgr
    {
        internal static void LoadAll()
        {
            if(Plugins.Count != 0)
                return;

            try
            {
                if(!Directory.Exists("plugins"))
                {
                    Directory.CreateDirectory("plugins");
                    return;
                }
            }
            catch
            {
                return;
            }

            string[] files = Directory.GetFiles("plugins", "*.dll");

            if(files.Length != 0)
            {
                foreach(string file in files)
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(Path.GetFullPath(file));
                        foreach(Type type in assembly.GetTypes())
                        {
                            if(!type.IsClass || type.IsNotPublic)
                                continue;

                            if(type.BaseType == typeof(Plugin))
                            {
                                try
                                {
                                    Plugin obj = (Plugin) Activator.CreateInstance(type);
                                    obj.ClassName = type.ToString();
                                    if(obj.RequiredCoreVersion > World.Version)
                                        throw new Exception("Newer version of core is needed! (" + obj.RequiredCoreVersion + ")");
                                    if(string.IsNullOrEmpty(obj.Keyword.Trim()) || string.IsNullOrEmpty(obj.Name.Trim()))
                                        throw new Exception("Plugin has invalid parameters!");
                                    if(Plugins.ContainsKey(obj.Keyword.ToLower().Trim()))
                                    {
                                        Plugin prev = Plugins[obj.Keyword.ToLower().Trim()];
                                        if(prev.Version >= obj.Version)
                                            throw new Exception("A newer version of this plugin was already loaded!");
                                    }
                                    if(obj.Keyword.ToLower().Trim() == "core" || obj.Keyword.ToLower().Trim() == "server")
                                        throw new Exception("Plugin has invalid keyword!");

                                    if(obj.Config != null)
                                    {
                                        obj.Config.Load(obj.Keyword.ToLower().Trim());
                                        obj.OnLoadedConfig(obj.Config.DidLoad);
                                    }
                                    Plugins[obj.Keyword.ToLower().Trim()] = obj;
                                    Log.Write("Loaded: [" + obj.Keyword.ToLower().Trim() + "] " + obj.Name + ", version " + obj.Version.ToString() + ".");
                                }
                                catch(Exception e)
                                {
                                    Log.Write("Failed: [" + type.ToString() + "] in " + file + "!");
                                    Log.Write("        " + e.Message);
                                    continue;
                                }
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            Log.Write("Done.");
        }

        internal static Dictionary<string, Plugin> Plugins = new Dictionary<string, Plugin>();

        /// <summary>
        /// Get plugin by keyword.
        /// </summary>
        /// <param name="Keyword">Keyword of plugin.</param>
        /// <returns></returns>
        public static Plugin GetPlugin(string Keyword)
        {
            Keyword = Keyword.ToLower().Trim();
            return Plugins.ContainsKey(Keyword) ? Plugins[Keyword] : null;
        }
    }
}
