using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JackFrostBot;
using System.Timers;
using System.Windows.Forms;

namespace Bot
{
    public class Program
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            client.Log += Log;
            client.Ready += Ready;
            client.JoinedGuild += BotJoined;
            client.ReactionAdded += ReactionAdded;

            string token = Setup.Token();

            services = new ServiceCollection()
                .BuildServiceProvider();
            await InstallCommands();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            client.UserJoined += UserJoin;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var msgId = arg3.MessageId;
            var channel = (ITextChannel)arg2;
            var msg = await channel.GetMessageAsync(Convert.ToUInt64(msgId));

            var context = new CommandContext(client, (IUserMessage)msg);
            //TODO: Don't duplicate previously pinned messages, use reaction count
            if (arg3.Emote.Name == "📌" && Xml.CommandAllowed("pin", context))
            {
                Console.WriteLine($"Pin reaction found");
                try
                {
                    var pinChannel = await channel.Guild.GetTextChannelAsync(JackFrostBot.UserSettings.Channels.PinsChannelId(channel.Guild.Id));

                    var embed = Embeds.Pin(channel, msg);
                    await pinChannel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                }
                catch
                {
                }
            }
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        public async Task Ready()
        {
            var guilds = client.Guilds.ToList();

            //Set up bot if first time running
            Xml.Setup(client.Guilds.ToList(), commands);

            //Open Form
            JackFrostBot.FrostForm form = new JackFrostBot.FrostForm(client);
            await Task.Run(() => { form.ShowDialog(); });
        }

        public async Task BotJoined(SocketGuild guild)
        {
            //Set up bot if first time running
            Xml.Setup(client.Guilds.ToList(), commands);
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            var channel = (SocketGuildChannel)message.Channel;

            // Don't process the command if it was a System Message
            if (message == null) return;

            //Process message...
            await Processing.LogSentMessage(message);
            await Processing.DuplicateMsgCheck(message, channel);
            await Processing.MsgLengthCheck(message, channel);
            await Processing.VerificationCheck(message);
            await Processing.MediaOnlyCheck(message);
            //await Processing.FilterCheck(message, channel);

            //Remove lurker role if member has one
            if (JackFrostBot.UserSettings.Roles.LurkerRoleAutoRemove(channel.Guild.Id))
            {
                try
                {
                    var user = (IGuildUser)message.Author;
                    var youlong = JackFrostBot.UserSettings.Roles.LurkerRoleID(user.Guild.Id);
                    var lurkRole = user.Guild.GetRole(youlong);
                    if (lurkRole != null)
                        await user.RemoveRoleAsync(lurkRole);
                }
                catch { }
            }

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix(JackFrostBot.UserSettings.BotOptions.CommandPrefix(channel.Guild.Id), ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            {
                if (JackFrostBot.UserSettings.BotOptions.AutoMarkov(channel.Guild.Id) && !message.Author.IsBot)
                    await Processing.Markov(message.Content, channel, JackFrostBot.UserSettings.BotOptions.AutoMarkovFrequency(channel.Guild.Id));
                else
                    return;
            }

            // Create a Command Context
            var context = new CommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
            {
                if (result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }

        }

        public async Task UserJoin(SocketGuildUser user)
        {
            if (Convert.ToBoolean(JackFrostBot.UserSettings.Roles.LurkerRoleEnabled(user.Guild.Id)))
            {
                //Give the new user the Lurker role if enabled
                var lurkRole = (IRole)user.Guild.GetRole(JackFrostBot.UserSettings.Roles.LurkerRoleID(user.Guild.Id));
                var newUser = (IGuildUser)user;
                if (lurkRole != null)
                    await newUser.AddRoleAsync(lurkRole);
            }

            int warnLevel = Moderation.WarnLevel(user);
            var defaultChannel = (SocketTextChannel)user.Guild.GetChannel(JackFrostBot.UserSettings.Channels.WelcomeChannelId(user.Guild.Id));

            if (warnLevel >= JackFrostBot.UserSettings.BotOptions.MuteLevel(user.Guild.Id))
            {
                await defaultChannel.SendMessageAsync($"**A user with multiple warns has rejoined: {user.Mention}.** Automatically muting...");
                Moderation.Mute(client.CurrentUser.Username, defaultChannel, user);
            }
            else if (JackFrostBot.UserSettings.Channels.WelcomeOnJoin(user.Guild.Id))
            {
                await defaultChannel.SendMessageAsync($"**Welcome to the server, {user.Mention}!** {JackFrostBot.UserSettings.BotOptions.GetString("WelcomeMessage", user.Guild.Id)}");
            }
        }

        private Task Log(LogMessage msg)
        {
            if (!msg.ToString().Contains("handler is blocking the gateway task"))
            {
                Console.WriteLine(msg.ToString());
                File.AppendAllText("Log.txt", msg + Environment.NewLine);
            }
            return Task.CompletedTask;
        }
    }
}