using System;
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
                builder = builder.WithColor(new Color(color));
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
            Server selectedServer = Botsettings.SelectedServer(guildId);
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
            Server selectedServer = Botsettings.SelectedServer(guildId);
            string url = Path.Combine(selectedServer.Strings.WikiURL,
                $"w/api.php?action=query&prop=revisions&titles={keyword}&rvslots=*&rvprop=content&formatversion=2&format=json");

            // TODO: Separate json into embed fields

            return ColorMsg("", guildId);
        }

        // Shows that a user has been warned
        static public Embed Warn(IGuildUser user, IGuildUser moderator, ITextChannel channel, string reason, bool modDetails = false)
        {
            if (!modDetails)
                return ColorMsg($":warning: **Warn** {user.Mention}: {reason}", user.Guild.Id, red);
            else
                return ColorMsg($":warning: {moderator.Mention} **Warned** {user.Mention} in {channel.Mention}: {reason}", user.Guild.Id, red);
        }

        // Shows that all of a user's warns have been cleared
        static public Embed ClearWarns(SocketGuildUser moderator, SocketGuildUser user)
        {
            return ColorMsg($":ok_hand: **{moderator.Username} cleared all warns for** {user.Username}.", user.Guild.Id, green);
        }

        // Shows that a user's singular warn has been cleared
        static public Embed ClearWarn(SocketGuildUser moderator, Warn removedWarn)
        {
            return ColorMsg($":ok_hand: **{moderator.Username} cleared {removedWarn.UserName}'s warn**:\n{removedWarn.Reason}", moderator.Guild.Id, green);
        }

        // Shows that a user has been warned (along with who issued it)
        static public Embed LogWarn(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            return ColorMsg($":warning: **{moderator} warned {user.Username}** in #{channel}: {reason}", user.Guild.Id, red);
        }

        // Shows that a user has been muted
        static public Embed Mute(SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);
            return ColorMsg($":mute: **Muted {user.Username}**. {selectedServer.Strings.MuteMsg}", user.Guild.Id, red);
        }

        // Shows that a user has been muted (along with who issued it)
        static public Embed LogMute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            return ColorMsg($":mute: **{moderator} muted {user.Username}** in #{channel}.", user.Guild.Id, red);
        }

        // Shows that a user has been unmuted
        static public Embed Unmute(SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);
            return ColorMsg($":speaker: **Unmuted {user.Username}**. {selectedServer.Strings.UnmuteMsg}", user.Guild.Id, green);
        }

        // Shows that a user has been unmuted (along with who issued it)
        static public Embed LogUnmute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            return ColorMsg($":speaker: **{moderator} unmuted {user.Username}** in #{channel}.", user.Guild.Id, green);
        }

        // Shows that a user has been unmuted (along with who issued it)
        static public Embed Lock(ITextChannel channel)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);
            return ColorMsg($":lock: **Channel Locked.** {selectedServer.Strings.LockMsg}", channel.Guild.Id, red);
        }

        static public Embed LogLock(SocketGuildUser moderator, ITextChannel channel)
        {
            return ColorMsg($":lock: **{moderator.Username} locked** #{channel}.", channel.Guild.Id, red);
        }

        static public Embed Unlock(ITextChannel channel)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);
            return ColorMsg($":unlock: **Channel Unlocked.** {selectedServer.Strings.UnlockMsg}", channel.Guild.Id, green);
        }

        static public Embed LogUnlock(SocketGuildUser moderator, ITextChannel channel)
        {
            return ColorMsg($":unlock: **{moderator.Username} unlocked** #{channel}.", channel.Guild.Id, green);
        }

        static public Embed Kick(SocketGuildUser user, string reason)
        {
            return ColorMsg($":boot: **Kick {user.Username}**: {reason}", user.Guild.Id, red);
        }

        static public Embed KickLog(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            return ColorMsg($":boot: **{moderator} kicked {user.Username}** in #{channel}: {reason}", user.Guild.Id, red);
        }

        static public Embed Ban(SocketGuildUser user, string reason)
        {
            return ColorMsg($":hammer: **Ban {user.Username}**: {reason}", user.Guild.Id, red);
        }

        static public Embed LogBan(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            return ColorMsg($":hammer: **{moderator} banned {user.Username}** in #{channel}: {reason}", user.Guild.Id, red);
        }

        static public Embed PruneLurkers(int usersPruned, ulong guildId)
        {
            return ColorMsg($":scissors: **Pruned** {usersPruned} Lurkers.", guildId, red);
        }

        static public Embed LogPruneLurkers(SocketGuildUser moderator, int usersPruned)
        {
            return ColorMsg($":scissors: **{moderator.Username} Pruned** {usersPruned} Lurkers.", moderator.Guild.Id, red);
        }

        static public Embed LogMemberAdd(IGuildUser user)
        {
            return ColorMsg($":thumbsup: {user.Username} gained the **Member** role.", user.Guild.Id);
        }

        public static Embed ShowWarns(SocketTextChannel channel, SocketGuildUser user = null)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            List<string> warns = new List<string>();
            for (int i = 0; i < selectedServer.Warns.Count(); i++)
                warns.Add($"{i + 1}. **{selectedServer.Warns[i].UserName}**: {selectedServer.Warns[i].Reason}");

            return ColorMsg($"The following warns have been issued: \n\n{String.Join($"\n", warns.ToArray())}", channel.Guild.Id);
        }

        public static Embed ShowColors(SocketTextChannel channel, List<string> colorRoleNames, ulong guildId)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

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

        static public Embed Award(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{username} was awarded** {amount} {selectedServer.Strings.CurrencyName}.", author.Guild.Id, green);
        }

        static public Embed LogAward(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{author.Username} awarded** {username} {amount} {selectedServer.Strings.CurrencyName}.", author.Guild.Id, green);
        }

        static public Embed Redeem(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{username} redeemed** {amount} {selectedServer.Strings.CurrencyName}.", author.Guild.Id, green);

        }

        static public Embed LogRedeem(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{author.Username} took {amount} {selectedServer.Strings.CurrencyName}.", author.Guild.Id, green);

        }

        static public Embed Send(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{author.Username} sent** {username} {amount} {selectedServer.Strings.CurrencyName}.", author.Guild.Id, green);

        }

        static public Embed Earn(string author, int amount, ulong guildId)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);

            return ColorMsg($":money_with_wings: **{author} earned** {amount} {selectedServer.Strings.CurrencyName}.", guildId, green);

        }

        
    }
}
