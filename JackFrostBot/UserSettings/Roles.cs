using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    public class Roles
    {
        public static bool ModeratorPermissions(string elementName, IGuildUser user)
        {
            string xmlPath = $"Servers//{user.Guild.Id}//Roles.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            //Verify that user is moderator if no specific elementName passed
            //if (elementName == "")

            XElement role = null;

            foreach (var roleId in user.RoleIds)
                if (xmldoc.Element("Roles").Element("ModeratorRoles").Elements().Any(r => r.Attribute("Id").Value == roleId.ToString()))
                    role = xmldoc.Element("Roles").Element("ModeratorRoles").Elements().Single(r => r.Attribute("Id").Value == roleId.ToString());

            if (elementName == "" && role != null)
            {
                return true;
            }
            else if (role != null)
            {
                return Convert.ToBoolean(role.Element(elementName).Value);
            }
                
            return false;
        }

        public static List<ulong> ModeratorRoleIDs(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Roles.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            var roles = xmldoc.Element("Roles").Element("ModeratorRoles").Elements();

            List<ulong> roleIds = new List<ulong>();
            foreach (var role in roles)
                roleIds.Add(Convert.ToUInt64(role.FirstAttribute.Value));

            return roleIds;
        }

        public static ulong LurkerRoleID(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Roles.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var roleId = Convert.ToUInt64(xmldoc.Element("Roles").Element("LurkerRole").Element("Role").Attribute("Id").Value);

            return roleId;
        }

        public static bool LurkerRoleEnabled(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Roles.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            return Convert.ToBoolean(xmldoc.Element("Roles").Element("LurkerRole").Element("Enabled").Value);
        }

        public static bool LurkerRoleAutoRemove(ulong guildId)
        {
            string xmlPath = $"Servers//{guildId}//Roles.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);

            return Convert.ToBoolean(xmldoc.Element("Roles").Element("LurkerRole").Element("RemoveAfterOneMessage").Value);
        }

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
