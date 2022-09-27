using System;
using System.Collections.Generic;

namespace FrostBot
{
    public class Settings
    {
        public string Token { get; set; } = "";
        public string ForumURL { get; set; } = "";
        public List<Tuple<string,string>> ForumChannelIds { get; set; } = new List<Tuple<string, string>>();
    }
}