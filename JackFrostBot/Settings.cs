using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FrostBot
{
    public class Config
    {
        public class Botsettings
        {
            public Botsettings() { }
            public Botsettings(string token, List<Server> servers, string activity, int activityType, int status) 
            {
                Token = token;
                Servers = servers;
                Activity = activity;
                ActivityType = activityType;
                Status = status;
            }
            public string Token { get; set; } = "";
            public List<Server> Servers { get; set; } = new List<Server>();
            public string Activity { get; set; } = "";
            public int ActivityType { get; set; } = 0;
            public int Status { get; set; } = 0;

            // Load settings.yml as settings object if it exists, or create a new one
            public static Botsettings Load()
            {
                Botsettings settings = new Botsettings();

                if (File.Exists(Program.ymlPath))
                {
                    Console.WriteLine("Reading settings.yml");
                    var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                    settings = deserializer.Deserialize<Botsettings>(File.ReadAllText(Program.ymlPath));
                }
                else
                {
                    Console.WriteLine("Creating settings.yml");
                    Directory.CreateDirectory(Path.GetDirectoryName(Program.ymlPath));
                    Save(settings);
                }

                return settings;
            }

            public static void Save(Botsettings settings)
            {
                // Save settings object to new settings.yml
                Console.WriteLine("Saving settings.yml");
                var serializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                File.WriteAllText(Program.ymlPath, serializer.Serialize(settings));
            }

            public static Server SelectedServer(ulong guildId)
            {
                return Program.settings.Servers.First(x => x.Id.Equals(guildId));
            }
        }

        public class Server
        {
            public string Name { get; set; } = "";
            public ulong Id { get; set; } = 0;
            public string BotNickname { get; set; } = "";
            public char Prefix { get; set; } = '?';
            public List<Warn> Warns { get; set; } = new List<Warn>();
            public Channels Channels { get; set; } = new Channels();
            public List<Command> Commands { get; set; } = new List<Command>();
            public List<Role> Roles { get; set; } = new List<Role>();
            public List<Currency> Currency { get; set; } = new List<Currency>();
            public List<string> WordFilter { get; set; } = new List<string>();

            // Chat
            public bool PublishAnnouncements { get; set; } = false;
            public bool SendTypingMsg { get; set; } = false;
            public bool AutoMarkov { get; set; } = false;
            public bool BotChannelMarkovOnly { get; set; } = false;
            public int MarkovFreq { get; set; } = 0;
            // Moderation
            public int MuteLevel { get; set; } = 2;
            public int KickLevel { get; set; } = 3;
            public int BanLevel { get; set; } = 4;
            public bool AutoDeleteDupes { get; set; } = false;
            public int DuplicateFreq { get; set; } = 2;
            public int MaxDuplicates { get; set; } = 2;
            public bool AutoDelete1CharMsgs { get; set; } = false;
            public bool WarnOnAutoDelete { get; set; } = false;
            public bool EnableWordFilter { get; set; } = true;
            public bool WarnOnFilter { get; set; } = true;
            // Embed
            public ServerStrings Strings { get; set; } = new ServerStrings();
            public string EmbedIconURL { get; set; } = "";
        }

        public class Warn
        {
            public ulong UserID { get; set; } = 0;
            public string UserName { get; set; } = "";
            public string Reason { get; set; } = "";
            public string CreatedAt { get; set; } = "";
            public string CreatedBy { get; set; } = "";
        }

        public class Channels
        {
            public ulong General { get; set; } = 0;
            public ulong BotSandbox { get; set; } = 0;
            public ulong BotLogs { get; set; } = 0;
            public ulong Pins { get; set; } = 0;
            public ulong Verification { get; set; } = 0;
        }

        public class Command
        {
            public string Name { get; set; } = "";
            public bool Enabled { get; set; } = true;
            public bool ModeratorsOnly { get; set; } = true;
            public bool BotChannelOnly { get; set; } = true;
            public bool IsSlashCmd { get; set; } = true;
        }

        public class Role
        {
            public string Name { get; set; } = "";
            public ulong Id { get; set; } = 0;
            public bool Moderator { get; set; } = false;
            public bool CanPin { get; set; } = false;
            public bool CanCreateColors { get; set; } = false;
            public bool CanCreateRoles { get; set; } = false;
            public bool Joinable { get; set; } = false;
            public bool IsVerifiedRole { get; set; } = false;
            public bool IsLurkerRole { get; set; } = false;
        }

        public class Currency
        {
            public string UserName { get; set; } = "";
            public ulong UserID { get; set; } = 0;
            public int Amount { get; set; } = 0;
        }

        public class ServerStrings
        {
            public string CurrencyName { get; set; } = "Macca";
            public string BotIconURL { get; set; } = "https://images-ext-2.discordapp.net/external/FnvGxO_YkmQbgUsGsSDUE1QlW9VNEBNBZoPy7294rHc/https/i.imgur.com/5I5Vos8.png";
            public string WikiURL { get; set; } = "https://amicitia.miraheze.org";
            public string MuteMsg { get; set; } = "You will not be able to speak until a moderator unmutes you.";
            public string UnmuteMsg { get; set; } = "Be sure to follow the rules and have fun.";
            public string LockMsg { get; set; } = "Please bring this discussion elsewhere until things cool down.";
            public string UnlockMsg { get; set; } = "Be sure to follow the rules and have fun.";
            public string NoPermissionMsg { get; set; } = "You do not have permission to use this command.";
            public string WelcomeMessage { get; set; } = "Read the rules and make yourself at home!";

        }
    }


}