using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class Filters
    {
        public static bool Enabled(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Filters.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            return Convert.ToBoolean(xmldoc.Element("Filters").Element("Enabled").Value);
        }

        public static List<string> List(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Filters.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var filters = xmldoc.Element("Filters").Elements().Where(e => e.Name == "Filter");

            List<string> filterList = new List<string>();
            foreach (var filter in filters)
                filterList.Add(filter.Value);
            return filterList;
        }

        public static bool TermCausesWarn(string term, ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Filters.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var filters = xmldoc.Element("Filters").Elements().Where(e => e.Name == "Filter");

            if (filters.Any(f => Convert.ToBoolean(f.Attribute("AutoWarnUser").Value) == true))
                return true;
            else
                return false;
        }

        //why is this here
        public static List<ulong> OptInRoles(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Roles.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var roles = xmldoc.Element("Roles").Element("Opt-InRoles").Elements().Where(e => e.Name == "Role");

            List<ulong> roleIds = new List<ulong>();
            foreach (var role in roles)
            {
                roleIds.Add(Convert.ToUInt64(role.FirstAttribute.Value));
            }
            return roleIds;
        }
    }
}
