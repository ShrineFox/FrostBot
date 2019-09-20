using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class Currency
    {
        public static List<Tuple<string, int>> Get(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Currency.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            List<Tuple<string, int>> users = new List<Tuple<string, int>>();
            foreach (var entry in xmldoc.Element("Currency").Elements())
            {
                users.Add(new Tuple<string, int>(entry.Attribute("UserID").Value, Convert.ToInt32(entry.Element("Amount").Value)));
            }
                
            return users;
        }

        public static void Add(ulong guildId, ulong userID, int amount)
        {
            string xmlPath = $"Servers//{guildId}//Currency.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            int currentAmount = 0;
            bool entryExists = false;
            foreach (var element in xmldoc.Element("Currency").Elements())
            {
                if (element.FirstAttribute.Value == userID.ToString())
                {
                    currentAmount = Convert.ToInt32(element.Element("Amount").Value);
                    entryExists = true;
                }
            }

            if (currentAmount == 0 && !entryExists)
                xmldoc.Element("Currency").Add(new XElement("User", new XAttribute("UserID", userID), new XElement("Amount", amount)));
            else
                xmldoc.Element("Currency").Elements().Where(x => x.Attribute("UserID").Value.Equals(userID.ToString())).FirstOrDefault().Element("Amount").Value = (currentAmount + amount).ToString();

            xmldoc.Save(xmlPath);
        }

        public static void Remove(ulong guildId, ulong userID, int amount)
        {
            string xmlPath = $"Servers//{guildId}//Currency.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            int currentAmount = 0;
            bool entryExists = false;
            foreach (var element in xmldoc.Element("Currency").Elements())
            {
                if (element.FirstAttribute.Value == userID.ToString())
                {
                    currentAmount = Convert.ToInt32(element.Element("Amount").Value);
                    entryExists = true;
                }
            }

            if (currentAmount == 0 && !entryExists)
                xmldoc.Element("Currency").Add(new XElement("User", new XAttribute("UserID", userID), new XElement("Amount", (0 - amount))));
            else
                xmldoc.Element("Currency").Elements().Where(x => x.Attribute("UserID").Value.Equals(userID.ToString())).FirstOrDefault().Element("Amount").Value = (currentAmount - amount).ToString();

            xmldoc.Save(xmlPath);
        }

    }
}
