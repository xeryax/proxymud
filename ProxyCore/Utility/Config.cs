using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ProxyCore
{
    public class ConfigFile
    {
        protected ConfigFile()
        {
            
        }

        private string Filepath;

        /// <summary>
        /// Did we successfully load the config file? If not then it was probably missing.
        /// </summary>
        public bool DidLoad
        {
            get;
            private set;
        }

        /// <summary>
        /// Load a configuration file.
        /// </summary>
        /// <param name="fileName">Configuration name - for example "server". This should be your plugin name.</param>
        /// <returns></returns>
        internal void Load(string fileName)
        {
            fileName = "config." + fileName + ".txt";
            _cfgData.Clear();
            Filepath = fileName;
            OnCreated();

            if(_cfgData.Count == 0)
                return;

            StreamReader f = null;
            try
            {
                f = new StreamReader(fileName);
            }
            catch(FileNotFoundException)
            {
                SaveNew();
                return;
            }
            catch
            {
                return;
            }

            string l;
            while((l = f.ReadLine()) != null)
            {
                l = l.Trim();
                if(string.IsNullOrEmpty(l) || l.StartsWith("#") || l.StartsWith(";") || l.StartsWith("//"))
                    continue;

                Match m = _loadRegex.Match(l);
                if(!m.Success)
                    continue;

                string Key = m.Groups[1].Value.ToLower();
                if(!_cfgData.ContainsKey(Key))
                    continue;

                string Value = m.Groups[3].Value;
                if(Value.Contains('"') && _cfgData[Key].DefaultValue is string)
                    Value = Value.Substring(1, Value.Length - 2);

                if(_cfgData[Key].DefaultValue is int)
                {
                    int i;
                    if(!int.TryParse(Value, out i))
                        continue;

                    _cfgData[Key].Value = i;
                }
                else if(_cfgData[Key].DefaultValue is uint)
                {
                    uint i;
                    if(!uint.TryParse(Value, out i))
                        continue;

                    _cfgData[Key].Value = i;
                }
                else if(_cfgData[Key].DefaultValue is long)
                {
                    long i;
                    if(!long.TryParse(Value, out i))
                        continue;

                    _cfgData[Key].Value = i;
                }
                else if(_cfgData[Key].DefaultValue is ulong)
                {
                    ulong i;
                    if(!ulong.TryParse(Value, out i))
                        continue;

                    _cfgData[Key].Value = i;
                }
                else if(_cfgData[Key].DefaultValue is string)
                {
                    _cfgData[Key].Value = Value;
                }
                else if(_cfgData[Key].DefaultValue is float)
                {
                    try
                    {
                        float i = Convert.ToSingle(Value, CultureInfo.InvariantCulture.NumberFormat);
                        _cfgData[Key].Value = i;
                    }
                    catch
                    {
                        continue;
                    }
                }
                else if(_cfgData[Key].DefaultValue is double)
                {
                    try
                    {
                        double i = Convert.ToDouble(Value, CultureInfo.InvariantCulture.NumberFormat);
                        _cfgData[Key].Value = i;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            f.Close();
        }

        /// <summary>
        /// Regex pattern used to load a line from config file.<para></para>
        /// Groups[1]: Name of setting<para></para>
        /// Groups[3]: Value of setting (including "" if string)
        /// </summary>
        private static readonly Regex _loadRegex = new Regex("([\\w\\.]+)\\s*(=|:|-)\\s*((\".*\")|(-?\\d+\\.\\d+)|(-?\\d+))", RegexOptions.Compiled);

        /// <summary>
        /// Save a new configuration file with default values. If there is an existing config file, it will be replaced.
        /// </summary>
        public void SaveNew()
        {
            if(string.IsNullOrEmpty(Filepath) || _cfgData.Count == 0)
                return;

            StreamWriter f = null;
            try
            {
                f = new StreamWriter(Filepath, false);
            }
            catch
            {
                return;
            }

            foreach(KeyValuePair<string, CfgEntry> x in _cfgData)
            {
                f.WriteLine("###############################################################################");
                f.WriteLine("#");
                f.WriteLine("#    " + x.Value.Key);
                if(!string.IsNullOrEmpty(x.Value.Desc))
                {
                    string[] oDesc = Utility.WrapColored(x.Value.Desc, 70, 0);
                    for(int i = 0; i < oDesc.Length; i++)
                        f.WriteLine("#        " + oDesc[i]);
                }
                f.WriteLine("#");
                f.WriteLine("###############################################################################");
                f.WriteLine("");
                f.WriteLine(x.Value.Key + " = " + ((x.Value.DefaultValue is string) ? ("\"" + x.Value.DefaultValue + "\"") : x.Value.DefaultValue.ToString().Replace(",", ".")));
                f.WriteLine("");
            }

            f.Close();
        }

        /// <summary>
        /// This will be called to populate config data with default values and descriptions.
        /// </summary>
        protected virtual void OnCreated()
        {
        }

        /// <summary>
        /// Create a new setting for the file. If setting with this name already exists then skip.
        /// </summary>
        /// <param name="Key">Setting name.</param>
        /// <param name="Value">Default value for setting. Make sure to use type casting if not integer.</param>
        /// <param name="Desc">Description to write in the file for this setting.</param>
        protected void CreateSetting(string Key, object Value, string Desc)
        {
            if(_cfgData.ContainsKey(Key.ToLower().Trim()))
                return;

            CfgEntry e = new CfgEntry()
            {
                Key = Key,
                Value = Value,
                DefaultValue = Value,
                Desc = Desc
            };

            _cfgData[Key.ToLower().Trim()] = e;
        }

        /// <summary>
        /// Read a 32 bit integer value from configuration file.
        /// </summary>
        /// <param name="Key">Name of the option.</param>
        /// <param name="Default">Default value if config is missing this option or is invalid.</param>
        /// <returns></returns>
        public int GetInt32(string Key, int Default)
        {
            Key = Key.ToLower().Trim();
            if(string.IsNullOrEmpty(Key) || !_cfgData.ContainsKey(Key) || !(_cfgData[Key].Value is int))
                return Default;

            return (int)_cfgData[Key].Value;
        }

        /// <summary>
        /// Read a 32 bit unsigned integer value from configuration file.
        /// </summary>
        /// <param name="Key">Name of the option.</param>
        /// <param name="Default">Default value if config is missing this option or is invalid.</param>
        /// <returns></returns>
        public uint GetUInt32(string Key, uint Default)
        {
            Key = Key.ToLower().Trim();
            if(string.IsNullOrEmpty(Key) || !_cfgData.ContainsKey(Key) || !(_cfgData[Key].Value is uint))
                return Default;

            return (uint)_cfgData[Key].Value;
        }

        /// <summary>
        /// Read a 64 bit integer value from configuration file.
        /// </summary>
        /// <param name="Key">Name of the option.</param>
        /// <param name="Default">Default value if config is missing this option or is invalid.</param>
        /// <returns></returns>
        public long GetInt64(string Key, long Default)
        {
            Key = Key.ToLower().Trim();
            if(string.IsNullOrEmpty(Key) || !_cfgData.ContainsKey(Key) || !(_cfgData[Key].Value is long))
                return Default;

            return (long)_cfgData[Key].Value;
        }

        /// <summary>
        /// Read a 64 bit unsigned integer value from configuration file.
        /// </summary>
        /// <param name="Key">Name of the option.</param>
        /// <param name="Default">Default value if config is missing this option or is invalid.</param>
        /// <returns></returns>
        public ulong GetUInt64(string Key, ulong Default)
        {
            Key = Key.ToLower().Trim();
            if(string.IsNullOrEmpty(Key) || !_cfgData.ContainsKey(Key) || !(_cfgData[Key].Value is ulong))
                return Default;

            return (ulong)_cfgData[Key].Value;
        }

        /// <summary>
        /// Read a float value from configuration file.
        /// </summary>
        /// <param name="Key">Name of the option.</param>
        /// <param name="Default">Default value if config is missing this option or is invalid.</param>
        /// <returns></returns>
        public float GetFloat(string Key, float Default)
        {
            Key = Key.ToLower().Trim();
            if(string.IsNullOrEmpty(Key) || !_cfgData.ContainsKey(Key) || !(_cfgData[Key].Value is float))
                return Default;

            return (float)_cfgData[Key].Value;
        }

        /// <summary>
        /// Read a double value from configuration file.
        /// </summary>
        /// <param name="Key">Name of the option.</param>
        /// <param name="Default">Default value if config is missing this option or is invalid.</param>
        /// <returns></returns>
        public double GetDouble(string Key, double Default)
        {
            Key = Key.ToLower().Trim();
            if(string.IsNullOrEmpty(Key) || !_cfgData.ContainsKey(Key) || !(_cfgData[Key].Value is double))
                return Default;

            return (double)_cfgData[Key].Value;
        }

        /// <summary>
        /// Read a string value from configuration file.
        /// </summary>
        /// <param name="Key">Name of the option.</param>
        /// <param name="Default">Default value if config is missing this option or is invalid.</param>
        /// <returns></returns>
        public string GetString(string Key, string Default)
        {
            Key = Key.ToLower().Trim();
            if(string.IsNullOrEmpty(Key) || !_cfgData.ContainsKey(Key) || !(_cfgData[Key].Value is string))
                return Default;

            return (string)_cfgData[Key].Value;
        }

        private Dictionary<string, CfgEntry> _cfgData = new Dictionary<string, CfgEntry>();

        private class CfgEntry
        {
            public string Key;
            public object Value;
            public object DefaultValue;
            public string Desc;
        }
    }
}
