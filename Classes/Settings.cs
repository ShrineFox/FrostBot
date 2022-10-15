using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ShrineFox.IO;
using Discord.WebSocket;
using Discord;

namespace FrostBot
{
    public class Settings
    {
        public string Token { get; set; } = "";

        public List<Server> Servers { get; set; } = new List<Server>();
        public UserStatus Status { get; set; } = UserStatus.Online;
        public ActivityType Activity { get; set; } = ActivityType.CustomStatus;
        public string Game { get; set; } = "hee-ho!";

        public void Load()
        {
            // Add program directory to json path if not already present
            if (!Program.JsonPath.Contains(Exe.Directory()))
                Program.JsonPath = Path.Combine(Exe.Directory(), Program.JsonPath);

            if (File.Exists(Program.JsonPath))
            {
                Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Program.JsonPath));
                this.Token = settings.Token;
                if (!String.IsNullOrEmpty(Token))
                    Output.Log("Loaded token!", ConsoleColor.Green);
                else
                    Output.Log($"Error: No token found! Please set one in: {Program.JsonPath}", ConsoleColor.Red);
                this.Servers = settings.Servers;
            }
            else
            {
                Output.Log($"Warning: Could not find settings file, creating one at: {Program.JsonPath}", ConsoleColor.Yellow);
                Save();
            }
        }

        public void Save()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(this, 
                new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(Program.JsonPath, json);
            Output.Log("Saved settings!", ConsoleColor.Green);
        }
    }

    public class Server
    {
        public string ServerName { get; set; } = "";
        public string ServerID { get; set; } = "";
        public Channel BotLogChannel { get; set; } = new Channel();
        public Channel ModMailChannel { get; set; } = new Channel();
        public Channel PinChannel { get; set; } = new Channel();
        public Channel AutoMarkovChannel { get; set; } = new Channel();
        public int AutoMarkovRate { get; set; } = 0;
        public int AutoMarkovLength { get; set; } = 10;
        public WarnSettings WarnOptions { get; set; } = new WarnSettings();
        public List<OptInRole> OptInRoles { get; set; } = new List<OptInRole>();
        public List<Warn> Warns { get; set; } = new List<Warn>();
    }

    public class Channel
    {
        public string Name { get; set; } = "";
        public string ID { get; set; } = "";
    }

    public class OptInRole
    {
        public string RoleName { get; set; } = "";
        public string RoleID { get; set; } = "";
    }

    public class WarnSettings
    {
        public int TimeOutAfter { get; set; } = 2;
        public int TimeOutLength { get; set; } = 60;
        public int KickAfter { get; set; } = 3;
        public int BanAfter { get; set; } = 4;
    }

    public class Warn
    {
        public string Username { get; set; } = "";
        public string UserID { get; set; } = "";
        public string Date { get; set; } = "";
        public string Reason { get; set; } = "";
        public string ModeratorName { get; set; } = "";
    }
}