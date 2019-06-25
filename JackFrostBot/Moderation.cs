using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot;
using JackFrostBot;
using System.Text.RegularExpressions;
using System.Threading;

namespace JackFrostBot
{
    public class Moderation
    {
        public static bool IsModerator(IGuildUser user)
        {
            if (UserSettings.Roles.ModeratorPermissions("", user))
                return true;
            return false;
        }

        public static bool IsPublicChannel(SocketGuildChannel channel)
        {
            if (Setup.PrivateChannelIds(channel.Guild).Any(c => c.Equals(channel.Id)))
                    return false;

            return true;
        }

        public static int WarnLevel(SocketGuildUser user)
        {
            //Measure # of warns the user now has
            int WarnLevel = 0;
            foreach(Tuple<string, string> tuple in UserSettings.Warns.Get(user.Guild.Id))
                if (tuple.Item1 == user.Id.ToString())
                    WarnLevel++;
            return WarnLevel;
        }

        public static async void Warn(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            reason = $"({user.Username}#{user.Discriminator}) {reason}";
            //Warn User
            var embed = Embeds.Warn(user, reason);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the warn in bot-logs
            embed = Embeds.LogWarn(moderator, channel, user, reason);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(user.Guild.Id));
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Write the userid and reason in warns.xml
            UserSettings.Warns.Add(user.Guild.Id, user.Id, reason);

            //Measure # of warns the user now has
            int warns = WarnLevel(user);

            //Mute, kick or ban a user if they've accumulated too many warns
            int muteLevel = UserSettings.BotOptions.MuteLevel(user.Guild.Id);
            int kickLevel = UserSettings.BotOptions.KickLevel(user.Guild.Id);
            int banLevel = UserSettings.BotOptions.BanLevel(user.Guild.Id);

            if (warns > 0)
            {
                await channel.SendMessageAsync($"{user.Username} has been warned {warns} times.");
                if (warns >= banLevel ) 
                    Ban(user.Guild.CurrentUser.Username, channel, user,
                                "User was automatically banned for accumulating too many warnings.");
                else if (warns >= kickLevel)
                    Kick(user.Guild.CurrentUser.Username, channel, user,
                                "User was automatically kicked for accumulating too many warnings.");
                else if (warns >= muteLevel)
                    Mute(user.Guild.CurrentUser.Username, channel, user);
            }
        }

        public static async void ClearWarns(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
        {
            //Announce clearing of warns in both channel and bot-logs
            var embed = Embeds.ClearWarns(moderator, channel, user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(user.Guild.Id));
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Clear warns from this user
            UserSettings.Warns.Clear(channel.Guild.Id, user.Id);
        }

        public static async void ClearWarn(SocketGuildUser moderator, ITextChannel channel, int index, SocketGuildUser user)
        {
            //Get warn userId and description
            Tuple<string, string> tuple = UserSettings.Warns.Get(channel.Guild.Id, index);
            string removedWarn = tuple.Item1 + " " + tuple.Item2;

            //Announce clearing of warns in both channel and bot-logs
            var embed = Embeds.ClearWarn(moderator, removedWarn);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(moderator.Guild.Id));

            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Write new warns list to file
            UserSettings.Warns.Remove(channel.Guild.Id, index);
            
        }

        public static async void Mute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            var guild = user.Guild;
            //Mute the specified user in each text channel if it's not a bot
            if (!user.IsBot)
            {
                foreach (SocketTextChannel chan in user.Guild.TextChannels)
                {
                    //Don't mess with channel permissions if nonmembers can't speak there anyway
                    if (IsPublicChannel(chan))
                    {
                        try
                        {
                            await chan.AddPermissionOverwriteAsync(user, new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny), RequestOptions.Default);
                        }
                        catch
                        {
                            Processing.LogConsoleText($"Failed to mute in {guild.Name}#{chan.Name.ToString()}", guild.Id);
                        }
                    }
                }
            }

            //Announce the mute
            var embed = Embeds.Mute(user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the mute in the bot-logs channel
            embed = Embeds.LogMute(moderator, channel, user);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(user.Guild.Id));
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void Unmute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            foreach (SocketTextChannel chan in user.Guild.TextChannels)
            {
                var guild = user.Guild;
                //Don't mess with channel permissions if nonmembers can't speak there anyway
                if (IsPublicChannel(chan))
                {
                    try
                    {
                        await chan.RemovePermissionOverwriteAsync(user, RequestOptions.Default);
                    }
                    catch
                    {
                        Processing.LogConsoleText($"Failed to unmute in {guild.Name}#{chan.Name.ToString()}", guild.Id);
                    }
                }
            }

            var embed = Embeds.Unmute(user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the unmute in the bot-logs channel
            embed = Embeds.LogUnmute(moderator, channel, user);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(user.Guild.Id));
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void Lock(SocketGuildUser moderator, ITextChannel channel)
        {
            //Lock the channel
            var guild = channel.Guild;
            //Don't mess with channel permissions if nonmembers can't speak there anyway
            if (IsPublicChannel((SocketGuildChannel)channel))
            {
                try
                {
                    await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Allow, sendMessages: PermValue.Deny, addReactions: PermValue.Deny), RequestOptions.Default);
                }
                catch
                {
                    Processing.LogConsoleText($"Failed to lock {guild.Name}#{channel.Name.ToString()}", guild.Id);
                }
            }

            //Announce the lock
            var embed = Embeds.Lock(channel);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the lock in the bot-logs channel
            embed = Embeds.LogLock(moderator, channel);
            var channelId = UserSettings.Channels.BotLogsId(channel.Guild.Id);
            var botlog = await channel.Guild.GetTextChannelAsync(channelId);
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void Unlock(SocketGuildUser moderator, ITextChannel channel)
        {
            //Unlock the channel
            var guild = channel.Guild;
            //Don't mess with channel permissions if nonmembers can't speak there anyway
            if (IsPublicChannel((SocketGuildChannel)channel))
            {
                try
                {
                    await channel.RemovePermissionOverwriteAsync(guild.EveryoneRole, RequestOptions.Default);
                }
                catch
                {
                    Processing.LogConsoleText($"Failed to unlock {guild.Name}#{channel.Name.ToString()}", guild.Id);
                }
            }

            //Announce the unlock
            var embed = Embeds.Unlock(channel);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the lock in the bot-logs channel
            embed = Embeds.LogUnlock(moderator, channel);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(channel.Guild.Id));
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void Kick(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            try {
                //Kick user
                await user.KickAsync(reason);

                //Announce kick in channel where infraction occured
                var embed = Embeds.Kick(user, reason);
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

                //Log kick in bot-logs
                embed = Embeds.KickLog(moderator, channel, user, reason);
                var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(user.Guild.Id));
                if (botlog != null)
                    await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
            catch
            {
                await channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", channel.Guild.Id));
            }
        }

        public static async void Ban(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            try {
                //Ban user
                await user.Guild.AddBanAsync((IUser)user, 0, reason);

                //Announce ban in channel where infraction occured
                var embed = Embeds.Ban(user, reason);
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

                //Log ban in bot-logs
                embed = Embeds.LogBan(moderator, channel, user, reason);
                var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(user.Guild.Id));
                if (botlog != null)
                    await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
            catch
            {
                await channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", channel.Guild.Id));
            }
        }

        public static async void DeleteMessages(ITextChannel channel, int amount)
        {
            var msgs = await channel.GetMessagesAsync(amount).FlattenAsync();

            await channel.DeleteMessagesAsync(msgs);
        }

        public static async void PruneLurkers(SocketGuildUser moderator, ITextChannel channel, IReadOnlyCollection<IGuildUser> users)
        {
            int usersPruned = 0;
            foreach (IGuildUser user in users)
            {
                if (user.RoleIds.Any(x => x == UserSettings.Roles.LurkerRoleID(channel.Guild.Id)))
                {
                    Console.WriteLine($"Lurker found: {user.Username}");
                    await user.KickAsync("Inactivity");
                    usersPruned++;
                }
            }

            var embed = Embeds.PruneLurkers(usersPruned);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the unmute in the bot-logs channel
            embed = Embeds.LogPruneLurkers(moderator, usersPruned);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(channel.Guild.Id));
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void PruneNonmembers(SocketGuildUser moderator, ITextChannel channel, IReadOnlyCollection<IGuildUser> users)
        {
            int usersPruned = 0;
            foreach (IGuildUser user in users)
            {
                if (user.RoleIds.Count <= 1)
                {
                    Processing.LogConsoleText($"Nonmember found: {user.Username}", channel.Guild.Id);
                    await user.KickAsync("Inactivity");
                    usersPruned++;
                }
            }

            var embed = Embeds.PruneNonmembers(usersPruned);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the prune in the bot-logs channel
            embed = Embeds.LogPruneNonmembers(moderator, usersPruned);
            var botlog = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.BotLogsId(channel.Guild.Id));
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static bool IsFiltered(IMessage message)
        {
            var guildchannel = (IGuildChannel)message.Channel;
            foreach (string term in File.ReadLines($"{guildchannel.Guild.Id.ToString()}\\filter.txt"))
            {
                if (Regex.IsMatch(message.Content.ToLower(), string.Format(@"\b{0}\b", Regex.Escape(term))))
                    message.DeleteAsync();
            }
            return false;
        }
    }
}
