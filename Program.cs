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
using System.Threading;
using Discord.Interactions;

namespace FrostBot
{
    public class Program
    {
        public static CommandService commands;
        public static DiscordSocketClient client;
        private IServiceProvider services;
        public static InteractionService interactions;
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
            //commands.Log += LogCommands;
            client.SlashCommandExecuted += SlashCommandHandler;
            //client.MessageCommandExecuted += MessageCommandHandler;
            //client.UserCommandExecuted += UserCommandHandler;
            // message edited
            // message deleted

            // Block this task until the program is closed.
            await Task.Delay(Timeout.Infinite);
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            ulong guildId = command.GuildId ?? 0;
            string location = "";
            if (guildId != 0)
            {
                SocketGuild guild = client.GetGuild(guildId);
                location = $"\"{guild.Name}\" #{command.Channel}";
            }
            else if (command.IsDMInteraction)
                location = "DMs";
            
            Output.Log(LogString((IGuildUser)command.User, location, 
                $"used Slash Command {command.CommandName}: {command.Data}"), ConsoleColor.Blue);
            
            await command.RespondAsync($"You executed {command.Data.Name}");
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
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
            interactions = new InteractionService(client.Rest);
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

        private async Task Ready()
        {
            await RegisterSlashCmds();
            return;
        }

        private async Task RegisterSlashCmds()
        {
            foreach (var guild in client.Guilds)
            {
                var guildCommand = new SlashCommandBuilder();

                // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
                guildCommand.WithName("say");

                // Descriptions can have a max length of 100.
                guildCommand.WithDescription("This is my first guild slash command!");

                // Let's do our global command
                //var globalCommand = new SlashCommandBuilder();
                //globalCommand.WithName("first-global-command");
                //globalCommand.WithDescription("This is my first global slash command");

                try
                {
                    // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                    await guild.CreateApplicationCommandAsync(guildCommand.Build());

                    // With global commands we don't need the guild.
                    //await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
                    // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                    // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.

                    Output.Log("Slash Commands have been registered.", ConsoleColor.Gray);
                }
                catch (HttpException exception)
                {
                    // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                    //var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);

                    // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                    Output.Log(exception.Message);
                }
            }
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

            //Process message...
            await LogMessage(message);
            //await Phpbb.ForumUpdate(message, channel);

            // Stop processing if user is a bot
            if (message.Author.IsBot)
                return;

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
                Output.Log("Connection Lost", ConsoleColor.Red);
            else
                Output.Log(exception.Message, ConsoleColor.Red);
        }

        public async Task LogMessage(SocketMessage message)
        {
            IGuildUser user = (IGuildUser)message.Author;
            string logLine = LogString(user, $"\"{user.Guild.Name}\" #{message.Channel}", $": {message}");
            // Include attachment URL
            if (message.Attachments.Count > 0)
            {
                logLine += "\n\tAttachments:";
                foreach (var attachment in message.Attachments)
                    logLine += $"\n\t\t{attachment.Url}";
            }
            Output.Log(logLine);

            await Task.CompletedTask;
            return;
        }

        public static string LogString(IGuildUser user, string location, string message)
        {
            return $"{user.DisplayName} ({user.Username}#{user.Discriminator}) in {location} {message}";
        }
    }
}
