using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using ShrineFox.IO;
using System;
using System.Collections.Generic;
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

        private Task MsgReceivedAsync(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            string text = GetMessageContents(message);

            Output.Log($"{user.DisplayName} ({user.Id}) in \"{user.Guild.Name}\" #{message.Channel}: {text}");

            return Task.CompletedTask;
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
            /*
            IMessageChannel chnl = await channel.GetOrDownloadAsync();
            IMessage msg = await message.GetOrDownloadAsync();
            Output.Log($"Message deleted in #{chnl}:\n{GetMessageContents(msg)}", ConsoleColor.DarkRed);
            */
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

        private Task UserLeft(SocketGuild guild, SocketUser user)
        {
            Output.Log($"{user.Username} ({user.Id}) has left the server: \"{guild.Name}\"", ConsoleColor.DarkYellow);
            return Task.CompletedTask;
        }

        private Task UserJoined(SocketGuildUser user)
        {
            Output.Log($"{user.Username} ({user.Id}) has joined the server: \"{user.Guild.Name}\"", ConsoleColor.DarkCyan);
            return Task.CompletedTask;
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
