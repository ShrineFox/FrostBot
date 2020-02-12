using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace JackFrostBot
{
    public class Xml
    {
        public static CommandService _commands;
        public static ulong exampleId = 540749842620612681;

        public static void Setup(List<SocketGuild> guilds, CommandService commands)
        {
            _commands = commands;

            Console.WriteLine("Checking bot configuration.");
            if (!Directory.Exists("Servers"))
                Directory.CreateDirectory("Servers");

            foreach (SocketGuild guild in guilds)
            {
                Console.WriteLine($"Checking configuration for {guild.Name}...");
                string xmlPath = "Servers//Servers.xml";

                if (File.Exists(xmlPath))
                {
                    bool serverFound = false;
                    XDocument xmldoc = XDocument.Load(xmlPath);
                    var servers = xmldoc.Element("Servers");

                    foreach (var server in servers.Elements())
                    {
                        //Check if guild Id is found in server list
                        if (server.Attribute("Id").Value == guild.Id.ToString() && Directory.Exists($"Servers//{guild.Id}//"))
                        {
                            serverFound = true;
                            //Update server name for config file
                            server.Element("Name").Value = guild.Name;
                            xmldoc.Save(xmlPath);

                            Console.WriteLine($"{guild.Name} found and updated successfully.");
                        }
                    }
                    
                    //Create new configs for specified server.
                    if (!serverFound)
                        SetupServer(guild);
                }
                else
                {
                    //Create config at root of Servers folder, then configure each server.
                    SetupServerConfig(guilds);
                }
            }
        }

        private static void SetupServerConfig(List<SocketGuild> guilds)
        {
            Console.WriteLine($"Servers.xml was not found.\nCreating new Servers.xml...");
            string xmlPath = "Servers//Servers.xml";
            XElement srcTree = new XElement("Servers");

            //Add server to root of document
            foreach (SocketGuild guild in guilds)
                srcTree.Add(new XElement("Server", new XAttribute("Id", guild.Id), new XElement("Name", guild.Name)));
            
            //Save Server Configuration document to xml file
            srcTree.Save(xmlPath);
            //Set up configuration for each registered server
            foreach (SocketGuild guild in guilds)
                SetupServer(guild);
        }

        private static void SetupServer(SocketGuild guild)
        {
            Console.WriteLine($"{guild.Name} config was not found.\nCreating new config files for {guild.Name}.");
            SetupServerConfig(guild);
            Console.WriteLine($"Finished creating config files for {guild.Name}.");
        }

        private static void SetupServerConfig(SocketGuild guild)
        {
            //Create all xml doc objects
            List<XElement> xElementList = new List<XElement> { Channels(guild), Commands(guild), Roles(guild), Verification(guild), Filters(guild), BotOptions(guild), Info(guild), Warns(guild), Currency(guild), Invites(guild) };

            //Serialize each xml doc object to xml file in appropriate folder
            foreach (XElement xElement in xElementList)
            {
                string xmlPath = $"Servers//{guild.Id}//{xElement.Name}.xml";
                if (!Directory.Exists(Path.GetDirectoryName(xmlPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(xmlPath));
                }
                xElement.Save(xmlPath);
            }
            
        }

        public static XElement Channels(SocketGuild guild)
        {
            //Important
            XElement channels = new XElement("Channels");
            channels.Add(new XComment("The following settings are important to fill out."));
            channels.Add(new XElement("Welcome", new XComment("Channel where welcome pings are posted when users join."),
                new XElement("Channel", new XAttribute("Id", exampleId), new XAttribute("Name", "example-general-channel"), new XElement("EnableWelcomeMessageOnJoin", "true"))));
            channels.Add(new XElement("BotLogs", new XComment("Channel where detailed logs of the bot usage by moderators will be kept."),
                new XElement("Channel", new XAttribute("Id", exampleId), new XAttribute("Name", "example-bot-logs-channel"), new XElement("EnableBotLogs", "true"))));
            channels.Add(new XElement("BotChannel", new XComment("Channel where users are encouraged to out bot commands."),
                new XElement("Channel", new XAttribute("Id", exampleId), new XAttribute("Name", "example-bot-channel"))));
            //Optional
            channels.Add(new XComment("The following settings are optional."));
            channels.Add(new XElement("Media-Only", new XComment("Channels where users can upload files, images, videos, links to other websites etc."), 
                new XElement("Channel", new XAttribute("Id", exampleId), new XAttribute("Name", "example-media-channel"))));
            channels.Add(new XElement("Pins", new XComment("Channel where users can save messages using the pin command."),
                new XElement("Channel", new XAttribute("Id", exampleId), new XAttribute("Name", "example-pins-channel"))));

            return channels;
        }

        public static XElement Commands(SocketGuild guild)
        {
            XElement commands = new XElement("Commands");
            foreach(var cmdModule in _commands.Modules.Where(m => m.Parent == null))
            {
                commands.Add(new XComment("Manually edit true/false values below to change command usage permissions"));
                foreach (var cmd in cmdModule.Commands)
                {
                    var paramList = cmd.Parameters.ToList();
                    List<XElement> paramElements = new List<XElement>();
                    foreach (var parameter in paramList)
                        paramElements.Add(new XElement(parameter.Type.ToString(), parameter.Name));
                    commands.Add(new XElement("Command", new XAttribute("Name", cmd.Name), new XElement("Description", cmd.Summary), new XElement("Enabled", "true"), new XElement("ModeratorsOnly", "true"), new XElement("BotChannelOnly", "false"), new XElement("Parameters", paramElements)));
                }
            }
            return commands;
        }

        public static bool CommandAllowed(string commandName, ICommandContext context)
        {
            ulong botChannelId = UserSettings.Channels.BotChannelId(context.Guild.Id);
            bool moderatorRole = Moderation.IsModerator((IGuildUser)context.Message.Author);

            UserSettings.Commands.Get(commandName, context.Guild.Id);

            if (UserSettings.Commands.enabled && !context.User.IsBot)
                if ((UserSettings.Commands.botChannelOnly && (context.Channel.Id == botChannelId)) || !UserSettings.Commands.botChannelOnly)
                    if (!UserSettings.Commands.moderatorsOnly || (UserSettings.Commands.moderatorsOnly && moderatorRole))
                    return true;
            return false;
        }

        public static XElement Roles(SocketGuild guild)
        {
            XElement roles = new XElement("Roles");
            //Moderator Role
            roles.Add(new XElement("ModeratorRoles", new XComment("Roles that are allowed to use the bot's moderation commands.")));
            var modRoles = roles.Element("ModeratorRoles");
            foreach (var role in guild.Roles.Where(r => r.Permissions.Administrator == true))
            {
                modRoles.Add(new XElement("Role", new XAttribute("Id", role.Id), new XAttribute("Name", role.Name), 
                    new XElement("CanWarnUsers", "true"), new XElement("CanMuteUsers", "true"), new XElement("CanBypassFilter", "true"), new XElement("CanUsePinCommand", "true")));
            }

            //Default Role
            roles.Add(new XElement("LurkerRole", new XElement("Description", "Role that users are given after joining.")));
            XElement defaultRole = roles.Element("LurkerRole");
            defaultRole.Add(new XComment("This can be used to purge users who have never sent a message by using the purgelurkers command."));
            defaultRole.Add(new XElement("Role", new XAttribute("Id", exampleId), new XAttribute("Name", "Default Role Name")));
            defaultRole.Add(new XElement("Enabled", "true"));
            defaultRole.Add(new XElement("RemoveAfterOneMessage", "true"));

            //Opt-In Role
            roles.Add(new XElement("Opt-InRoles", new XElement("Description", "Roles that users can grant themselves at any time.")));
            XElement optInRoles = roles.Element("Opt-InRoles");
            optInRoles.Add(new XComment("Manually add roles below to enable users to grant themselves the role."), new XElement("Role", new XAttribute("Id", exampleId), new XAttribute("Name", "Example Role")));

            return roles;
        }

        public static XElement Verification(SocketGuild guild)
        {
            XElement verification = new XElement("Verification");
            verification.Add(new XComment( "Requires new users to correctly post a specific message in the 'verification' channel before granting them a role."));
            verification.Add(new XElement("Enabled", "false"));
            verification.Add(new XElement("VerificationChannel", new XAttribute("Id", exampleId), new XAttribute("Name", "Example Verification Channel"), 
                new XElement("VerificationMessage", new XAttribute("MessageText", "I have read and agreed to the rules."))));
            verification.Add(new XComment("Role given to users who have successfully verified themselves."));
            verification.Add(new XElement("MemberRole", new XAttribute("Id", exampleId), "Member Role Name"));

            return verification;
        }

        public static XElement Filters(SocketGuild guild)
        {
            XElement filters = new XElement("Filters");
            filters.Add(new XComment("Add words or phrases to moderate below."));
            filters.Add(new XElement("Enabled", "false"));
            filters.Add(new XElement("Filter", new XAttribute("AutoWarnUser", "true"), "quagga"));

            return filters;
        }

        public static XElement BotOptions(SocketGuild guild)
        {
            XElement botOptions = new XElement("BotOptions");
            botOptions.Add(new XElement("CommandPrefix", "?"));
            botOptions.Add(new XElement("WarnLevels", 
                new XComment("Maximum number of warns a user can recieve before automatically being penalized."),
                new XElement("Mute", "2"),
                new XElement("Kick", "3"),
                new XElement("Ban", "4")));
            botOptions.Add(new XElement("MessageLimits",
                new XComment("Messages will be automatically deleted if they fail to meet the following requirements."),
                new XElement("MaximumDuplicates", "3"),
                new XElement("MinimumLength", "0"),
                new XElement("MinimumAlphanumericCharacters", "0"),
                new XElement("DuplicateFrequencyThreshold", "1"),
                new XElement("AutoWarnDuplicates", "true")
                ));
            botOptions.Add(new XElement("Strings",
                new XComment("Automated messages that will be used to give the bot some personal flair."),
                new XElement("LockMessage", "Please do not bring this discussion to other channels."),
                new XElement("UnlockMessage", "Be mindful of the rules and don't forget to have fun."),
                new XElement("MuteMessage", "You are unable to type until a moderator unmutes you."),
                new XElement("UnmuteMessage", "You are able to type again."),
                new XElement("NoPermissionMessage", "You don't have permission to use this command."),
                new XElement("BotIconURL", "https://images-ext-2.discordapp.net/external/FnvGxO_YkmQbgUsGsSDUE1QlW9VNEBNBZoPy7294rHc/https/i.imgur.com/5I5Vos8.png"),
                new XElement("WelcomeMessage", "Be sure to read the rules and  enjoy your stay!"),
                new XElement("CurrencyName", "Coins")
                ));
            
            botOptions.Add(new XElement("Markov",
                new XComment("The following settings determine how the markov command behaves."),
                new XElement("AutoMarkov", "false"),
                new XElement("BotChannelOnly", "false"),
                new XElement("AutoMarkovFrequency", 100),
                new XElement("MinimumLength", 0)));

            return botOptions;
        }

        public static XElement Info(SocketGuild guild)
        {
            XElement info = new XElement("Info");
            info.Add(new XElement("Entry", new XAttribute("Keywords", "example, example2"), new XElement("About", "**JackFrostBot** is a Discord bot that uses **Discord.Net** created by _ShrineFox_."), new XElement("Links", "[Github Repo](https://github.com/Amicitia/JackFrost-Bot), [Release Page](https://github.com/Amicitia/JackFrost-Bot/releases)")));
            return info;
        }

        private static XElement Warns(SocketGuild guild)
        {
            XElement info = new XElement("Warns");
            info.Add(new XElement("Warn", new XAttribute("UserID", exampleId), new XElement("Reason", "example reason for warn")));
            return info;
        }

        private static XElement Currency(SocketGuild guild)
        {
            XElement info = new XElement("Currency");
            info.Add(new XElement("User", new XAttribute("UserID", exampleId), new XElement("Amount", "0")));
            return info;
        }

        private static XElement Invites(SocketGuild guild)
        {
            XElement invites = new XElement("Invites");
            invites.Add(new XElement("Invite", new XAttribute("User", ""), new XElement("Code", exampleId)));
            return invites;
        }

    }
}
