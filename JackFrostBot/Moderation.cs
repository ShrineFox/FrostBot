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

namespace JackFrostBot
{
    public class Moderation
    {
        public static bool IsModerator(IGuildUser user)
        {
            bool moderator = false;
            List<ulong> modRoleIds = Setup.ModeratorRoleIds(user.Guild.Id);
            foreach (ulong roleID in user.RoleIds)
            {
                foreach (ulong modRoleId in modRoleIds)
                {
                    if (roleID == modRoleId)
                        moderator = true;
                }
            }
            return moderator;
        }

        public static bool IsModder(IGuildUser user)
        {
            bool modder = false;
            List<ulong> modRoleIds = Setup.ModderRoleIds(user.Guild.Id);
            foreach (ulong roleID in user.RoleIds)
            {
                foreach (ulong modRoleId in modRoleIds)
                {
                    if (roleID == modRoleId)
                        modder = true;
                }
            }
            return modder;
        }

        public static bool IsPublicChannel(IGuildChannel channel)
        {
            bool isPublic = true;
            List<ulong> privateChannelIds = Setup.PrivateChannelIds(channel.Guild.Id);
            foreach (ulong channelId in privateChannelIds)
            {
                if (channelId == channel.Id)
                    isPublic = false;
            }
            return isPublic;
        }

        public static int WarnLevel(SocketGuildUser user)
        {
            //Measure # of warns the user now has
            int WarnLevel = 0;
            foreach (string line in File.ReadLines($"{user.Guild.Id.ToString()}\\warns.txt"))
            {
                if (Convert.ToUInt64(line.Split(' ')[0]) == user.Id)
                {
                    WarnLevel++;
                }
            }

            return WarnLevel;
        }

        public static async void Warn(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            //Warn User
            var embed = Embeds.Warn(user, reason);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the warn in bot-logs
            embed = Embeds.LogWarn(moderator, channel, user, reason);
            var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(user.Guild.Id));
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Write the userid and reason for warn in warns.txt
            using (StreamWriter stream = File.AppendText($"{channel.Guild.Id.ToString()}\\warns.txt"))
            {
                stream.WriteLine($"{user.Id} ({user.Username}) {reason}");
            }
            //Measure # of warns the user now has
            int warns = 0;
            foreach (string line in File.ReadLines($"{channel.Guild.Id.ToString()}\\warns.txt"))
            {
                if (Convert.ToUInt64(line.Split(' ')[0]) == user.Id)
                {
                    warns++;
                }
            }

            //Mute, kick or ban a user if they've accumulated too many warns
            int muteLevel = Setup.MuteLevel(user.Guild.Id);
            int kickLevel = Setup.KickLevel(user.Guild.Id);
            int banLevel = Setup.BanLevel(user.Guild.Id);

            if (warns > 0)
            {
                await channel.SendMessageAsync($"{user.Username} has been warned {warns} times.");
                if (warns >= banLevel ) 
                    Ban(Setup.BotName(user.Guild.Id), channel, user,
                                "User was automatically banned for accumulating too many warnings.");
                else if (warns >= kickLevel)
                    Kick(Setup.BotName(user.Guild.Id), channel, user,
                                "User was automatically kicked for accumulating too many warnings.");
                else if (warns >= muteLevel)
                    Mute(Setup.BotName(user.Guild.Id), channel, user);
            }
        }

        public static async void ClearWarns(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
        {
            string warnsTxtPath = $"{channel.Guild.Id.ToString()}\\warns.txt";
            //Announce clearing of warns in both channel and bot-logs
            var embed = Embeds.ClearWarns(moderator, channel, user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(user.Guild.Id));
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //List all lines that don't match the user ID and overwrite warns.txt
            List<string> warnsToKeep = new List<string>();
            foreach (string line in File.ReadLines(warnsTxtPath))
            {
                if (Convert.ToUInt64(line.Split(' ')[0]) != user.Id)
                {
                    warnsToKeep.Add(line);
                }
            }
            File.Delete(warnsTxtPath);
            File.WriteAllLines(warnsTxtPath, warnsToKeep);
        }

        public static async void ClearWarn(SocketGuildUser moderator, ITextChannel channel, int index, SocketGuildUser user)
        {
            string warnsTxtPath = $"{channel.Guild.Id.ToString()}\\warns.txt";

            //List all lines that don't match the warm index and overwrite warns.txt
            List<string> warnsToKeep = new List<string>();
            string removedWarn = null;

            if (user == null)
            {
                foreach (string line in File.ReadLines(warnsTxtPath))
                {
                    warnsToKeep.Add(line);
                }

                for (int i = 0; i < warnsToKeep.Count; i++)
                {
                    if (index - 1 == i)
                    {
                        removedWarn = warnsToKeep[i];
                        warnsToKeep.RemoveAt(i);
                    }
                }
            }
            else
            {
                int matches = 0;
                foreach (string line in File.ReadLines(warnsTxtPath))
                {
                    if (Convert.ToUInt64(line.Split(' ')[0]) != user.Id)
                    {
                        warnsToKeep.Add(line);
                    }
                    else
                    {
                        matches++;
                        if (matches == index)
                        {
                            removedWarn = line;
                        }
                        else
                        {
                            warnsToKeep.Add(line);
                        }
                    }
                }
            }
            //Announce clearing of warns in both channel and bot-logs
            if (removedWarn != null)
            {
                var embed = Embeds.ClearWarn(moderator, removedWarn);
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(moderator.Guild.Id));
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                File.Delete(warnsTxtPath);
                File.WriteAllLines(warnsTxtPath, warnsToKeep);
            }
            else
            {
                await channel.SendMessageAsync("Failed to remove warn. Couldn't find it. Sorry, hee-ho!");
            }
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
            var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(user.Guild.Id));
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void Unmute(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
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
            var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(user.Guild.Id));
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void Lock(SocketGuildUser moderator, ITextChannel channel)
        {
            //Lock the channel
            var guild = channel.Guild;
            //Don't mess with channel permissions if nonmembers can't speak there anyway
            if (IsPublicChannel(channel))
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
            var channelId = Setup.BogLotChannelId(channel.Guild.Id);
            var botlog = await channel.Guild.GetTextChannelAsync(channelId);
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void Unlock(SocketGuildUser moderator, ITextChannel channel)
        {
            //Unlock the channel
            var guild = channel.Guild;
            //Don't mess with channel permissions if nonmembers can't speak there anyway
            if (IsPublicChannel(channel))
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
            var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(channel.Guild.Id));
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
                var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(user.Guild.Id));
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
            catch
            {
                await channel.SendMessageAsync(Setup.NoPermissionMessage(channel.Guild.Id));
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
                var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(user.Guild.Id));
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
            catch
            {
                await channel.SendMessageAsync(Setup.NoPermissionMessage(channel.Guild.Id));
            }
        }

        public static async void PruneLurkers(SocketGuildUser moderator, ITextChannel channel, IReadOnlyCollection<IGuildUser> users)
        {
            int usersPruned = 0;
            foreach (IGuildUser user in users)
            {
                if (user.RoleIds.Any(x => x == (Setup.LurkerRoleId(channel.Guild.Id))))
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
            var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(channel.Guild.Id));
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async void PruneNonmembers(SocketGuildUser moderator, ITextChannel channel, IReadOnlyCollection<IGuildUser> users)
        {
            int usersPruned = 0;
            foreach (IGuildUser user in users)
            {
                if (!user.RoleIds.Any(x => x == (Setup.MemberRoleId(moderator.Guild.Id))))
                {
                    Console.WriteLine($"Nonmember found: {user.Username}");
                    await user.KickAsync("Inactivity");
                    usersPruned++;
                }
            }

            var embed = Embeds.PruneNonmembers(usersPruned);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the unmute in the bot-logs channel
            embed = Embeds.LogPruneNonmembers(moderator, usersPruned);
            var botlog = await channel.Guild.GetTextChannelAsync(Setup.BogLotChannelId(channel.Guild.Id));
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static bool IsFiltered(IMessage message)
        {
            var guildchannel = (IGuildChannel)message.Channel;
            foreach (string term in File.ReadLines($"{guildchannel.Guild.Id.ToString()}\\filter.txt"))
            {
                if (Regex.IsMatch(message.Content.ToLower(), string.Format(@"\b{0}\b", Regex.Escape(term))))
                {
                    Console.WriteLine("Regex match");
                    message.DeleteAsync();
                    bool bypass = Moderation.IsBypassTerm(message.Author, term, message.Channel);
                    return bypass;
                }
            }
            return false;
        }

        public static bool IsBypassTerm(IUser author, string term, IMessageChannel channel)
        {
            var guildchannel = (IGuildChannel)channel;
            foreach (string bypassTerm in File.ReadLines($"{guildchannel.Guild.Id.ToString()}\\filterbypasscheck.txt"))
            {
                if (term == bypassTerm)
                {
                    Moderation.Warn(Setup.BotName(guildchannel.Guild.Id), (ITextChannel)channel, (SocketGuildUser)author, "Stop trying to bypass the word filter.");
                    return true;
                }
            }
            return false;
        }
    }
}
