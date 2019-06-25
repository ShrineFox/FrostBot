using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class Verification
    {
        public static bool Enabled(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Verification.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            return Convert.ToBoolean(xmldoc.Element("Verification").Element("Enabled").Value);
        }

        public static ulong WelcomeChannelId(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Verification.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var channelId = Convert.ToUInt64(xmldoc.Element("Verification").Element("WelcomeChannel").FirstAttribute.Value);

            return channelId;
        }
        public static ulong VerificationChannelId(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Verification.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var channelId = Convert.ToUInt64(xmldoc.Element("Verification").Element("VerificationChannel").FirstAttribute.Value);

            return channelId;
        }
        public static string VerificationMessage(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Verification.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var messageText = xmldoc.Element("Verification").Element("VerificationChannel").Element("VerificationMessage").FirstAttribute.Value;

            return messageText;
        }

        public static bool IsCaseSensitive(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Verification.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var caseSensitive = Convert.ToBoolean(xmldoc.Element("Verification").Element("VerificationChannel").Element("VerificationMessage").LastAttribute.Value);

            return caseSensitive;
        }

        public static ulong MemberRoleID(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Verification.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var roleId = Convert.ToUInt64(xmldoc.Element("Verification").Element("MemberRole").FirstAttribute.Value);

            return roleId;
        }
    }
}
