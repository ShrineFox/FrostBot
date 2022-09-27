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
            //client.Ready += Ready;
            //commands.Log += LogCommands;
            // message edited
            // message deleted

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private void LoadSettings(string[] args)
        {
            settings = new Settings();

            // Get token from commandline args
            if (args != null && args[0].Length == 70)
                settings.Token = args[0];
        }

        private void SetupAppearance()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Output.Logging = true;
            Output.LogPath = "FrostBot_Log.txt";
            #if DEBUG
                Output.VerboseLogging = true;
            #endif
        }

        private async Task ConnectBot()
        {
            // Start up Bot
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                GatewayIntents = GatewayIntents.All,
                #if DEBUG
                    //LogLevel = LogSeverity.Debug,
                #endif
                AlwaysDownloadUsers = true
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
                    Output.Log("Failed to connect. Please set bot token as first argument.", ConsoleColor.Red);
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

        private Task Ready()
        {
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
            LoadSettings(new string[] { settings.Token });

            // Get message content and channel
            var message = messageParam as SocketUserMessage;

            // Stop processing if it's a system message
            if (message == null)
                return;
            // Stop processing if user is a bot
            if (message.Author.IsBot)
                return;

            //Process message...
            await LogMessage(message);
            //await Phpbb.ForumUpdate(message, channel);

            // Track where the prefix ends and the command begins
            int argPos = 0;
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
            Output.Log(msg.Message, ConsoleColor.DarkGray);
            if (msg.Exception != null)
                LogException(msg.Exception);

            return Task.CompletedTask;
        }

        private void LogException(Exception exception)
        {
            if (exception.Message.Contains("WebSocket connection was closed"))
                Output.Log("Connection was lost", ConsoleColor.Red);
            else
                Output.Log(exception.Message, ConsoleColor.Red);
        }

        public async Task LogMessage(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            string nickname = user.DisplayName;
            var guild = user.Guild;

            //Create txt if it doesn't exist
            string path = Path.Combine(Exe.Directory(), $"Servers//{guild.Id}//Log.txt");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Write timestamp, message, channel and username/nickname
            string timeStamp = DateTime.Now.ToString("hh: mm");
            string logLine = $"<{timeStamp}> {nickname} ({user.Username}#{user.Discriminator}) in \"{guild.Name}\" #{message.Channel}: {message}";
            // Include attachment URL
            if (message.Attachments.Count > 0)
                logLine += "\n" + message.Attachments.FirstOrDefault().Url;

            Console.WriteLine(logLine);
            File.AppendAllText(path, logLine + "\n");

            await Task.CompletedTask;
            return;
        }
    }
}
