using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore.Scripting;
using ProxyCore.Input;
using ProxyCore.Output;
using ProxyCore;

namespace GQPredict
{
    public class GQPredict : Plugin
    {
        public GQPredict()
            : base("gqpredict", "Global Quest Predictor")
        {
            Author = "Duckbat";
            Version = 1;
            Description = "Calculates the chance that the next global quest will be for you (or the level you specify). And shows the ranges that will occur and have already occured in a simple list sorted by range. Type gqpredict to see.";
            UpdateUrl = "www.duckbat.com/plugins/update.gqpredict.txt";
            Website = "www.duckbat.com/plugins/index.php?t=gqpredict";

            RegisterCommand("gqpredict", @"(\d+)", Predict, 3);
            RegisterTrigger("ranges", @"^        \s*@w(\d+)\s+(\d+)\s+(Yes|No)$", Ranges);
            RegisterTrigger("start", "@CFrom Level  To Level   Already Run?", Start, TriggerFlags.NotRegex);
            RegisterTrigger("delimiter", "@W----------- ---------  ------------", Delim, TriggerFlags.NotRegex);
            RegisterTrigger("level", @"^\$gmcp\.char\.status\.level (\d+)$", Level);
        }

        private int nt;
        private int forLevel = 1;
        private int charLevel = 1;
        private int count;
        private uint[] ClientMask;
        private int Listen = 0;
        private readonly SortedDictionary<int, SortedDictionary<int, List<bool>>> gqRanges = new SortedDictionary<int, SortedDictionary<int, List<bool>>>();

        private bool Predict(InputData i)
        {
            Listen = 3;
            if(i.Arguments.Success)
            {
                if(!int.TryParse(i.Arguments.Groups[1].Value, out forLevel))
                    forLevel = charLevel;
            }
            else
                forLevel = charLevel;

            i.Command = "gq ranges";
            ClientMask = i.ClientMask;
            return false;
        }

        private bool Ranges(TriggerData t)
        {
            if(Listen == 0)
                return false;
            int from;
            int to;
            if(!int.TryParse(t.Match.Groups[1].Value, out from) ||
                !int.TryParse(t.Match.Groups[2].Value, out to))
                return false;

            if(!gqRanges.ContainsKey(from))
                gqRanges[from] = new SortedDictionary<int, List<bool>>();
            if(!gqRanges[from].ContainsKey(to))
                gqRanges[from][to] = new List<bool>();
            gqRanges[from][to].Add(t.Match.Groups[3].Length == 3);
            if(t.Match.Groups[3].Length != 3)
                count++;
            if(to == 201)
            {
                nt++;
                if(nt == 3)
                {
                    StringBuilder str = new StringBuilder();
                    int i = 0;
                    int myGQ = 0;
                    foreach(KeyValuePair<int, SortedDictionary<int, List<bool>>> x in gqRanges)
                    {
                        foreach(KeyValuePair<int, List<bool>> y in x.Value)
                        {
                            foreach(bool z in y.Value)
                            {
                                if(forLevel < x.Key || forLevel > y.Key || z)
                                    str.Append(z ? "@r" : "@g");
                                else
                                {
                                    str.Append("@G");
                                    myGQ++;
                                }
                                str.Append("(" + string.Format("{0,3}", x.Key) + " - " + string.Format("{0,3}", y.Key) +
                                           ") ");
                                i++;
                                if(i == 6)
                                {
                                    World.Instance.SendMessage(str.ToString(), ClientMask);
                                    i = 0;
                                    str.Remove(0, str.Length);
                                }
                            }
                        }
                    }

                    if(i > 0)
                        World.Instance.SendMessage(str.ToString(), ClientMask);

                    double chance = 0;
                    if(count > 0 && myGQ > 0)
                        chance = (double)myGQ / (double)count * 100.0;
                    World.Instance.SendMessage("@wChance that the next global quest will be for " + (forLevel != charLevel ? ("level " + forLevel) : "your level") + " is @Y" + string.Format("{0:0.00}", chance).Replace(',', '.') + "%@w.", ClientMask);
                    if(forLevel == charLevel)
                        World.Instance.SendMessage("@wUse '@Wgqpredict <level>@w' to see from the viewpoint of another level.", ClientMask);
                    Listen = 0;
                }
            }
            return true;
        }

        private bool Level(TriggerData t)
        {
            int i;
            if(int.TryParse(t.Match.Groups[1].Value, out i))
                charLevel = i;
            return false;
        }

        private bool Start(TriggerData t)
        {
            nt = 0;
            gqRanges.Clear();
            count = 0;
            if(Listen > 0)
            {
                Listen--;
                return true;
            }
            return false;
        }

        private bool Delim(TriggerData t)
        {
            if(Listen > 0)
            {
                Listen--;
                return true;
            }
            return false;
        }
    }
}
