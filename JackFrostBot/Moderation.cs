using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using System.Linq;
using Discord.Commands;
using FrostBot;
using System.Text.RegularExpressions;
using static FrostBot.Config;

namespace FrostBot
{
    class Moderation
    {
        // Returns true if a user isn't a bot, the command is enabled, has proper authority and is in the right channel
        public static bool CommandAllowed(string commandName, ICommandContext context)
        {
            Server selectedServer = Botsettings.SelectedServer(context.Guild.Id);
            ulong botChannelId = selectedServer.Channels.BotSandbox;
            bool isModerator = Moderation.IsModerator((IGuildUser)context.Message.Author, context.Guild.Id);

            Command cmd = selectedServer.Commands.First(x => x.Name.Equals(commandName));

            if (cmd.Enabled && !context.Message.Author.IsBot)
                if ((cmd.BotChannelOnly && (context.Channel.Id == botChannelId)) || !cmd.BotChannelOnly)
                    if (!cmd.ModeratorsOnly || (cmd.ModeratorsOnly && isModerator))
                        return true;
            return false;
        }

        // If a user has a role marked as "moderator" in config.yml
        public static bool IsModerator(IGuildUser user, ulong guildId)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);
            var guildUser = (SocketGuildUser)user;

            if (guildUser.Roles.Any(x => selectedServer.Roles.Any(y => y.Moderator && y.Id.Equals(x.Id))))
                return true;
            return false;
        }

        // If a channel doesn't have the "view channel" permission disabled for @everyone
        public static bool IsPublicChannel(SocketGuildChannel channel)
        {
            bool isPublic = false;
            if (channel.PermissionOverwrites.Any(p =>
                (p.TargetType == PermissionTarget.Role && channel.Guild.EveryoneRole.Id.Equals(p.TargetId))
                    && (p.Permissions.ViewChannel.Equals(PermValue.Allow)
                    || p.Permissions.ViewChannel.Equals(PermValue.Inherit))))
            {
                isPublic = true;
            }

            return isPublic;
        }

        // Measure # of warns the user now has
        public static int WarnLevel(SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);

            return selectedServer.Warns.Where(x => x.UserID.Equals(user.Id)).Count();
        }

        // Keep track of a rule infraction for automated moderation purposes
        public static async void Warn(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);

            reason = $"({user.Username}#{user.Discriminator}) {reason}";

            // Warn user in channel
            var embed = Embeds.Warn(user, reason);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the warn in bot-logs
            await Processing.LogEmbed(Embeds.LogWarn(moderator, channel, user, reason), channel);

            // Write the userid and reason in warns.xml
            Program.settings.Servers.First(x => x.Id.Equals(user.Guild.Id)).Warns.Add(new Warn() { UserName = user.Username, UserID = user.Id, Reason = reason, CreatedAt = DateTime.Now.ToString(), CreatedBy = moderator});

            // Measure # of warns the user now has
            int warns = WarnLevel(user);

            // Mute, kick or ban a user if they've accumulated too many warns
            if (warns > 0)
            {
                await channel.SendMessageAsync($"{user.Username} has been warned {warns} times.");
                if (warns >= selectedServer.BanLevel) 
                    Ban(user.Guild.CurrentUser.Username, channel, user,
                                "User was automatically banned for accumulating too many warnings.");
                else if (warns >= selectedServer.KickLevel)
                    Kick(user.Guild.CurrentUser.Username, channel, user,
                                "User was automatically kicked for accumulating too many warnings.");
                else if (warns >= selectedServer.MuteLevel)
                    Mute(user.Guild.CurrentUser.Username, channel, user);
            }
        }

        // Remove all records of infractions for a given user
        public static async void ClearWarns(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);

            // Announce clearing of warns in both channel and bot-logs
            await Processing.LogEmbed(Embeds.ClearWarns(moderator, channel, user), channel, true);

            // Clear warns from this user
            Program.settings.Servers.First(x => x.Id.Equals(user.Guild.Id)).Warns = selectedServer.Warns.Where(x => !x.UserID.Equals(user.Id)).ToList();
        }

        // Remove a specific infraction from a user
        public static async void ClearWarn(SocketGuildUser moderator, ITextChannel channel, int index, SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);

            try
            {
                // Get warn user, reason and description
                var removedWarn = selectedServer.Warns.ElementAt(index - 1);

                // Announce clearing of warns in both channel and bot-logs
                await Processing.LogEmbed(Embeds.ClearWarn(moderator, removedWarn), channel, true);

                // Write new warns list to file
                Program.settings.Servers.First(x => x.Id.Equals(user.Guild.Id)).Warns.Remove(removedWarn);
            }
            catch
            {
                await channel.SendMessageAsync("Couldn't clear warn!");
            }
        }

        // Stop a user from typing in all channels until unmuted
        public static async void Mute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);
            var guild = user.Guild;

            // Mute the specified user in each text channel if it's not a bot
            if (!user.IsBot)
            {
                foreach (SocketTextChannel chan in user.Guild.TextChannels)
                {
                    try
                    {
                        await chan.AddPermissionOverwriteAsync(user, 
                            new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny), 
                            RequestOptions.Default);
                    }
                    catch
                    {
                        Processing.LogConsoleText($"Failed to mute in {guild.Name}#{chan.Name}", guild.Id);
                    }
                }
            }

            // Announce the mute (ambiguous as to who muted)
            var embed = Embeds.Mute(user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the mute in the bot-logs channel (with admin name)
            await Processing.LogEmbed(Embeds.LogMute(moderator, channel, user), channel);
        }

        // Removes a user's permission override in all channels
        public static async void Unmute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            foreach (SocketTextChannel chan in user.Guild.TextChannels)
            {
                var guild = user.Guild;
                try
                {
                    await chan.RemovePermissionOverwriteAsync(user, RequestOptions.Default);
                }
                catch
                {
                    Processing.LogConsoleText($"Failed to unmute in {guild.Name}#{chan.Name}", guild.Id);
                }
            }

            // Announce the unmute (ambiguous as to who unmuted
            var embed = Embeds.Unmute(user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the unmute in the bot-logs channel
            await Processing.LogEmbed(Embeds.LogUnmute(moderator, channel, user), channel);
        }

        // Stops @everyone (without a permission override) from typing in a public channel
        public static async void Lock(SocketGuildUser moderator, ITextChannel channel)
        {
            // Lock the channel
            var guild = channel.Guild;
            // Don't mess with channel permissions if nonmembers can't speak there anyway
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
            
            // Announce the lock
            var embed = Embeds.Lock(channel);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the lock in the bot-logs channel
            await Processing.LogEmbed(Embeds.LogLock(moderator, channel), channel);
        }

        // Removes @everyone's permission override in a public channel
        public static async void Unlock(SocketGuildUser moderator, ITextChannel channel)
        {
            var guild = channel.Guild;
            // Don't mess with channel permissions if nonmembers can't speak there anyway
            if (IsPublicChannel((SocketGuildChannel)channel))
            {
                try
                {
                    await channel.RemovePermissionOverwriteAsync(guild.EveryoneRole, RequestOptions.Default);
                }
                catch
                {
                    Processing.LogConsoleText($"Failed to unlock {guild.Name}#{channel.Name}", guild.Id);
                }
            }

            // Announce the unlock
            var embed = Embeds.Unlock(channel);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the lock in the bot-logs channel
            await Processing.LogEmbed(Embeds.LogUnlock(moderator, channel), channel);
        }

        // Removes a user from the server with a given reason
        public static async void Kick(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);

            try
            {
                // Kick user
                await user.KickAsync(reason);

                // Announce kick in channel where infraction occured (ambiguous as to who kicked)
                var embed = Embeds.Kick(user, reason);
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

                // Log kick in bot-logs (with name of moderator that kicked)
                await Processing.LogEmbed(Embeds.KickLog(moderator, channel, user, reason), channel);

                // TODO: DM User
            }
            catch
            {
            }
        }

        public static async void Ban(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);

            try
            {
                // Ban user
                await user.Guild.AddBanAsync(user, 0, reason);

                // Announce ban in channel where infraction occured (ambiguous as to who issued ban)
                var embed = Embeds.Ban(user, reason);
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

                // Log ban in bot-logs (with who issued ban)
                await Processing.LogEmbed(Embeds.LogBan(moderator, channel, user, reason), channel);

                // TODO: DM User
            }
            catch
            {
            }
        }

        public static async void DeleteMessages(ITextChannel channel, int amount)
        {
            var msgs = await channel.GetMessagesAsync(amount).FlattenAsync();

            await channel.DeleteMessagesAsync(msgs);
        }

        // Kicks all users that still have an auto-assigned role, which is removed upon activity
        public static async void PruneLurkers(SocketGuildUser moderator, ITextChannel channel, IReadOnlyCollection<IGuildUser> users)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            int usersPruned = 0;
            foreach (IGuildUser user in users)
            {
                if (user.RoleIds.Any(x => selectedServer.Roles.Any(y => y.IsLurkerRole && y.Id.Equals(x))))
                {
                    Console.WriteLine($"Lurker found: {user.Username}");
                    await user.KickAsync("Inactivity");
                    usersPruned++;
                }
            }

            // Announce prune in channel where prune was initiated
            var embed = Embeds.PruneLurkers(usersPruned);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the unmute in the bot-logs channel along with who initiated it
            await Processing.LogEmbed(Embeds.LogPruneLurkers(moderator, usersPruned), channel);
        }

        // Kicks all users that lack a "verified" role, indicating server activity
        public static async void PruneNonmembers(SocketGuildUser moderator, ITextChannel channel, IReadOnlyCollection<IGuildUser> users)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            int usersPruned = 0;
            foreach (IGuildUser user in users)
            {
                if (!user.RoleIds.Any(x => selectedServer.Roles.Any(y => y.IsVerifiedRole && y.Id.Equals(x))))
                {
                    Processing.LogConsoleText($"Unverified member found: {user.Username}", channel.Guild.Id);
                    await user.KickAsync("Inactivity");
                    usersPruned++;
                }
            }
            
            // Announce prune in channel where prune was initiated
            var embed = Embeds.PruneNonmembers(usersPruned);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            //Log the prune in the bot-logs channel along with who initiated it
            await Processing.LogEmbed(Embeds.LogPruneNonmembers(moderator, usersPruned), channel);
        }
    }
}
