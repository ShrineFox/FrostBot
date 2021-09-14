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
using System.Threading.Tasks;

namespace FrostBot
{
    class Moderation
    {
        // Returns true if a user isn't a bot, the command is enabled, has proper authority and is in the right channel
        public static bool CommandAllowed(string commandName, ICommandContext context)
        {
            Server selectedServer = Botsettings.GetServer(context.Guild.Id);
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
            Server selectedServer = Botsettings.GetServer(guildId);
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

        // Send moderation embed to bot logs channel
        public static async Task SendToBotLogs(Embed embed, IGuild guild)
        {
            Server selectedServer = Botsettings.GetServer(guild.Id);

            var botlog = await guild.GetTextChannelAsync(selectedServer.Channels.BotLogs);
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed);
            return;
        }

        // Measure # of warns the user now has
        public static int WarnLevel(IGuildUser user)
        {
            return Botsettings.GetServer(user.Guild.Id).Warns.Where(x => x.UserID.Equals(user.Id)).Count();
        }

        // Keep track of a rule infraction for automated moderation purposes
        public static async void Warn(IGuildUser moderator, ITextChannel channel, IGuildUser user, string reason)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            reason = $"({user.Username}#{user.Discriminator}) {reason}";

            // Warn user in channel where it was issued and log warn in bot-logs
            await channel.SendMessageAsync(embed: Embeds.Warn(user, moderator, channel, reason));
            await SendToBotLogs(Embeds.Warn(user, moderator, channel, reason, true), channel.Guild);

            // Save warn info to settings
            Program.settings.Servers.First(x => x.Id.Equals(user.Guild.Id)).Warns.Add(new Warn() { UserName = user.Username, UserID = user.Id, Reason = reason, CreatedAt = DateTime.Now.ToString(), CreatedBy = moderator.Username});

            // Measure # of warns the user now has
            int warns = WarnLevel(user);
            // Mute, kick or ban a user if they've accumulated too many warns
            if (warns > 0)
            {
                /*
                await channel.SendMessageAsync($"{user.Username} has been warned {warns} times.");
                if (warns >= selectedServer.BanLevel) 
                    Ban(user.Guild.CurrentUser.Username, channel, user,
                                "User was automatically banned for accumulating too many warnings.");
                else if (warns >= selectedServer.KickLevel)
                    Kick(user.Guild.CurrentUser.Username, channel, user,
                                "User was automatically kicked for accumulating too many warnings.");
                else if (warns >= selectedServer.MuteLevel)
                    Mute(user.Guild.CurrentUser.Username, channel, user);*/
            }
        }

        // Remove all records of infractions for a given user
        public static async void ClearWarns(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            // Announce clearing of warns in both channel and bot-logs
            await Moderation.SendToBotLogs(Embeds.ClearWarns(moderator, user), channel.Guild);

            // Clear warns from this user
            Program.settings.Servers.First(x => x.Id.Equals(user.Guild.Id)).Warns = selectedServer.Warns.Where(x => !x.UserID.Equals(user.Id)).ToList();
        }

        // Remove a specific infraction from a user
        public static async void ClearWarn(SocketGuildUser moderator, ITextChannel channel, int index, SocketGuildUser user)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            try
            {
                // Get warn user, reason and description
                var removedWarn = selectedServer.Warns.ElementAt(index - 1);

                // Announce clearing of warns in both channel and bot-logs
                await Moderation.SendToBotLogs(Embeds.ClearWarn(moderator, removedWarn), channel.Guild);

                // Write new warns list to file
                Program.settings.Servers.First(x => x.Id.Equals(user.Guild.Id)).Warns.Remove(removedWarn);
            }
            catch
            {
                await channel.SendMessageAsync(embed: Embeds.ColorMsg("Couldn't clear warn!", user.Guild.Id));
            }
        }

        // Stop a user from typing in all channels until unmuted
        public static async void Mute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);
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
                        Processing.LogConsoleText($"Failed to mute in {guild.Name}#{chan.Name}");
                    }
                }
            }

            // Announce the mute (ambiguous as to who muted)
            var embed = Embeds.Mute(user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the mute in the bot-logs channel (with admin name)
            await Moderation.SendToBotLogs(Embeds.LogMute(moderator, channel, user), channel.Guild);
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
                    Processing.LogConsoleText($"Failed to unmute in {guild.Name}#{chan.Name}");
                }
            }

            // Announce the unmute (ambiguous as to who unmuted
            var embed = Embeds.Unmute(user);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the unmute in the bot-logs channel
            await Moderation.SendToBotLogs(Embeds.LogUnmute(moderator, channel, user), channel.Guild);
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
                    Processing.LogConsoleText($"Failed to lock {guild.Name}#{channel.Name}");
                }
            }
            
            // Announce the lock
            var embed = Embeds.Lock(channel);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the lock in the bot-logs channel
            await SendToBotLogs(Embeds.LogLock(moderator, channel), channel.Guild);
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
                    Processing.LogConsoleText($"Failed to unlock {guild.Name}#{channel.Name}");
                }
            }

            // Announce the unlock
            var embed = Embeds.Unlock(channel);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the lock in the bot-logs channel
            await Moderation.SendToBotLogs(Embeds.LogUnlock(moderator, channel), channel.Guild);
        }

        // Removes a user from the server with a given reason
        public static async void Kick(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            try
            {
                // Kick user
                await user.KickAsync(reason);

                // Announce kick in channel where infraction occured (ambiguous as to who kicked)
                var embed = Embeds.Kick(user, reason);
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

                // Log kick in bot-logs (with name of moderator that kicked)
                await Moderation.SendToBotLogs(Embeds.KickLog(moderator, channel, user, reason), channel.Guild);

                // TODO: DM User
            }
            catch
            {
            }
        }

        public static async void Ban(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            try
            {
                // Ban user
                await user.Guild.AddBanAsync(user, 0, reason);

                // Announce ban in channel where infraction occured (ambiguous as to who issued ban)
                var embed = Embeds.Ban(user, reason);
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

                // Log ban in bot-logs (with who issued ban)
                await Moderation.SendToBotLogs(Embeds.LogBan(moderator, channel, user, reason), channel.Guild);

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
            Server selectedServer = Botsettings.GetServer(channel.Guild.Id);

            int usersPruned = 0;
            foreach (IGuildUser user in users)
            {
                if (user.RoleIds.Any(x => selectedServer.Roles.Any(y => y.IsLurkerRole && y.Id.Equals(x))))
                {
                    Processing.LogDebugMessage($"Lurker found: {user.Username}");
                    await user.KickAsync("Inactivity");
                    usersPruned++;
                }
            }

            // Announce prune in channel where prune was initiated
            var embed = Embeds.PruneLurkers(usersPruned, channel.Guild.Id);
            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Log the unmute in the bot-logs channel along with who initiated it
            await Moderation.SendToBotLogs(Embeds.LogPruneLurkers(moderator, usersPruned), channel.Guild);
        }
    }
}
