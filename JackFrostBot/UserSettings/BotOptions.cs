using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class BotOptions
    {
        public static char CommandPrefix(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var commandPrefix = Convert.ToChar(xmldoc.Element("BotOptions").Element("CommandPrefix").Value);

            return commandPrefix;
        }
        public static int MuteLevel(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var mute = Convert.ToInt32(xmldoc.Element("BotOptions").Element("WarnLevels").Element("Mute").Value);

            return mute;
        }

        public static int KickLevel(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var kick = Convert.ToInt32(xmldoc.Element("BotOptions").Element("WarnLevels").Element("Kick").Value);

            return kick;
        }

        public static int BanLevel(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var ban = Convert.ToInt32(xmldoc.Element("BotOptions").Element("WarnLevels").Element("Ban").Value);

            return ban;
        }
        public static int MaximumDuplicates(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var maxDupes = Convert.ToInt32(xmldoc.Element("BotOptions").Element("MessageLimits").Element("MaximumDuplicates").Value);

            return maxDupes;
        }

        public static int MinimumLetters(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var minLetters = Convert.ToInt32(xmldoc.Element("BotOptions").Element("MessageLimits").Element("MinimumAlphanumericCharacters").Value);

            return minLetters;
        }

        public static bool AutoWarnDuplicates(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var botChannelOnly = Convert.ToBoolean(xmldoc.Element("BotOptions").Element("MessageLimits").Element("AutoWarnDuplicates").Value);

            return botChannelOnly;
        }

        public static int MinimumLength(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var minLength = Convert.ToInt32(xmldoc.Element("BotOptions").Element("MessageLimits").Element("MinimumLength").Value);

            return minLength;
        }

        public static int DuplicateFrequencyThreshold(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var dupeFrequency = Convert.ToInt32(xmldoc.Element("BotOptions").Element("MessageLimits").Element("DuplicateFrequencyThreshold").Value);

            return dupeFrequency;
        }

        public static string GetString(string elementName, ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var msg = xmldoc.Element("BotOptions").Element("Strings").Element(elementName).Value;

            return msg;
        }

        public static bool AutoMarkov(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var enableMarkov = Convert.ToBoolean(xmldoc.Element("BotOptions").Element("Markov").Element("AutoMarkov").Value);

            return enableMarkov;
        }

        public static bool MarkovBotChannelOnly(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var botChannelOnly = Convert.ToBoolean(xmldoc.Element("BotOptions").Element("Markov").Element("BotChannelOnly").Value);

            return botChannelOnly;
        }

        public static int AutoMarkovFrequency(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var markovRate = Convert.ToInt32(xmldoc.Element("BotOptions").Element("Markov").Element("AutoMarkovFrequency").Value);

            return markovRate;
        }

        public static int MarkovMinimumLength(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//BotOptions.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var markovLength = Convert.ToInt32(xmldoc.Element("BotOptions").Element("Markov").Element("MinimumLength").Value);

            return markovLength;
        }
    }
}
