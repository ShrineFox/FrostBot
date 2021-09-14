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
using static FrostBot.Components;
using static FrostBot.SlashCommands;

namespace FrostBot
{
    public class Program
    {
        public static CommandService commands;
        public static DiscordSocketClient client;
        private IServiceProvider services;
        public static string ymlPath = "";
        public static Botsettings settings;

        public static void Main(string[] args)
            => new Program().MainAsync(args).GetAwaiter().GetResult();

        public async Task MainAsync(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            // Set path to config file
            settings = new Botsettings();
            
            if (HttpContext.Current != null)
                ymlPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data//settings.yml");
            else
                ymlPath = ".//App_Data//settings.yml";

            // Get settings from config file (token etc.)
            Botsettings.Load();

            // Get token from commandline args
            if (args != null && args.Length > 0 && args[0].Length == 59)
                settings.Token = args[0];

            // Start up Bot
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                AlwaysAcknowledgeInteractions = false,
                GatewayIntents = GatewayIntents.All,
                #if DEBUG
                LogLevel = LogSeverity.Debug,
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
                if (settings.Token != "")
                {
                    await client.LoginAsync(TokenType.Bot, settings.Token);
                    await client.StartAsync();
                }
                else
                {
                    Processing.LogConsoleText("Failed to connect. Please set bot token in settings.yml.");
                    Console.ReadKey();
                    Close();
                }
            }
            catch
            {
                Processing.LogConsoleText("Failed to connect. Invalid token?");
                Console.ReadKey();
                Close();
            }

            // Tasks
            client.Log += Log;
            client.Ready += Ready;
            client.UserJoined += UserJoin;
            client.ReactionAdded += ReactionAdded;
            client.InteractionCreated += InteractionCreated;
            commands.Log += LogCommands;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task LogCommands(LogMessage arg)
        {
            Processing.LogDebugMessage($"Message source: {arg.Source} ||| Severity: {arg.Severity} ||| Source message: {arg.Message} ||| Exception(if applicable): {arg.Exception}");
            return Task.CompletedTask;
        }

        private async Task InteractionCreated(SocketInteraction interaction)
        {
            try
            {
                switch (interaction)
                {
                    // Slash commands
                    case SocketSlashCommand commandInteraction:
                        await HandleSlashCmd(commandInteraction);
                        break;

                    // Button clicks/selection dropdowns
                    case SocketMessageComponent componentInteraction:
                        await HandleComponentInteraction(componentInteraction);
                        break;

                    // Unused or Unknown/Unsupported
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Processing.LogDebugMessage(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private async Task HandleComponentInteraction(SocketMessageComponent interaction)
        {
            MessageProperties msgProps = Components.GetProperties(interaction);

            // Ignore if response is empty
            if (msgProps == new MessageProperties())
                return;

            await interaction.UpdateAsync(x =>
                {
                    x.Embed = msgProps.Embed;
                    x.Components = msgProps.Components;
                });
        }

        private async Task HandleSlashCmd(SocketSlashCommand interaction)
{
            // Checking command name
            if (interaction.Data.Name == "ping")
            {
                // Respond to interaction with message.
                // You can also use "ephemeral" so that only the original user of the interaction sees the message
                await interaction.RespondAsync($"Pong!", ephemeral: true);

                // Also you can followup with a additional messages, which also can be "ephemeral"
                await interaction.FollowupAsync($"PongPong!", ephemeral: true);
            }
        }

        public async Task UserJoin(SocketGuildUser user)
        {
            var selectedServer = Botsettings.GetServer(user.Guild.Id);
            // Give the new user the Lurker role if enabled
            if (selectedServer.Roles.Any(x => x.IsLurkerRole))
            {
                var lurkRole = (IRole)user.Guild.GetRole(selectedServer.Roles.First(x => x.IsLurkerRole).Id);
                var newUser = (IGuildUser)user;
                if (lurkRole != null)
                    await newUser.AddRoleAsync(lurkRole);
            }

            // Send custom welcome message
            // OR mute user automatically if their warn level is equal to or higher than the mute level
            int warnLevel = Moderation.WarnLevel(user);
            var defaultChannel = (SocketTextChannel)user.Guild.GetChannel(selectedServer.Channels.General);

            if (warnLevel >= selectedServer.MuteLevel)
            {
                await defaultChannel.SendMessageAsync($"**A user with multiple warns has rejoined: {user.Mention}.** Automatically muting...");
                Moderation.Mute(client.CurrentUser.Username, defaultChannel, user);
            }
            else
                await defaultChannel.SendMessageAsync($"**Welcome to the server, {user.Mention}!** {selectedServer.Strings.WelcomeMessage}");
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> usrMsg, Cacheable<IMessageChannel, ulong> msgChannel, SocketReaction reaction)
        {
            var amount = 1;
            var msgId = reaction.MessageId;
            //var channel = (ITextChannel)msgChannel;

            // Pin message using "pin" emoji
            /* 
            if (arg1.Id == 640802444233670676)
            {
                int originalAmount = 0;
                foreach (var pair in JackFrostBot.UserSettings.Currency.Get(channel.Guild.Id))
                    if (pair.Item1 == arg3.UserId.ToString())
                        originalAmount = pair.Item2;
                JackFrostBot.UserSettings.Currency.Add(channel.Guild.Id, arg3.UserId, amount);

                var botlog = await channel.Guild.GetTextChannelAsync(JackFrostBot.UserSettings.Channels.BotLogsId(channel.Guild.Id));
                var botchannel = await channel.Guild.GetTextChannelAsync(JackFrostBot.UserSettings.Channels.BotChannelId(channel.Guild.Id));
                Embed embed = Embeds.Earn(arg3.User.ToString(), amount, channel.Guild.Id);
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                await botchannel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }*/

            return;
        }

        public async Task Ready()
        {
            Program.settings.Active = true;

            // Get updated list of servers (and publish news)
            foreach (var guild in client.Guilds)
            {
                if (settings.Servers == null || !settings.Servers.Any(x => x.Id.Equals(guild.Id)))
                    AddServer(guild);
                await Processing.PublishNews(guild);
            }

            // Update settings.yml
            Botsettings.Save();

            // Set game activity and status from config
            /*if (settings.Activity != "")
                SetStatus(new Game(settings.Activity, (ActivityType)settings.ActivityType), (UserStatus)settings.Status);*/

            Processing.LogDebugMessage("Ready");
        }

        private void AddServer(SocketGuild guild)
        {
            Processing.LogDebugMessage($"Adding server {guild.Name} to config...");
            // Add list of commands (only enabled for moderators by default unless debug)
            List<Command> cmds = new List<Command>();
            foreach (var cmdModule in commands.Modules.Where(m => m.Parent == null))
                foreach (var cmd in cmdModule.Commands)
                {
                    if (cmd.Name == "setup")
                        cmds.Add(new Command { Name = cmd.Name, BotChannelOnly = false });
                    else
                    {
                        #if DEBUG
                        cmds.Add(new Command { Name = cmd.Name, ModeratorsOnly = false, BotChannelOnly = false });
                        #else
                        cmds.Add(new Command { Name = cmd.Name });
                        #endif
                    }
                }

            // Add list of moderator roles (derived from administrator permissions)
            List<Role> roles = new List<Role>();
            foreach (var role in guild.Roles.Where(x => x.Permissions.Administrator))
                roles.Add(new Role { Name = role.Name, Id = role.Id, Moderator = true, CanCreateColors = true, CanCreateRoles = true, CanPin = true });

            settings.Servers.Add(new Server { Id = guild.Id, Name = guild.Name, Commands = cmds, Roles = roles });
        }

        public static void SetStatus(Game activity, UserStatus status)
        {
            Processing.LogDebugMessage("Setting activity and status...");
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
            // Ensure we have latest settings
            Botsettings.Load();
            // Set status to offline and stop executing if bot has been deativated remotely
            if (!settings.Active)
                Close();

            // Get message content and channel
            var message = messageParam as SocketUserMessage;
            var channel = (SocketGuildChannel)message.Channel;
            var user = (IGuildUser)message.Author;
            
            // Get settings for the server the message is in
            var selectedServer = Botsettings.GetServer(channel.Guild.Id);

            // Stop processing if it's a system message or author is a bot
            if (message == null || message.Author.IsBot) return;

            // Remove lurker role if member has one
            foreach (var roleId in user.RoleIds)
                if (selectedServer.Roles.Any(x => x.Equals(roleId) && x.IsLurkerRole))
                    await user.RemoveRoleAsync(roleId);

            //Process message...
            await Processing.LogSentMessage(message);
            await Processing.DuplicateMsgCheck(message, channel);
            await Processing.FilterCheck(message, (ITextChannel)channel);
            await Processing.UnarchiveThreads(channel.Guild);

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

        private Task Log(LogMessage msg)
        {
            if (!msg.ToString().Contains("handler is blocking the gateway task"))
                Processing.LogConsoleText(msg.Message);
            return Task.CompletedTask;
        }

        public static void Close()
        {
            // Save changes
            Botsettings.Save();
            // Set status to offline
            SetStatus(null, 0);
            // End program execution
            Environment.Exit(0);
        }
    }
}
