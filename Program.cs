﻿using Discord;
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
using Newtonsoft.Json;

namespace FrostBot
{
    public class Program
    {
        private readonly IServiceProvider _services;
        public static Settings settings;
        public static string JsonPath = "_settings.json";

        private readonly DiscordSocketConfig _socketConfig = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All,
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
            => new Program().RunAsync(args)
                .GetAwaiter()
                .GetResult();

        public async Task RunAsync(string[] args)
        {
            settings = new Settings();
            if (!IsDebug() || args.Length == 0)
                settings.Load();
            else
            {
                // Get token from commandline argument instead of loading settings file
                settings.Token = args[0];
                Output.Log("Got token from commandline args!", ConsoleColor.Green);
            }

            var client = _services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            client.Ready += ReadyAsync;
            client.MessageReceived += MsgReceivedAsync;

            // Here we can initialize the service that will register and execute our commands
            await _services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();

            // Bot token can be provided from the Configuration object we set up earlier
            await client.LoginAsync(TokenType.Bot, settings.Token);
            await client.StartAsync();

            // Never quit the program until manually forced to.
            await Task.Delay(Timeout.Infinite);
        }

        private Task MsgReceivedAsync(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            string text = message.Content;
            foreach (var embed in message.Embeds)
                text += $"\nEmbed: {JsonConvert.SerializeObject(embed)}";
            foreach (var attachment in message.Attachments)
                text += $"\nAttachment: {JsonConvert.SerializeObject(attachment)}";
            foreach (var sticker in message.Stickers)
                text += $"\nSticker: {JsonConvert.SerializeObject(sticker)}";

            Output.Log($"{user.DisplayName} ({user.Id}) in {user.Guild.Name} #{message.Channel}: {text}");

            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            UpdateServerList();
            if (IsDebug())
            {
                settings.Servers.First(x => x.ServerID.Equals("866656658774949898")).PinChannel = new Channel { ID = "866656775133593610", Name = "Pins" };
                settings.Servers.First(x => x.ServerID.Equals("866656658774949898")).ModMailChannel = new Channel { ID = "1017200068480204880", Name = "ModMail" };
            }
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
            settings.Save();
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