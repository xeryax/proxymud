using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ProxyCore
{
    internal static class JSON
    {
        internal static bool Parse(string msg, string module, List<KeyValuePair<string, string>> data)
        {
            try
            {
                JsonObject obj = (JsonObject)JsonConvert.Import(msg);
                Explore(module, obj, data);
            }
            catch
            {
                //data.Clear();
                return false;
            }
            return true;
        }

        private static void Explore(string nameSpace, JsonObject obj, List<KeyValuePair<string, string>> x)
        {
            var keys = obj.Names;
            foreach(string i in keys)
                ExplorePair(nameSpace + "." + i, obj[i], x);
        }

        private static void ExplorePair(string nameSpace, object obj, List<KeyValuePair<string, string>> x)
        {
            if((obj is JsonNumber) ||
                (obj is JsonNull) ||
                (obj is string) ||
                (obj is JsonBoolean) ||
                (obj is JsonString))
                x.Add(new KeyValuePair<string, string>(nameSpace, obj.ToString()));
            else if(obj is JsonArray)
            {
                foreach(JsonObject i in (JsonArray)obj)
                    Explore(nameSpace, i, x);
            }
            else if(obj is JsonObject)
                Explore(nameSpace, (JsonObject)obj, x);
            else if(obj == null)
                x.Add(new KeyValuePair<string, string>(nameSpace, "null"));
            else throw new Exception();
        }
    }
}
