using System;
using System.Collections.Generic;

namespace FrostBot
{
    public class Settings
    {
        public string Token { get; set; } = "";
        public string ForumURL { get; set; } = "";
        public List<Tuple<string,string>> ForumChannelIds { get; set; } = new List<Tuple<string, string>>();

        public void Load(string[] args)
        {
            // Get token from commandline args
            if (args != null && args[0].Length == 70)
                Token = args[0];
        }
    }
}