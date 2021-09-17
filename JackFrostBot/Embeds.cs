﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Xml;
using Discord.WebSocket;
using System.IO;
using IniParser;
using IniParser.Model;
using FrostBot;
using static FrostBot.Config;

namespace FrostBot
{
    class Embeds
    {
        public static uint red = 0xD0021B;
        public static uint green = 0x37FF68;
        public static uint blue = 0xD0021B;

        public static uint GetUsernameColor(ulong guildId)
        {
            // Default color is blue if no role colors found
            uint colorValue = blue;
            // Convert highest role with color to uint
            var roles = Program.client.Guilds.First(x => x.Id.Equals(guildId)).CurrentUser.Roles.OrderBy(x => x.Position);
            if (roles.Count() > 0)
                foreach (var role in roles)
                    if (role.Color != new Discord.Color(0, 0, 0))
                        colorValue = (uint)((role.Color.R << 16) | (role.Color.G << 8) | role.Color.B);
            return colorValue;
        }

        // Message with colored box and optional icon/fields
        public static Embed ColorMsg(string msg, ulong guildId, uint color = 0, bool icon = false, List<Tuple<string, string>> fields = null)
        {
            if (color == 0)
                color = GetUsernameColor(guildId);
            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color));
            if (icon)
                builder = builder.WithThumbnailUrl(Botsettings.GetServer(guildId).Strings.BotIconURL);
            if (fields != null)
                foreach (var field in fields)
                    builder = builder.AddField(field.Item1, field.Item2);

            return builder.Build();
        }

        static public Embed DeletedMessage(IMessage message, string reason)
        {
            ITextChannel channel = (ITextChannel)message.Channel;
            return ColorMsg($"**Deleted Message** by {message.Author.Mention} in #{channel.Mention}:\n\n{message.Content}\n\nReason:{reason}", red);
        }

        // List of commands you're permitted to use
        static public Embed Help(ulong guildId, bool isModerator)
        {
            // Get list of commands, usage and descriptions as single string
            string help = "";
            Server selectedServer = Botsettings.GetServer(guildId);
            var cmds = selectedServer.Commands;
            // Only show non-moderator commands if user is not a moderator
            if (!isModerator)
                cmds = cmds.Where(x => !x.ModeratorsOnly).ToList();
            foreach (var cmd in cmds)
            {
                // Add command name and prefix to help string
                help += $"**{selectedServer.Prefix}{cmd.Name}**";
                // Narrow down command object from bot code
                var command = Program.commands.Modules
                    .Where(m => m.Parent == null)
                    .First(x => x.Commands
                    .Any(z => z.Name.Equals("say"))).Commands
                    .First(y => y.Name.Equals(cmd.Name));
                // Get list of parameters per command
                string paramList = "";
                foreach (var param in command.Parameters)
                    paramList += $" <{param.Name}>";
                // Append summary and newline
                help += paramList + $" {command.Summary}\n";
            }
            // Return first 2048 characters
            if (help.Length > 2048)
                help = help.Remove(2048);
            return ColorMsg(help, guildId);
        }

        // Returns info from a wiki page of a specified name
        static public Embed Wiki(string keyword, ulong guildId)
        {
            Server selectedServer = Botsettings.GetServer(guildId);
            string url = Path.Combine(selectedServer.Strings.WikiURL,
                $"w/api.php?action=query&prop=revisions&titles={keyword}&rvslots=*&rvprop=content&formatversion=2&format=json");

            // TODO: Separate json into embed fields

            return ColorMsg("", guildId);
        }

        // Shows that a user has been warned
        static public Embed Warn(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, string reason, bool modDetails = false)
        {
            if (!modDetails)
                return ColorMsg($":warning: **Warn** {user.Mention}: {reason}", user.Guild.Id, red);
            else
                return ColorMsg($":warning: {moderator.Mention} **Warned** {user.Mention} in {channel.Mention}: {reason}", user.Guild.Id, red);
        }

        // Shows that all of a user's warns have been cleared
        static public Embed ClearWarns(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, bool modDetails = false)
        {
            if (!modDetails)
                return ColorMsg($":ok_hand: **Cleared all warns for** {user.Mention}.", user.Guild.Id, green);
            else
                return ColorMsg($":ok_hand: **{moderator.Mention} cleared all warns for** {user.Mention} in {channel.Mention}.", user.Guild.Id, green);
        }

        // Shows that a user's singular warn has been cleared
        static public Embed ClearWarn(Warn removedWarn, SocketGuildUser moderator, bool modDetails = false)
        {
            string userMention = "";
            try
            {
                userMention = moderator.Guild.Users.First(x => x.Id.Equals(removedWarn.UserID)).Mention;
            } catch { userMention = removedWarn.UserName; }

            if (!modDetails)
                return ColorMsg($":ok_hand: **Cleared {userMention}'s Warn**:\n{removedWarn.Reason}", moderator.Guild.Id, green);
            else
                return ColorMsg($":ok_hand: **{moderator.Username} cleared {removedWarn.UserName}'s warn**:\n{removedWarn.Reason}", moderator.Guild.Id, green);
        }

        // Shows that a user has been muted
        static public Embed Mute(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, bool modDetails = false)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);
            if (!modDetails)
                return ColorMsg($":mute: **Muted {user.Username}**. {selectedServer.Strings.MuteMsg}", user.Guild.Id, red);
            else
                return ColorMsg($":mute: **{moderator} muted {user.Username}** in {channel.Mention}.", user.Guild.Id, red);
        }

        // Shows that a user has been unmuted
        static public Embed Unmute(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, bool modDetails = false)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);
            if (!modDetails)
                return ColorMsg($":speaker: **Unmuted {user.Username}**. {selectedServer.Strings.UnmuteMsg}", user.Guild.Id, green);
            else
                return ColorMsg($":speaker: **{moderator} unmuted {user.Username}** in {channel.Mention}.", user.Guild.Id, green);
        }

        // Shows that a user has been unmuted (along with who issued it)
        static public Embed Lock(SocketGuildUser moderator, ITextChannel channel, bool modDetails = false)
        {
            Server selectedServer = Botsettings.GetServer(channel.Guild.Id);
            if (!modDetails)
                return ColorMsg($":lock: **Channel Locked.** {selectedServer.Strings.LockMsg}", channel.Guild.Id, red);
            else
                return ColorMsg($":lock: **{moderator.Username} locked** {channel.Mention}.", channel.Guild.Id, red);
        }

        static public Embed Unlock(SocketGuildUser moderator, ITextChannel channel, bool modDetails = false)
        {
            Server selectedServer = Botsettings.GetServer(channel.Guild.Id);
            if (!modDetails)
                return ColorMsg($":unlock: **Channel Unocked.** {selectedServer.Strings.UnlockMsg}", channel.Guild.Id, green);
            else
                return ColorMsg($":unlock: **{moderator.Username} unlocked** {channel.Mention}.", channel.Guild.Id, green);
        }

        static public Embed Kick(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, string reason, bool modDetails = false)
        {
            if (!modDetails)
                return ColorMsg($":boot: **Kick {user.Username}**: {reason}", user.Guild.Id, red);
            else
                return ColorMsg($":boot: **{moderator.Mention} kicked {user.Username}** in {channel.Mention}: {reason}", user.Guild.Id, red);
        }

        static public Embed Ban(SocketGuildUser user, SocketGuildUser moderator, ITextChannel channel, string reason, bool modDetails = false)
        {
            if (!modDetails)
                return ColorMsg($":hammer: **Ban {user.Username}**: {reason}", user.Guild.Id, red);
            else
                return ColorMsg($":hammer: **{moderator.Mention} banned {user.Username}** in {channel.Mention}: {reason}", user.Guild.Id, red);
        }

        static public Embed PruneLurkers(SocketGuildUser moderator, int usersPruned, ulong guildId, bool modDetails = false)
        {
            if (!modDetails)
                return ColorMsg($":scissors: **Pruned** {usersPruned} Lurkers.", guildId, red);
            else
                return ColorMsg($":scissors: **{moderator.Mention} Pruned** {usersPruned} Lurkers.", moderator.Guild.Id, red);
        }

        static public Embed LogRoleAdd(IGuildUser user, IRole role)
        {
            return ColorMsg($":thumbsup: {user.Username} gained the {role.Name} role.", user.Guild.Id);
        }

        public static Embed ShowWarns(ITextChannel channel, SocketGuildUser mention = null)
        {
            Server selectedServer = Botsettings.GetServer(channel.Guild.Id);

            List<string> warns = new List<string>();
            if (mention != null)
                for (int i = 0; i < selectedServer.Warns.Count(); i++)
                    warns.Add($"{i + 1}. **{selectedServer.Warns[i].UserName}**: {selectedServer.Warns[i].Reason}");
            else
            {
                var userWarns = selectedServer.Warns.Where(x => x.UserID.Equals(mention.Id)).ToList();
                for (int i = 0; i < userWarns.Count(); i++)
                    warns.Add($"{i + 1}. **{userWarns[i].UserName}**: {userWarns[i].Reason}");
            }

            return ColorMsg($"The following warns have been issued: \n\n{String.Join($"\n", warns.ToArray())}", channel.Guild.Id);
        }

        public static Embed ShowColors(SocketTextChannel channel, List<string> colorRoleNames, ulong guildId)
        {
            Server selectedServer = Botsettings.GetServer(channel.Guild.Id);

            string desc = $"You can assign yourself the following color roles using " +
            $"``{selectedServer.Prefix}give color roleName``, " +
            $"or add your own using ``{selectedServer.Prefix}create color #hexValue roleName``: " +
            $"\n{String.Join(Environment.NewLine, colorRoleNames.ToArray())}";

            return ColorMsg(desc, channel.Guild.Id);
        }

        public static Embed Pin(SocketTextChannel channel, IMessage msg, IUser user)
        {
            var eBuilder = new EmbedBuilder()
            .WithTitle("Jump to message")
            .WithDescription(msg.Content)
            .WithUrl(msg.GetJumpUrl())
            .WithTimestamp(msg.Timestamp)
            .WithAuthor(msg.Author)
            .WithFooter($"Pinned by {user.Username} in #{channel.Name}", $"{user.GetAvatarUrl()}")
            .WithColor(GetUsernameColor(channel.Guild.Id));

            if (msg.Attachments.Count > 0)
                eBuilder.WithImageUrl(msg.Attachments.FirstOrDefault().Url);
            else
            {
                var links = msg.Content.Split("\t\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => (s.StartsWith("http://") || s.StartsWith("www.") || s.StartsWith("https://") || s.StartsWith("@")) && (s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".gif")));
                if (links.Count() > 0)
                {
                    eBuilder.WithImageUrl(links.FirstOrDefault());
                }
            }

            return eBuilder.Build();
        }

        static public Embed Award(SocketGuildUser user, SocketGuildUser moderator, int amount, bool modDetails = false)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);
            if (!modDetails)
                return ColorMsg($":money_with_wings: **{user.Mention} was awarded** {amount} {selectedServer.Strings.CurrencyName}.", user.Guild.Id, green);
            else
                return ColorMsg($":money_with_wings: **{moderator.Mention} awarded {user.Mention}** with {amount} {selectedServer.Strings.CurrencyName}.", user.Guild.Id, green);
        }

        static public Embed Redeem(SocketGuildUser user, SocketGuildUser moderator, int amount, bool modDetails = false)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);
            if (!modDetails)
                return ColorMsg($":money_with_wings: **{user.Mention} redeemed** {amount} {selectedServer.Strings.CurrencyName}.", user.Guild.Id);
            else
                return ColorMsg($":money_with_wings: **{moderator.Mention} took {user.Mention}**'s {amount} {selectedServer.Strings.CurrencyName}.", user.Guild.Id);
        }

        static public Embed Send(SocketGuildUser user, SocketGuildUser sender, int amount)
        {
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            return ColorMsg($":money_with_wings: **{sender.Mention} sent** {user.Mention} {amount} {selectedServer.Strings.CurrencyName}.", user.Guild.Id, green);
        }
    }
}
