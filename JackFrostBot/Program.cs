using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Web;
using FrostBot;
using static FrostBot.Config;

namespace FrostBot
{
    public class Program
    {
        public static bool active;
        private CommandService commands;
        public static DiscordSocketClient client;
        private IServiceProvider services;
        public static string ymlPath = "";
        public static Botsettings settings;

        public static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // Set path to config file
            settings = new Botsettings();
            
            if (HttpContext.Current != null)
                ymlPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data//settings.yml");
            else
                ymlPath = ".//App_Data//settings.yml";


            // Get settings from config file (token etc.)
            settings = Botsettings.Load();

            // Start up Bot
            client = new DiscordSocketClient();
            commands = new CommandService();
            services = new ServiceCollection()
                .BuildServiceProvider();
            await InstallCommands();

            // Attempt to connect
            try
            {
                if (settings.Token != "")
                {
                    await client.LoginAsync(TokenType.Bot, settings.Token);
                    await client.StartAsync();
                }
                else
                    Console.WriteLine("Failed to connect. Please set bot token in settings.yml.");
            }
            catch
            {
                Console.WriteLine("Failed to connect. Invalid token?");
            }

            // Tasks
            //client.Log += Log;
            client.Ready += Ready;
            //client.Disconnected += Disconnected;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public async Task Ready()
        {
            Console.WriteLine("Connected");
            active = true;

            // Get updated list of servers
            Console.WriteLine("Getting servers...");
            foreach (var guild in client.Guilds)
                if (settings.Servers == null || !settings.Servers.Any(x => x.Id.Equals(guild.Id)))
                    AddServer(guild);

            // Update settings.yml
            Botsettings.Save(settings);

            // Set game activity and status from config
            /*if (settings.Activity != "")
                SetStatus(new Game(settings.Activity, (ActivityType)settings.ActivityType), (UserStatus)settings.Status);*/

            Console.WriteLine("Ready");
        }

        private void AddServer(SocketGuild guild)
        {
            // Add list of commands (only enabled for moderators by default)
            List<Command> cmds = new List<Command>();
            foreach (var cmdModule in commands.Modules.Where(m => m.Parent == null))
                foreach (var cmd in cmdModule.Commands)
                    cmds.Add(new Command { Name = cmd.Name });

            // Add list of moderator roles (derived from administrator permissions)
            List<Role> roles = new List<Role>();
            foreach (var role in guild.Roles.Where(x => x.Permissions.Administrator))
                roles.Add(new Role { Name = role.Name, Id = role.Id, Moderator = true, CanCreateColors = true, CanCreateRoles = true, CanPin = true, IsVerifiedRole = true });

            settings.Servers.Add(new Server { Id = guild.Id, Name = guild.Name, Commands = cmds, Roles = roles });
        }

        public static void SetStatus(Game activity, UserStatus status)
        {
            Console.WriteLine("Setting activity and status...");
            client.SetActivityAsync(activity);
            client.SetStatusAsync(status);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            Console.WriteLine("Message Received");
            // Get message content and channel
            var message = messageParam as SocketUserMessage;
            var channel = (SocketGuildChannel)message.Channel;

            // Stop processing if it's a system message or author is a bot
            if (message == null || message.Author.IsBot) return;

            // Ensure we have latest settings
            settings = Botsettings.Load();

            // Get settings for the server the message is in
            var selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            // Set status to offline and stop executing if deactivate button has been clicked
            if (!active)
                Close();

            //Process message...
            await Processing.LogSentMessage(message);
            await Processing.DuplicateMsgCheck(message, channel);
            //await Processing.MsgLengthCheck(message, channel);
            //await Processing.VerificationCheck(message);
            //await Processing.MediaOnlyCheck(message);
            await Processing.FilterCheck(message, channel);

            // Track where the prefix ends and the command begins
            int argPos = 0;

            // If the message doesn't start with the server's command prefix or a bot mention...
            if (!message.HasCharPrefix(Convert.ToChar(settings.Servers.First(x => x.Id.Equals(channel.Guild.Id)).Prefix), ref argPos)
                && !message.HasMentionPrefix(client.CurrentUser, ref argPos))
            {
                // Send markov message if permissible by settings
                if (selectedServer.AutoMarkov && !message.Author.IsBot && selectedServer.Channels.BotLogs == channel.Guild.Id)
                    await Processing.Markov(message, channel, selectedServer);
                // Stop processing message
                return;
            }

            // Create Command Context
            var context = new CommandContext(client, message);

            // Execute the command (result indicates if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            // Send error message unless command is unknown
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        public static void Close()
        {
            // Set status to offline
            SetStatus(null, 0);
            // Set that bot is inactive
            active = false;
            // End program execution
            Environment.Exit(0);
        }
    }
}
