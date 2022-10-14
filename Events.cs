using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ShrineFox.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostBot
{
    public partial class Program
    {
        private async Task ReadyAsync()
        {
            UpdateServerList();
            await Task.CompletedTask;
        }

        private async Task LogAsync(LogMessage message)
        {
            Output.Log(message.ToString(), ConsoleColor.DarkGray);
            await Task.CompletedTask;
        }

        private async Task MsgReceivedAsync(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            string text = GetMessageContents(message);

            // Log incoming message
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(user.Guild.Id.ToString()));
            string path = Path.Combine(Path.Combine(Path.Combine(Exe.Directory(), "Servers"), serverSettings.ServerID), "MsgReceived.txt");
            Output.Log($"{user.DisplayName} ({user.Id}) in \"{user.Guild.Name}\" #{message.Channel}: {text}", ConsoleColor.White, path);

            // Send auto-markov if in designated channel
            if (!user.IsBot)
            {
                Processing.FeedMarkovString(serverSettings, text);
                if (serverSettings.AutoMarkovChannel.ID == message.Channel.Id.ToString())
                    await message.Channel.SendMessageAsync(Processing.CreateMarkovString(serverSettings));
            }

            await Task.CompletedTask;
        }

        private Task ChannelUpdated(SocketChannel channel, SocketChannel updatedChannel)
        {
            return Task.CompletedTask;
        }

        private Task ChannelDestroyed(SocketChannel channel)
        {
            return Task.CompletedTask;
        }

        private Task ChannelCreated(SocketChannel channel)
        {
            return Task.CompletedTask;
        }

        private Task ThreadUpdated(Cacheable<SocketThreadChannel, ulong> thread, SocketThreadChannel updpatedThread)
        {
            return Task.CompletedTask;
        }

        private Task ThreadDeleted(Cacheable<SocketThreadChannel, ulong> thread)
        {
            return Task.CompletedTask;
        }

        private Task ThreadCreated(SocketThreadChannel thread)
        {
            return Task.CompletedTask;
        }

        private Task MessageCommandExecuted(SocketMessageCommand msgcmd)
        {
            return Task.CompletedTask;
        }

        private Task UsercommandExecuted(SocketUserCommand usrcmd)
        {
            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SocketSlashCommand slashcmd)
        {
            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> msg, SocketMessage updatedMsg, ISocketMessageChannel channel)
        {
            return Task.CompletedTask;
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            var client = _services.GetRequiredService<DiscordSocketClient>();
            // Get server the message is from
            SocketGuild server = null;
            foreach (var guild in client.Guilds)
                foreach (var chan in guild.Channels)
                    if (chan.Id == channel.Id)
                        server = guild;

            // Get message contents if cached
            IMessageChannel chnl = await channel.GetOrDownloadAsync();
            IMessage msg = await message.GetOrDownloadAsync();
            string deletedText = GetMessageContents(msg);

            // Log message deleted
            Output.Log($"Message deleted in \"{server.Name}\" #{chnl.Name}:\n{deletedText}", ConsoleColor.DarkRed);
            await Processing.SendToBotLogs(server, $":x: **Message Deleted** in #{chnl.Name}:\n{deletedText}", Color.Red, msg.Author);
            
            await Task.CompletedTask;
        }

        private Task LeftGuild(SocketGuild guild)
        {
            Output.Log($"Left server: \"{guild.Name}\"", ConsoleColor.Red);
            UpdateServerList();
            return Task.CompletedTask;
        }

        private Task JoinedGuild(SocketGuild guild)
        {
            Output.Log($"Joined server: \"{guild.Name}\"", ConsoleColor.Green);
            UpdateServerList();
            return Task.CompletedTask;
        }

        private async Task UserLeft(SocketGuild guild, SocketUser user)
        {
            Output.Log($"{user.Username}#{user.Discriminator} ({user.Id}) has left the server: \"{guild.Name}\"", ConsoleColor.DarkYellow);
            await Processing.SendToBotLogs(guild, $":door: {user.Username}#{user.Discriminator} ({user.Id}) has left.", Color.Red);

            await Task.CompletedTask;
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            Output.Log($"{user.Username} ({user.Id}) has joined the server: \"{user.Guild.Name}\"", ConsoleColor.DarkCyan);
            await Processing.SendToBotLogs(user.Guild, $":door: {user.Username}#{user.Discriminator} ({user.Id}) has joined.", Color.Green);

            await Task.CompletedTask;
        }

        private Task ReactionRemoved(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            Output.Log($"{reaction.User} removed their reaction to \"{reaction.Message}\" in #{reaction.Channel} with:\n{JsonConvert.SerializeObject(reaction.Emote)}", ConsoleColor.DarkRed);
            return Task.CompletedTask;
        }

        private Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            Output.Log($"{reaction.User} reacted to \"{reaction.Message}\" in #{reaction.Channel} with:\n{JsonConvert.SerializeObject(reaction.Emote)}", ConsoleColor.DarkGreen);
            return Task.CompletedTask;
        }

        private Task InteractionCreated(SocketInteraction interaction)
        {
            //Output.Log($"{interaction.User.Username} interacted with \"{interaction.Id}\" in #{interaction.Channel}:\n{JsonConvert.SerializeObject(interaction.Data)}", ConsoleColor.Cyan);
            return Task.CompletedTask;
        }

        private Task GuildAvailable(SocketGuild guild)
        {
            Output.Log($"Guild Available: \"{guild.Name}\"", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        private Task GuildUnavailable(SocketGuild guild)
        {
            Output.Log($"Guild Unavailable: \"{guild.Name}\"", ConsoleColor.Red);
            return Task.CompletedTask;
        }

        private Task AppCmdUpdated(SocketApplicationCommand appcmd)
        {
            Output.Log($"App Command Updated in \"{appcmd.Guild.Name}\": {appcmd.Name} ({appcmd.Description})", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        private Task AppCmdDeleted(SocketApplicationCommand appcmd)
        {
            Output.Log($"App Command Deleted in \"{appcmd.Guild.Name}\": {appcmd.Name} ({appcmd.Description})", ConsoleColor.Red);
            return Task.CompletedTask;
        }

        private Task AppCmdCreated(SocketApplicationCommand appcmd)
        {
            Output.Log($"App Command Created in \"{appcmd.Guild.Name}\": {appcmd.Name} ({appcmd.Description})", ConsoleColor.Green);
            return Task.CompletedTask;
        }
    }
}
