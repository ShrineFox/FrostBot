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
        public static SocketGuild GetGuild(ulong guildId)
        {
            return Program.client.Guilds.First(x => x.Id.Equals(guildId));
        }
        public static SocketGuildUser GetUser(ulong guildId, ulong userId)
        {
            return GetGuild(guildId).Users.First(x => x.Id.Equals(userId));
        }
        public static SocketRole GetRole(ulong guildId, ulong roleId)
        {
            return GetGuild(guildId).Roles.First(x => x.Id.Equals(roleId));
        }
        public static SocketTextChannel GetChannel(ulong guildId, ulong channelId)
        {
            return GetGuild(guildId).TextChannels.First(x => x.Id.Equals(channelId));
        }
        public static SocketThreadChannel GetThread(ulong guildId, ulong channelId, ulong threadId)
        {
            return GetChannel(guildId, channelId).Threads.First(x => x.Id.Equals(threadId));
        }

        // Returns true if a user isn't a bot, the command is enabled, has proper authority and is in the right channel
        public static bool CommandAllowed(string commandName, ICommandContext context)
        {
            Server selectedServer = Botsettings.GetServer(context.Guild.Id);
            ulong botChannelId = selectedServer.Channels.BotSandbox;
            bool isModerator = Moderation.IsModerator((SocketGuildUser)context.Message.Author, context.Guild.Id);

            Command cmd = selectedServer.Commands.First(x => x.Name.Equals(commandName));

            if (cmd.Enabled && !context.Message.Author.IsBot)
                if ((cmd.BotChannelOnly && (context.Channel.Id == botChannelId)) || !cmd.BotChannelOnly || isModerator)
                    if (!cmd.ModeratorsOnly || (cmd.ModeratorsOnly && isModerator))
                        return true;
            context.Channel.SendMessageAsync(embed: Embeds.ErrorMsg("You don't have permission to use that command here."));
            return false;
        }

        // If a user has a role marked as "moderator" in config.yml
        public static bool IsModerator(SocketGuildUser user, ulong guildId)
        {
            Server selectedServer = Botsettings.GetServer(guildId);

            if (user.Roles.Any(x => selectedServer.Roles.Any(y => y.Moderator && y.Id.Equals(x.Id))))
                return true;
            return false;
        }

        // If a channel doesn't have the "view channel" permission disabled for @everyone
        public static bool IsPublicChannel(SocketGuildChannel channel)
        {
            bool isPublic = false;
            if (!channel.PermissionOverwrites.Any(p => channel.Guild.EveryoneRole.Id.Equals(p.TargetId) 
                && p.Permissions.ViewChannel.Equals(PermValue.Deny)))
                    isPublic = true;

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
        public static async void Warn(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, string reason)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            // Warn user in channel where it was issued and log warn in bot-logs
            await channel.SendMessageAsync(embed: Embeds.Warn(user, moderator, channel, reason));
            await SendToBotLogs(Embeds.Warn(user, moderator, channel, reason, true), channel.Guild);

            // Save warn info to settings
            Botsettings.GetServer(user.Guild.Id).Warns.Add(new Warn() { UserName = user.Username, UserID = user.Id, Reason = reason, CreatedAt = DateTime.Now.ToString(), CreatedBy = moderator.Username});
            Botsettings.Save();

            // Measure # of warns the user now has
            int warns = WarnLevel(user);
            // Mute, kick or ban a user if they've accumulated too many warns
            if (warns >= 1)
            {
                // Program.client.Guilds.First(x => x.Id.Equals(guildId)).CurrentUser
                if (warns >= 2)
                    await channel.SendMessageAsync($"{user.Mention} has been warned {warns} times.");
                if (warns >= selectedServer.BanLevel) 
                    Ban(user, moderator, channel,
                                "User was automatically banned for accumulating too many warnings.");
                else if (warns >= selectedServer.KickLevel)
                    Kick(user, moderator, channel,
                                "User was automatically kicked for accumulating too many warnings.");
                else if (warns >= selectedServer.MuteLevel)
                    Mute(user, moderator, channel, selectedServer.MuteDuration);
            }

            // TODO: DM User
        }

        // Remove all records of infractions for a given user
        public static async void ClearWarns(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            // Announce clearing of warns in both channel and bot-logs
            await channel.SendMessageAsync(embed: Embeds.ClearWarns(user, moderator, channel));
            await SendToBotLogs(Embeds.ClearWarns(user, moderator, channel, true), channel.Guild);

            // Clear warns from this user
            Program.settings.Servers.First(x => x.Id.Equals(user.Guild.Id)).Warns = selectedServer.Warns.Where(x => !x.UserID.Equals(user.Id)).ToList();
        }

        // Remove a specific infraction from a user
        public static async void ClearWarn(SocketGuildUser moderator, ITextChannel channel, int index, SocketGuildUser user = null)
        {
            Server selectedServer = Botsettings.GetServer(moderator.Guild.Id);

            try
            {
                var removedWarn = new Warn();
                // Get warn user, reason and description
                if (user == null)
                    removedWarn = selectedServer.Warns.ElementAt(index - 1);
                else
                    removedWarn = selectedServer.Warns.Where(x => x.UserID.Equals(user.Id)).ElementAt(index - 1);

                // Announce clearing of warns in both channel and bot-logs
                await channel.SendMessageAsync(embed: Embeds.ClearWarn(removedWarn, moderator));
                await SendToBotLogs(Embeds.ClearWarn(removedWarn, moderator, true), channel.Guild);

                // Write new warns list to file
                selectedServer.Warns.Remove(removedWarn);
                Botsettings.UpdateServer(selectedServer);
            }
            catch
            {
                await channel.SendMessageAsync(embed: Embeds.ErrorMsg("Couldn't clear warn!"));
            }
        }

        // Stop a user from typing in all channels until unmuted
        public static async void Mute(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, int muteDur = 0)
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
                        // Save mute info to settings
                        // Remove mute info from settings
                        var server = Botsettings.GetServer(user.Guild.Id);
                        if (!server.Mutes.Any(x => x.UserID.Equals(user.Id)))
                        {
                            // Base default duration on settings, override with specified duration unique to this mute
                            var duration = 0;
                            if (server.MuteDuration > 0)
                                duration = server.MuteDuration;
                            if (muteDur > 0)
                                duration = muteDur;
                            server.Mutes.Add(new Mute() { UserName = user.Username, UserID = user.Id, CreatedAt = DateTime.Now.ToString(), CreatedBy = moderator.Username, Duration = duration });
                            Botsettings.UpdateServer(server);
                        }
                    }
                    catch
                    {
                        Processing.LogConsoleText($"Failed to mute in {guild.Name}#{chan.Name}");
                    }
                }
            }

            // Announce the mute (ambiguous as to who muted)
            await channel.SendMessageAsync(embed: Embeds.Mute(user, moderator, channel, false, muteDur));
            await SendToBotLogs(Embeds.Mute(user, moderator, channel, true, muteDur), channel.Guild);
        }

        // Removes a user's permission override in all channels
        public static async void Unmute(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel)
        {
            foreach (SocketTextChannel chan in user.Guild.TextChannels)
            {
                var guild = user.Guild;
                try
                {
                    await chan.RemovePermissionOverwriteAsync(user, RequestOptions.Default);
                    // Remove mute info from settings
                    var server = Botsettings.GetServer(user.Guild.Id);
                    if (server.Mutes.Any(x => x.UserID.Equals(user.Id)))
                    {
                        server.Mutes.Remove(server.Mutes.First(x => x.UserID.Equals(user.Id)));
                        Botsettings.UpdateServer(server);
                    }
                }
                catch
                {
                    Processing.LogConsoleText($"Failed to unmute in {guild.Name}#{chan.Name}");
                }
            }

            // Announce the unmute (ambiguous as to who unmuted
            await channel.SendMessageAsync(embed: Embeds.Unmute(user, moderator, channel));
            await SendToBotLogs(Embeds.Unmute(user, moderator, channel, true), channel.Guild);
        }

        // Stops @everyone (without a permission override) from typing in a public channel
        public static async void Lock(SocketGuildUser moderator, ITextChannel channel, int lockDur = 0)
        {
            // Lock the channel
            var guild = channel.Guild;
            // Don't mess with channel permissions if nonmembers can't speak there anyway
            if (IsPublicChannel((SocketGuildChannel)channel))
            {
                try
                {
                    await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Allow, sendMessages: PermValue.Deny, addReactions: PermValue.Deny), RequestOptions.Default);
                    // Base default duration on settings, override with specified duration unique to this lock
                    var duration = 0;
                    var server = Botsettings.GetServer(channel.Guild.Id);
                    if (server.LockDuration > 0)
                        duration = server.LockDuration;
                    if (lockDur > 0)
                        duration = lockDur;
                    server.Locks.Add(new Lock() { ChannelID = channel.Id, CreatedAt = DateTime.Now.ToString(), CreatedBy = moderator.Username, Duration = duration });
                    Botsettings.UpdateServer(server);
                }
                catch
                {
                    Processing.LogConsoleText($"Failed to lock {guild.Name}#{channel.Name}");
                }
            }
            
            // Announce the lock
            await channel.SendMessageAsync(embed: Embeds.Lock(moderator, channel));
            await SendToBotLogs(Embeds.Lock(moderator, channel, true), channel.Guild);
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
            await channel.SendMessageAsync(embed: Embeds.Unlock(moderator, channel));
            await SendToBotLogs(Embeds.Unlock(moderator, channel, true), channel.Guild);
        }

        // Removes a user from the server with a given reason
        public static async void Kick(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, string reason)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            try
            {
                // Kick user
                await user.KickAsync(reason);

                // Announce kick in channel where infraction occured (ambiguous as to who kicked)
                await channel.SendMessageAsync(embed: Embeds.Kick(user, moderator, channel, reason));
                await SendToBotLogs(Embeds.Kick(user, moderator, channel, reason, true), channel.Guild);

                // TODO: DM User
            }
            catch
            {
            }
        }

        public static async void Ban(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, string reason)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            try
            {
                // Announce ban in channel where infraction occured (ambiguous as to who issued ban)
                await channel.SendMessageAsync(embed: Embeds.Ban(user, moderator, channel, reason));
                await SendToBotLogs(Embeds.Ban(user, moderator, channel, reason, true), channel.Guild);

                // TODO: DM User

                // Ban user
                await user.Guild.AddBanAsync(user, 0, reason);
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
            await channel.SendMessageAsync(embed: Embeds.PruneLurkers(moderator, usersPruned, channel.GuildId));
            await SendToBotLogs(Embeds.PruneLurkers(moderator, usersPruned, channel.GuildId, true), channel.Guild);
        }

        // Put new currency into circulation by giving it to a user
        public static async void Award(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, int amount)
        {
            var selectedServer = Botsettings.GetServer(channel.GuildId);
            if (!selectedServer.Currency.Any(x => x.UserID.Equals(user.Id)))
                selectedServer.Currency.Add(new Currency() { UserName = user.Username, UserID = user.Id, Amount = amount });
            else
                selectedServer.Currency.First(x => x.UserID.Equals(user.Id)).Amount += amount;
            Botsettings.UpdateServer(selectedServer);

            await channel.SendMessageAsync("", embed: Embeds.Award(user, moderator, amount)).ConfigureAwait(false);
            await SendToBotLogs(Embeds.Award(user, moderator, amount, true), channel.Guild);
        }

        // Remove currency in circulation by taking from a user
        public static async void Redeem(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, int amount)
        {
            var selectedServer = Botsettings.GetServer(channel.GuildId);
            if (!selectedServer.Currency.Any(x => x.UserID.Equals(user.Id)) || selectedServer.Currency.First(x => x.UserID.Equals(user.Id)).Amount < amount)
            {
                await channel.SendMessageAsync($"User doesn't have enough {selectedServer.Strings.CurrencyName}.");
            }
            else
            {
                selectedServer.Currency.Single(x => x.UserID.Equals(user.Id)).Amount -= amount;
                Botsettings.UpdateServer(selectedServer);
                await channel.SendMessageAsync("", embed: Embeds.Redeem(user, moderator, amount)).ConfigureAwait(false);
                await SendToBotLogs(Embeds.Redeem(user, moderator, amount, true), channel.Guild);
            }
        }

        // Send currency you currently posess to another user
        internal static async void Send(SocketGuildUser user, SocketGuildUser sender, ITextChannel channel, int amount)
        {
            var selectedServer = Botsettings.GetServer(channel.GuildId);
            if (!selectedServer.Currency.Any(x => x.UserID.Equals(sender.Id)) || selectedServer.Currency.First(x => x.UserID.Equals(sender.Id)).Amount < amount)
            {
                await channel.SendMessageAsync($"You don't have enough {selectedServer.Strings.CurrencyName}.");
            }
            else
            {
                selectedServer.Currency.First(x => x.UserID.Equals(sender.Id)).Amount -= amount;
                if (!selectedServer.Currency.Any(x => x.UserID.Equals(user.Id)))
                    selectedServer.Currency.Add(new Currency() { UserName = user.Username, UserID = user.Id, Amount = amount });
                else
                    selectedServer.Currency.First(x => x.UserID.Equals(user.Id)).Amount += amount;
                Botsettings.UpdateServer(selectedServer);
                await channel.SendMessageAsync("", embed: Embeds.Send(user, sender, amount)).ConfigureAwait(false);
            }
        }
    }
}
