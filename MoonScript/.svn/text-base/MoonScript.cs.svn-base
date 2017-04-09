using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProxyCore.Scripting;
using ProxyCore.Output;
using ProxyCore.Input;
using ProxyCore;

namespace MoonScript
{
    public class MoonScript : Plugin
    {
        public MoonScript() : base("moons", "Moon Script")
        {
            // Set up extra information (optional).
            Author = "Duckbat";
            Version = 1;
            Description = "Tracks the position of three moons and displays how many ticks are until a moon rises or falls. Also tracks, and if you want, alerts when all three moons are about to rise. You need to have been outside and seen all three moons rise or fall at least once for it to work.";
            RequiredPlayerConfig.Add("noweather ON"); // Need this to see the moons.
            UpdateUrl = "www.duckbat.com/plugins/update.moons.txt";
            Website = "www.duckbat.com/plugins/index.php?t=moons";

            // Register some triggers needed for the moon script
            RegisterTrigger("whiterise", "@WYou see the white moon rising in the west.", OnSeeMoon, TriggerFlags.NotRegex, 1000, 3);
            RegisterTrigger("whitefall", "@WYou notice the white moon falling to the east.", OnSeeMoon, TriggerFlags.NotRegex, 1000, -3);
            RegisterTrigger("greyrise", "@[wD]You see the grey moon rising in the east.", OnSeeMoon, TriggerFlags.None, 1000, 2);
            RegisterTrigger("greyfall", "@[wD]You notice the grey moon falling to the west.", OnSeeMoon, TriggerFlags.None, 1000, -2);
            RegisterTrigger("blackrise", "@BYou see the black moon rising in the east.", OnSeeMoon, TriggerFlags.NotRegex, 1000, 1);
            RegisterTrigger("blackfall", "@BYou notice the black moon falling to the west.", OnSeeMoon, TriggerFlags.NotRegex, 1000, -1);
            RegisterTrigger("tick", "$gmcp.comm.tick", OnTick, TriggerFlags.NotRegex);

            // Register commands to check on the moons
            RegisterCommand("moons", @"^alert\s+(on|off)$", MoonsCommand, 4);
        }

        private int AlertMoons = -1;

        /// <summary>
        /// This command is set up so if you type moons without arguments you will see the status of all moons.
        /// If you type argument "alert on" or "alert off" you will change whether the plugin will alert us
        /// when three moons are about to rise.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private bool MoonsCommand(InputData i)
        {
            // If arguments capture was success it means user typed "alert on" or "alert off"
            if(i.Arguments.Success)
            {
                // If length of captured word is 2 then we must have typed "on" and not "off"
                if(i.Arguments.Groups[1].Length == 2)
                {
                    AlertMoons = 3; // Change this to any amount you like, you will be alerted this many ticks before moons rise.
                    World.Instance.SendMessage("@wYou will now be alerted " + AlertMoons + " tick" + (AlertMoons != 1 ? "s" : "") + " before three moons appear.", i.ClientMask);
                }
                else // Otherwise it's "off"
                {
                    AlertMoons = -1;
                    World.Instance.SendMessage("@wYou will no longer be alerted of three moons.", i.ClientMask);
                }
            }
            else
            {
                /* Only send to the client that typed this alias, other's don't need to see it.
                 * This is what i.ClientMask is here. If it was called from a plugin send to all
                 * connected clients. */
                World.Instance.SendMessage("@w+-----------------------------+", i.ClientMask);
                World.Instance.SendMessage("@w|            @gMOONS            @w|", i.ClientMask);
                World.Instance.SendMessage("@w+-----------------------------+", i.ClientMask);
                if(!HasSeenMoon(MoonTypes.Black))
                    World.Instance.SendMessage("@w| @cBlack@w: " + string.Format("{0,-21}", "Unknown") + "|", i.ClientMask);
                else
                {
                    int j = WhenAreMoonsUp(MoonTypes.Black);
                    if(j == 0)
                    {
                        j = HowLongAreMoonsUp(MoonTypes.Black);
                        World.Instance.SendMessage("@w| @cBlack@w: @G" + string.Format("{0,-21}", j) + "@w|", i.ClientMask);
                    }
                    else
                        World.Instance.SendMessage("@w| @cBlack@w: @W" + string.Format("{0,-21}", j) + "@w|", i.ClientMask);
                }
                if(!HasSeenMoon(MoonTypes.Grey))
                    World.Instance.SendMessage("@w| @cGrey @w: " + string.Format("{0,-21}", "Unknown") + "|", i.ClientMask);
                else
                {
                    int j = WhenAreMoonsUp(MoonTypes.Grey);
                    if(j == 0)
                    {
                        j = HowLongAreMoonsUp(MoonTypes.Grey);
                        World.Instance.SendMessage("@w| @cGrey @w: @G" + string.Format("{0,-21}", j) + "@w|", i.ClientMask);
                    }
                    else
                        World.Instance.SendMessage("@w| @cGrey @w: @W" + string.Format("{0,-21}", j) + "@w|", i.ClientMask);
                }
                if(!HasSeenMoon(MoonTypes.White))
                    World.Instance.SendMessage("@w| @cWhite@w: " + string.Format("{0,-21}", "Unknown") + "|", i.ClientMask);
                else
                {
                    int j = WhenAreMoonsUp(MoonTypes.White);
                    if(j == 0)
                    {
                        j = HowLongAreMoonsUp(MoonTypes.White);
                        World.Instance.SendMessage("@w| @cWhite@w: @G" + string.Format("{0,-21}", j) + "@w|", i.ClientMask);
                    }
                    else
                        World.Instance.SendMessage("@w| @cWhite@w: @W" + string.Format("{0,-21}", j) + "@w|", i.ClientMask);
                }
                if(!HasSeenMoon(MoonTypes.Black) || !HasSeenMoon(MoonTypes.Grey) || !HasSeenMoon(MoonTypes.White))
                    World.Instance.SendMessage("@w| @cAll  @w: " + string.Format("{0,-21}", "Unknown") + "|", i.ClientMask);
                else
                {
                    int j = WhenAreMoonsUp(MoonTypes.Black, MoonTypes.Grey, MoonTypes.White);
                    if(j == 0)
                    {
                        j = HowLongAreMoonsUp(MoonTypes.Black, MoonTypes.Grey, MoonTypes.White);
                        World.Instance.SendMessage("@w| @cAll  @w: @G" + string.Format("{0,-21}", j) + "@w|", i.ClientMask);
                    }
                    else
                    {
                        string k = j + "(" + HowLongAreMoonsUp(MoonTypes.Black, MoonTypes.Grey, MoonTypes.White) + ")";
                        World.Instance.SendMessage("@w| @cAll  @w: @W" + string.Format("{0,-21}", k) + "@w|",
                                                   new[] {i.ClientId});
                    }
                }
                World.Instance.SendMessage("@w+-----------------------------+", i.ClientMask);
                if(AlertMoons == -1)
                    World.Instance.SendMessage("@wUse '@Wmoons alert on@w' to see alert when three moons are about to rise.");
                else
                    World.Instance.SendMessage("@wUse '@Wmoons alert off@w' if you don't want to be notified of three moons.");
            }

            return true;
        }

        private bool OnSeeMoon(TriggerData t)
        {
            /* Arg here is what we used to register trigger with, for example if you see whiterise trigger
             * I set the argument to be 3 so it will go to case 3: */
            switch(t.Arg)
            {
                case 3:
                    MoonTimer[(int)MoonTypes.White] = MoonDuration[(int)MoonTypes.White] - 1;
                    break;
                case -3:
                    MoonTimer[(int)MoonTypes.White] = 0;
                    break;

                case 2:
                    MoonTimer[(int)MoonTypes.Grey] = MoonDuration[(int)MoonTypes.Grey] - 1;
                    break;
                case -2:
                    MoonTimer[(int)MoonTypes.Grey] = 0;
                    break;

                case 1:
                    MoonTimer[(int)MoonTypes.Black] = MoonDuration[(int)MoonTypes.Black] - 1;
                    break;
                case -1:
                    MoonTimer[(int)MoonTypes.Black] = 0;
                    break;
            }
            return false;
        }

        /// <summary>
        /// Full interval of moons (black, grey, white).
        /// </summary>
        private readonly int[] MoonInterval = new[] { 50, 30, 65 };

        /// <summary>
        /// Duration of the moons, this is inverted so duration is actually MoonInterval[i] - MoonDuration[i].
        /// </summary>
        private readonly int[] MoonDuration = new[] { 39, 24, 50 };

        /// <summary>
        /// Current tick timers on the moons. -1 means unknown haven't seen moon yet.
        /// </summary>
        private int[] MoonTimer = new[] { -1, -1, -1 };

        private bool OnTick(TriggerData t)
        {
            for(int i = 0; i < MoonTimer.Length; i++)
            {
                // Increase moon timers if we have seen a moon rise or fall.
                if(MoonTimer[i] == -1)
                    continue;
                MoonTimer[i]++;
                MoonTimer[i] %= MoonInterval[i];
            }

            // See if we must alert users when moons are about to come up.
            if(AlertMoons != -1)
            {
                int i = WhenAreMoonsUp(MoonTypes.Black, MoonTypes.Grey, MoonTypes.White);
                if(i == 0)
                {
                    if(!IsMoonUp(MoonTypes.Black, -1) || !IsMoonUp(MoonTypes.Grey, -1) || !IsMoonUp(MoonTypes.White, -1))
                        World.Instance.SendMessage("@GMOONS: @wThree moons are now @WUP@w!");
                }
                else if(i <= AlertMoons)
                {
                    World.Instance.SendMessage("@GMOONS: @wThree moons will be up in @W" + i + " @wtick" + (i != 1 ? "s" : "") + ".");
                }
            }
            return false;
        }

        private void Reset()
        {
            // Reset moon timers to unknown, this will be called if we lose connection.
            MoonTimer = new[] { -1, -1, -1 };
        }

        public override void OnDisconnect()
        {
            base.OnDisconnect();
            Reset();
        }

        /// <summary>
        /// Check if moon is up.
        /// </summary>
        /// <param name="i">Moon type to check for.</param>
        /// <param name="tickOffset">Offset to current tick. For example 0 would check if moon is currently up but 1 would check if moon is up in 1 tick.</param>
        /// <returns></returns>
        public bool IsMoonUp(MoonTypes i, int tickOffset)
        {
            if(MoonTimer[(int)i] == -1)
                return false;
            return (MoonTimer[(int)i] + tickOffset) % MoonInterval[(int)i] >= MoonDuration[(int)i] - 1;
        }

        /// <summary>
        /// Check if we have seen a moon rise or fall.
        /// </summary>
        /// <param name="i">Moon type to check for.</param>
        /// <returns></returns>
        public bool HasSeenMoon(MoonTypes i)
        {
            return MoonTimer[(int)i] != -1;
        }

        /// <summary>
        /// Check in how many ticks are all these moons up.
        /// </summary>
        /// <param name="args">Moons types to check for. Can be more than one or all.</param>
        /// <returns></returns>
        public int WhenAreMoonsUp(params MoonTypes[] args)
        {
            if(args.Length == 0)
                return 0;

            foreach(MoonTypes t in args)
            {
                if(!HasSeenMoon(t))
                    return -1;
            }

            int i = 0;
            while(true)
            {
                bool are = true;
                for(int j = 0; j < args.Length; j++)
                {
                    if(!IsMoonUp(args[j], i))
                    {
                        are = false;
                        break;
                    }
                }

                if(!are)
                {
                    i++;
                    continue;
                }

                return i;
            }
        }

        /// <summary>
        /// Next time these moons are up how long are they up for (how many ticks).
        /// </summary>
        /// <param name="args">Moons to check for. Can be more than one or all.</param>
        /// <returns></returns>
        public int HowLongAreMoonsUp(params MoonTypes[] args)
        {
            if(args.Length == 0)
                return 0;

            foreach(MoonTypes t in args)
            {
                if(!HasSeenMoon(t))
                    return -1;
            }

            int i = WhenAreMoonsUp(args);
            int k = 0;
            while(true)
            {
                bool are = true;
                for(int j = 0; j < args.Length; j++)
                {
                    if(!IsMoonUp(args[j], i))
                    {
                        are = false;
                        break;
                    }
                }

                if(!are)
                    return k;

                k++;
                i++;
            }
        }
    }

    public enum MoonTypes
    {
        Black = 0,
        Grey = 1,
        White = 2,
    }
}
