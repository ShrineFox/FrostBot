using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;
using Discord.WebSocket;

namespace JackFrostBot
{
    class Xml
    {
        public static void Setup(List<SocketGuild> guilds)
        {
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
                    var servers = xmldoc.Element("Root").Elements("Servers");

                    foreach (var server in servers)
                    {
                        if (server.Element("Server").Attribute("Id").Value == guild.Id.ToString())
                        {
                            
                            serverFound = true;
                            //Update server name for config file and folder name.
                            if (server.Element("Server").Attribute("Name").Value != guild.Name)
                            {
                                //Ensure that this works vvvvvv
                                Directory.Move($"Servers//{server.Element("Server").Attribute("Name").Value}", $"Servers//{guild.Name}");
                                server.Element("Server").Attribute("Name").Value = guild.Name;
                            }
                            
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
            Console.WriteLine($"ServerInfo.xml was not found.\nCreating new ServerInfo.xml...");
            string xmlPath = "Servers//Servers.xml";
            XElement srcTree = new XElement("Root");

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
            //Get server name from Servers.xml using guild id, use it as folderName
            //TODO: UPDATE XML ON SERVER RENAME
            string folderName = " ";

            //Create all xml doc objects
            List<XElement> xElementList = new List<XElement> { Channels(guild, folderName), Roles(guild, folderName), Verification(guild, folderName), Filters(guild, folderName), BotOptions(guild, folderName) };

            //Serialize each xml doc object to xml file in appropriate folder
            foreach (XElement xElement in xElementList)
            {
                string xmlPath = $"Servers//{folderName}//{nameof(Channels)}.xml";
                if (!Directory.Exists(Path.GetDirectoryName(xmlPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(xmlPath));
                }
                File.Create(xmlPath);
            }
            
        }

        public static XElement Channels(SocketGuild guild, string folderName)
        {
            XElement channels = new XElement("Root");
            return channels;
        }

        public static XElement Roles(SocketGuild guild, string folderName)
        {
            XElement roles = new XElement("Root");
            return roles;
        }

        public static XElement Verification(SocketGuild guild, string folderName)
        {
            XElement verification = new XElement("Root");
            return verification;
        }

        public static XElement Filters(SocketGuild guild, string folderName)
        {
            XElement filters = new XElement("Root");
            return filters;
        }

        public static XElement BotOptions(SocketGuild guild, string folderName)
        {
            XElement botOptions = new XElement("Root");
            return botOptions;
        }

    }
}
