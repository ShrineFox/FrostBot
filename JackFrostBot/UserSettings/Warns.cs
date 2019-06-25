using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class Warns
    {
        public static List<Tuple<string, string>> Get(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Warns.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            List<Tuple<string, string>> warns = new List<Tuple<string, string>>();
            foreach (var entry in xmldoc.Element("Warns").Elements())
            {
                warns.Add(new Tuple<string, string>(entry.Attribute("UserID").Value, entry.Element("Reason").Value));
            }
                
            return warns;
        }

        public static Tuple<string, string> Get(ulong guildId, int index)
        {
            string xmlPath = $"Servers//{guildId}//Warns.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var warn = xmldoc.Element("Warns").Elements().Skip(index).Take(1).First();

            Tuple<string, string> tuple = new Tuple<string, string>(warn.Attribute("UserID").Value, warn.Element("Reason").Value);

            return tuple;
        }

        public static void Add(ulong guildId, ulong userID, string reason)
        {
            string xmlPath = $"Servers//{guildId}//Warns.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            xmldoc.Element("Warns").Add(new XElement("Warn", new XAttribute("UserID", userID), new XElement("Reason", reason)));

            xmldoc.Save(xmlPath);
        }

        public static void Remove(ulong guildId, int index)
        {
            string xmlPath = $"Servers//{guildId}//Warns.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            try
            {
                xmldoc.Element("Warns").Elements().Skip(index).Take(1).Remove();
                xmldoc.Save(xmlPath);
            }
            catch { }
        }

        public static void Clear(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Warns.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            foreach (var entry in xmldoc.Element("Warns").Elements())
            {
                entry.Remove();
            }

            xmldoc.Save(xmlPath);
        }

        public static void Clear(ulong guildId, ulong userID)
        {
            string xmlPath = $"Servers//{guildId}//Warns.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            foreach (var entry in xmldoc.Element("Warns").Elements().Where(e => e.Attribute("UserID").Value == userID.ToString()))
            {
                entry.Remove();
            }

            xmldoc.Save(xmlPath);
        }

        public static List<string> List(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Warns.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            List<string> warns = new List<string>();
            int index = 0;
            foreach (var entry in xmldoc.Element("Warns").Elements())
            {
                string userID = entry.Attribute("UserID").Value;
                string reason = entry.Element("Reason").Value;
                warns.Add( $"#{index}: {userID} {reason}" );
                index++;
            }

            return warns;
        }

    }
}
