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

namespace Bot
{
    public class Program
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        public static bool slow = false;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            client.Log += Log;
            client.Ready += Ready;

            string token = Setup.Token();

            services = new ServiceCollection()
                .BuildServiceProvider();
            await InstallCommands();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            //Add the string of the game currently being played if game.txt exists
            if (File.Exists("game.txt"))
                await client.SetGameAsync(File.ReadAllText("game.txt"));
            else
                File.CreateText("game.txt").Close();

            client.UserJoined += UserJoin;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task Ready()
        {
            var guilds = client.Guilds.ToList();
            foreach (var guild in guilds)
            {
                if (!Directory.Exists(guild.Id.ToString()))
                    Directory.CreateDirectory(guild.Id.ToString());
                string setupName = $"{guild.Id.ToString()}\\setup.ini";
                if (!File.Exists(setupName))
                {
                    Setup.CreateIni(setupName);
                    await guild.DefaultChannel.SendMessageAsync("Please fill out **setup.ini** in order to complete server setup and start using commands! " +
                        "\nYou can find this file in a new folder at the directory where this bot lives. In order to get channel IDs, enable Discord developer mode. " +
                        "\nYou can get role IDs by pinging a role with the escape character ``\\`` in front of it. Example: ``\\@role``");
                }
            }
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            var channel = (SocketGuildChannel)message.Channel;

            // Don't process the command if it was a System Message
            if (message == null) return;

            //Process message...
            Processing.LogSentMessage(message);
            await Processing.NewArrivalsCheck(message);
            await Processing.MemesCheck(message);
            await Processing.DuplicateMsgCheck(message, channel);
            await Processing.FilterCheck(message, channel);

            //Remove lurker role if member has one
            if (Setup.AssignLurkerRoles(channel.Guild.Id))
            {
                var user = (IGuildUser)message.Author;
                var lurkRole = user.Guild.GetRole(Setup.LurkerRoleId(user.Guild.Id));
                if (lurkRole != null)
                    await user.RemoveRoleAsync(lurkRole);
            }
            
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('?', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
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
            if (Setup.AssignLurkerRoles(user.Guild.Id))
            {
                //Give the new user the Lurker role if enabled
                var lurkRole = (IRole)user.Guild.GetRole(Setup.LurkerRoleId(user.Guild.Id));
                var newUser = (IGuildUser)user;
                if (lurkRole != null)
                await newUser.AddRoleAsync(lurkRole);
            }
            
            int warnLevel = Moderation.WarnLevel(user);
            var defaultChannel = (SocketTextChannel)user.Guild.GetChannel(Setup.DefaultChannelId(user.Guild.Id));

            if (warnLevel >= Setup.MuteLevel(user.Guild.Id))
            {
                await defaultChannel.SendMessageAsync($"**A user with multiple warns has rejoined: {user.Mention}.** Automatically muting...");
                Moderation.Mute(client.CurrentUser.Username, defaultChannel, user);
            }
            else
            {
                await defaultChannel.SendMessageAsync($"**Welcome to the server, {user.Mention}!** Be sure to read <#{Setup.WelcomeChannelId(user.Guild.Id)}> and enjoy your stay, hee-ho!");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            File.AppendAllText("Log.txt", msg + Environment.NewLine);
            return Task.CompletedTask;
        }
    }
}