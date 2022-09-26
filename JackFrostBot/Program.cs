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
using Microsoft.Extensions.DependencyInjection;
using Discord.Net;
using ShrineFox.IO;

namespace FrostBot
{
    public class Program
    {
        public static CommandService commands;
        public static DiscordSocketClient client;
        private IServiceProvider services;
        public static Settings settings;

        public static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            LoadSettings(args);
            SetupAppearance();

            await ConnectBot();

            // Tasks
            client.Log += Log;
            client.Ready += Ready;
            commands.Log += LogCommands;
            // message edited
            // message deleted

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task ConnectBot()
        {
            // Start up Bot
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                //GatewayIntents = GatewayIntents.All,
                #if DEBUG
                    LogLevel = LogSeverity.Debug
                #endif
                //AlwaysDownloadUsers = true
            });
            commands = new CommandService();
            services = new ServiceCollection()
                .BuildServiceProvider();
            await InstallCommands();

            // Attempt to connect
            try
            {
                if (!string.IsNullOrEmpty(settings.Token))
                {
                    await client.LoginAsync(TokenType.Bot, settings.Token);
                    await client.StartAsync();
                }
                else
                {
                    Output.Log("Failed to connect. Please set bot token in settings.yml.", ConsoleColor.Red);
                    Console.ReadKey();
                    return;
                }
            }
            catch
            {
                Output.Log("Failed to connect. Invalid token?", ConsoleColor.Red);
                Console.ReadKey();
                return;
            }
        }

        private void LoadSettings(string[] args)
        {
            // Get settings from config file (token etc.)
            //Botsettings.Load();

            settings = new Settings();

            // Get token from commandline args
            if (args != null && args[0].Length == 70)
                settings.Token = args[0];
        }

        private void SetupAppearance()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            #if DEBUG
                Output.VerboseLogging = true;
            #endif
        }

        private Task LogCommands(LogMessage arg)
        {
            Output.Log($"Message source: {arg.Source} ||| Severity: {arg.Severity} ||| Source message: {arg.Message} ||| Exception(if applicable): {arg.Exception}");
            return Task.CompletedTask;
        }

        private Task Ready()
        {
            Output.Log("Ready", ConsoleColor.Green);
            return Task.CompletedTask;
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
            // Ensure up-to-date settings are loaded
            LoadSettings(new string[] { });

            // Get message content and channel
            var message = messageParam as SocketUserMessage;

            // Stop processing if it's a system message
            if (message == null) return;

            var channel = (SocketGuildChannel)message.Channel;
            var user = (IGuildUser)message.Author;
            
            //Process message...
            await LogMessage(message);
            await Phpbb.ForumUpdate(message, channel);

            // Track where the prefix ends and the command begins
            int argPos = 0;

            // Stop processing if user is a bot
            if (message.Author.IsBot)
                return;

            // Stop processing if no command character or user mention prefix
            if (!message.HasCharPrefix('?', ref argPos) && !message.HasMentionPrefix(client.CurrentUser, ref argPos))
                return;

            // Create Command Context
            var context = new CommandContext(client, message);

            // Execute the command (result indicates if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            // Send error message unless command is unknown
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private Task Log(LogMessage msg)
        {
            Output.Log(msg.Message);

            return Task.CompletedTask;
        }

        public async Task LogMessage(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            string nickname = user.Nickname;
            var guild = user.Guild;

            //Create txt if it doesn't exist
            string path = Path.Combine(Exe.Directory(), $"Servers//{guild.Id}//Log.txt");
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.Create(path);
            }

            // Write timestamp, message, channel and username/nickname
            string timeStamp = DateTime.Now.ToString("hh: mm");
            string logLine = "";
            if (nickname != "")
                logLine = $"<{timeStamp}> {nickname} ({user.Username}) in {guild.Name} #{message.Channel}: {message}";
            else
                logLine = $"<{timeStamp}> {nickname} ({user.Username}) in {guild.Name} #{message.Channel}: {message}";
            // Include attachment URL
            if (message.Attachments.Count > 0)
                logLine = logLine + message.Attachments.FirstOrDefault().Url;

            File.AppendAllText(path, logLine + "\n");

            await Task.CompletedTask;
            return;
        }
    }
}
