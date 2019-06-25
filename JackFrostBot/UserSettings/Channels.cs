using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class Channels
    {
        public static ulong WelcomeChannelId(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Channels.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var channelId = Convert.ToUInt64(xmldoc.Element("Channels").Element("Welcome").Elements().FirstOrDefault().FirstAttribute.Value);

            return channelId;
        }

        public static bool WelcomeOnJoin(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Channels.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var welcome = Convert.ToBoolean(xmldoc.Element("Channels").Element("Welcome").Elements().FirstOrDefault().Elements().FirstOrDefault().Value);

            return welcome;
        }

        public static ulong BotLogsId(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Channels.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var channelId = Convert.ToUInt64(xmldoc.Element("Channels").Element("BotLogs").Elements().FirstOrDefault().FirstAttribute.Value);

            return channelId;
        }

        public static bool BotLogsEnabled(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Channels.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var enabled = Convert.ToBoolean(xmldoc.Element("Channels").Element("BotLogs").Elements().FirstOrDefault().Elements().FirstOrDefault().Value);

            return enabled;
        }

        public static ulong BotChannelId(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Channels.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var channelId = Convert.ToUInt64(xmldoc.Element("Channels").Element("BotChannel").Elements().FirstOrDefault().FirstAttribute.Value);

            return channelId;
        }

        public static List<ulong> MediaOnlyChannels(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Channels.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var channels = xmldoc.Element("Channels").Element("Media-Only").Elements().Where(e => e.Name == "Channel");

            List<ulong> channelIds = new List<ulong>();
            foreach (var channel in channels)
            {
                channelIds.Add(Convert.ToUInt64(channel.FirstAttribute.Value));
            }
            return channelIds;
        }

        public static ulong PinsChannelId(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Channels.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var channelId = Convert.ToUInt64(xmldoc.Element("Channels").Element("Pins").Elements().FirstOrDefault().FirstAttribute.Value);

            return channelId;
        }
    }
}
