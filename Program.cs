using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using ShrineFox.IO;
using FrostBot;
using System.Linq;

namespace FrostBot
{
    public class Program
    {
        private readonly IServiceProvider _services;
        public static Settings settings;
        public static string settingsPath = "_settings.json";

        private readonly DiscordSocketConfig _socketConfig = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true,
        };

        public Program()
        {
            SetupAppearance();

            _services = new ServiceCollection()
                .AddSingleton(_socketConfig)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .BuildServiceProvider();
        }

        static void Main(string[] args)
            => new Program().RunAsync()
                .GetAwaiter()
                .GetResult();

        public async Task RunAsync()
        {
            settings = new Settings();
            settings.Load(settingsPath);

            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            client.Ready += ReadyAsync;

            // Here we can initialize the service that will register and execute our commands
            await _services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();

            // Bot token can be provided from the Configuration object we set up earlier
            await client.LoginAsync(TokenType.Bot, settings.Token);
            await client.StartAsync();

            // Never quit the program until manually forced to.
            await Task.Delay(Timeout.Infinite);
        }

        private async Task ReadyAsync()
        {
            UpdateServerList();
            await Task.CompletedTask;
        }

        private void UpdateServerList()
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();

            foreach (var server in client.Guilds)
            {
                if (settings.Servers.Any(x => x.ServerID.Equals(server.Id.ToString())))
                {
                    // Update server name
                    settings.Servers.First(x => x.ServerID.Equals(server.Id.ToString())).ServerName = server.Name;
                    Output.Log($"Server \"{server.Name}\" settings updated.", ConsoleColor.DarkGray);
                }
                else
                {
                    // Add server to settings
                    settings.Servers.Add(new Server() { ServerID = server.Id.ToString(), ServerName = server.Name });
                    Output.Log($"Server \"{server.Name}\" added to settings.", ConsoleColor.Green);
                }
            }

            foreach (var server in settings.Servers)
            {
                if (!client.Guilds.Any(x => x.Id.ToString().Equals(server.ServerID)))
                {
                    settings.Servers.Remove(server);
                    Output.Log($"Server \"{server.ServerName}\" not found, removed from settings.", ConsoleColor.Yellow);
                }
            }
            Output.Log("Done updating server settings.", ConsoleColor.DarkGray);
            settings.Save(settingsPath);
        }

        private async Task LogAsync(LogMessage message)
        {
            Output.Log(message.ToString(), ConsoleColor.DarkGray);
            await Task.CompletedTask;
        }

        public static bool IsDebug()
        {
            #if DEBUG
                return true;
            #else
                return false;
            #endif
        }

        private void SetupAppearance()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Output.Logging = true;
            Output.LogPath = "FrostBot_Log.txt";
            if (IsDebug())
                Output.VerboseLogging = true;
        }
    }
}