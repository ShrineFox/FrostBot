using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;

namespace JackFrostBot.UserSettings
{
    public class Invites
    {
        public static ulong GetUser(ulong guildId, string code)
        {
            string xmlPath = $"Servers//{guildId}//Invites.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            ulong userID = 0;
            foreach (var entry in xmldoc.Element("Invites").Elements().Where(x => x.Name == "Code" && x.Value == code))
            {
                userID = Convert.ToUInt64(entry.Attribute("User").Value);
            }

            return userID;
        }

        public static List<string> GetCodes(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Invites.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            List<string> codes = new List<string>();
            foreach (var entry in xmldoc.Element("Invites").Elements())
            {
                codes.Add(entry.Attribute("Code").Value);
            }

            return codes;
        }

        public static void Add(ulong guildId, ulong userID, string code)
        {
            string xmlPath = $"Servers//{guildId}//Invites.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            xmldoc.Element("Invites").Add(new XElement("Invite", new XAttribute("User", userID), new XElement("Code", code)));

            xmldoc.Save(xmlPath);
        }

        public static void Clear(ulong guildId, int index)
        {
            string xmlPath = $"Servers//{guildId}//Invites.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            try
            {
                xmldoc.Element("Invites").Elements().Skip(index).Take(1).Remove();
                xmldoc.Save(xmlPath);
            }
            catch { }
        }

        public static void ClearAll(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Invites.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            foreach (var entry in xmldoc.Element("Invites").Elements())
            {
                entry.Remove();
            }

            xmldoc.Save(xmlPath);
        }

        internal static void Update(ulong guildId, IReadOnlyCollection<IInviteMetadata> invites)
        {
            //Initialize Document
            string xmlPath = $"Servers//{guildId}//Invites.xml";
            XElement root = new XElement("Invites");

            //Reload
            root.Save(xmlPath);
            XDocument xmldoc = XDocument.Load(xmlPath);

            //Add all current invites
            foreach (var invite in invites)
            {
                xmldoc.Element("Invites").Add(new XElement("Invite", new XAttribute("User", invite.Inviter.Id), new XElement("Code", invite.Code)));
            }

            xmldoc.Save(xmlPath);
        }
    }
}
