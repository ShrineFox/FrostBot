using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JackFrostBot.UserSettings
{
    //Commands.xml

    //Gets the settings of a command from Commands.xml
    public class Commands
    {
        public static string description { get; set; } = "";
        public static bool enabled { get; set; } = false;
        public static bool moderatorsOnly { get; set; } = true;
        public static bool botChannelOnly { get; set; } = true;

        public static List<Tuple<string, string>> parameters = new List<Tuple<string, string>>();

        public static void Get(string commandName, ulong guildId)
        {
            description = "";
            enabled = false;
            moderatorsOnly = true;
            botChannelOnly = true;
            parameters = new List<Tuple<string, string>>();

            string xmlPath = $"Servers//{guildId}//Commands.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var command = xmldoc.Element("Commands").Elements().Single(c => c.FirstAttribute.Value == commandName);
            
            if (command != null)
            {
                description = command.Element("Description").Value;
                enabled = Convert.ToBoolean(command.Element("Enabled").Value);
                moderatorsOnly = Convert.ToBoolean(command.Element("ModeratorsOnly").Value);
                botChannelOnly = Convert.ToBoolean(command.Element("BotChannelOnly").Value);
                foreach (var element in command.Element("Parameters").Elements())
                {
                    parameters.Add(new Tuple<string, string>(element.Name.ToString(), element.Value));
                }
            }
        }

        public static List<string> List(ulong guildId, bool isModerator)
        {
            string xmlPath = $"Servers//{guildId}//Commands.xml";
            XDocument xmldoc = XDocument.Load(xmlPath);
            var commands = xmldoc.Element("Commands").Elements();

            List<string> commandList = new List<string>();
            foreach (var element in commands)
            {
                if (Convert.ToBoolean(element.Element("Enabled").Value) && (isModerator || !Convert.ToBoolean(element.Element("ModeratorsOnly").Value)))
                {
                    string commandName = element.Attribute("Name").Value;
                    char commandPrefix = UserSettings.BotOptions.CommandPrefix(guildId);
                    string commandUsage = $"**{commandPrefix}{commandName}** ";

                    Get(commandName, guildId);
                    foreach (var tuple in parameters)
                    {
                        commandUsage += $"<{tuple.Item2}> ";
                    }

                    commandUsage += $" - {element.Element("Description").Value}";

                    commandList.Add(commandUsage);
                }
            }

            return commandList;
        }
    }
}
