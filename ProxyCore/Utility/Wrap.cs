using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyCore
{
    public partial class Utility
    {
        public static string[] WrapColored(string text, int maxLength, int IndentSize)
        {
            if(maxLength <= 2)
                return null;

            if(text.Length <= maxLength)
                return new string[1] { text };

            StringBuilder wrapBuilder = new StringBuilder();
            int count = 0;
            bool printed = false;
            int lastOk = 0;
            int realLength = 0;
            int fakeIndex = 0;
            text = text.TrimEnd();
            bool didAt = false;
            bool doNow = false;
            for(int i = 0; i < text.Length; i++)
            {
                switch(text[i])
                {
                    case ' ':
                    /*case ',':
                    case '.':
                    case '?':
                    case '!':
                    case ':':
                    case ';':
                    case ')':
                    case ']':
                    case '}':
                    case '-':*/
                    case '\t':
                        lastOk = i;
                        realLength++;
                        break;

                    case '\n':
                    case '\r':
                        doNow = true;
                        lastOk = i;
                        realLength++;
                        break;

                    case '@':
                        if(didAt)
                            realLength++;
                        didAt = !didAt;
                        break;

                    default:
                        if(!didAt)
                            realLength++;
                        else
                            didAt = false;
                        break;
                }

                if(realLength >= maxLength || doNow)
                {
                    if(printed && IndentSize > 0)
                        wrapBuilder.Append(' ', IndentSize);
                    if(lastOk > 0)
                    {
                        wrapBuilder.Append(text.Substring(fakeIndex, lastOk - fakeIndex + 1).TrimEnd() + Environment.NewLine);
                        i = lastOk;
                    }
                    else
                        wrapBuilder.Append(text.Substring(fakeIndex, i - fakeIndex + 1).TrimEnd() + Environment.NewLine);

                    fakeIndex = lastOk > 0 ? lastOk + 1 : i + 1;
                    lastOk = 0;
                    realLength = IndentSize;
                    printed = true;
                    count++;
                    doNow = false;
                }
            }

            if(fakeIndex < text.Length - 1)
            {
                if(printed && IndentSize > 0)
                    wrapBuilder.Append(' ', IndentSize);
                wrapBuilder.Append(text.Substring(fakeIndex, text.Length - fakeIndex).TrimEnd() + Environment.NewLine);
                count++;
            }

            List<string> lp = wrapBuilder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            lp.RemoveAt(lp.Count - 1);
            return lp.ToArray();
        }

        public static string FormatColoredString(string msg, int len)
        {
            StringBuilder str = new StringBuilder();
            int real = 0;
            bool at = false;
            for(int i = 0; i < msg.Length; i++)
            {
                if(msg[i] == '@')
                {
                    if(at)
                    {
                        real++;
                        str.Append("@@");
                    }
                    at = !at;
                }
                else if(at)
                {
                    str.Append("@" + msg[i].ToString());
                    at = false;
                }
                else
                {
                    str.Append(msg[i]);
                    real++;
                }
            }

            if(real < Math.Abs(len))
            {
                if(len < 0)
                    str.Append(' ', Math.Abs(len) - real);
                else
                    str.Insert(0, " ", len - real);
            }

            return str.ToString();
        }
    }
}
