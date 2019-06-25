using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class Info
    {
        public static List<string> Keywords(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Info.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            List<string> keywords = new List<string>();
            foreach (var entry in xmldoc.Element("Info").Elements())
                keywords.Add(entry.Attribute("Keywords").Value);
            return keywords;
        }
        public static string About(ulong guildId, string keyword)
        {
            string xmlPath = $"Servers//{guildId}//Info.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            string about = "";

            foreach (var element in xmldoc.Element("Info").Elements())
            {
                List<string> keywords = element.Attribute("Keywords").Value.Split(',').ToList();
                foreach (string word in keywords)
                {
                    if (Regex.IsMatch(word, $"\\b{keyword}\\b"))
                        about = element.Element("About").Value;
                }
            }

            return about;
        }

        public static string Links(ulong guildId, string keyword)
        {
            string xmlPath = $"Servers//{guildId}//Info.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            string links = "";

            foreach (var entry in xmldoc.Element("Info").Elements().Where(e => Regex.IsMatch(e.Attribute("Keywords").Value, $"\\b{keyword}\\b")))
                links = entry.Element("Links").Value;

            return links;
        }

    }
}
