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
        // Message with colored box
        static public Embed ColorMsg(string msg, uint color)
        {
            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color));
            return builder.Build();
        }

        // Message with colored box and thumbnail image
        static public Embed ColorMsg(string msg, uint color, ulong guildId)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);

            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color))
            .WithThumbnailUrl(selectedServer.Strings.BotIconURL);
            return builder.Build();
        }

        // Message with colored box, thumbnail image, and additional field
        static public Embed ColorMsg(string msg, uint color, ulong guildId, Tuple<string, string> field)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);

            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color))
            .WithThumbnailUrl(selectedServer.Strings.BotIconURL)
            .AddField(field.Item1, field.Item2);
            return builder.Build();
        }

        // Message with colored box, thumbnail image, and two additional fields
        static public Embed ColorMsg(string msg, uint color, ulong guildId, Tuple<string, string> field, Tuple<string, string> field2)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);

            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color))
            .WithThumbnailUrl(selectedServer.Strings.BotIconURL)
            .AddField(field.Item1, field.Item2)
            .AddField(field2.Item1, field2.Item2);
            return builder.Build();
        }

        static public Embed DeletedMessage(SocketMessage message, string reason)
        {
            return ColorMsg($"**Deleted Message** by {message.Author.Username} in #{message.Channel}:\n\n{message.Content}\n\nReason:{reason}", 0xD0021B);
        }

        // List of commands you're permitted to use
        static public Embed Help(ulong guildId, bool isModerator)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);

            string commands = String.Join("\n", selectedServer.Commands.Where(x => x.ModeratorsOnly.Equals(isModerator)));
            if (commands.Length > 2048)
                commands = commands.Remove(2048);

            return ColorMsg(commands, 0x4A90E2, guildId);
        }

        // Returns info from a wiki page of a specified name
        static public Embed Wiki(string keyword, ulong guildId)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);
            string url = Path.Combine(selectedServer.Strings.WikiURL,
                $"w/api.php?action=query&prop=revisions&titles={keyword}&rvslots=*&rvprop=content&formatversion=2&format=json");

            // TODO: Separate json into embed fields

            return ColorMsg("", 0x4A90E2);
        }

        // Shows that a user has been warned
        static public Embed Warn(SocketGuildUser user, string reason)
        {
            return ColorMsg($":warning: **Warn** {user.Username}: {reason}", 0xD0021B);
        }

        // Shows that all of a user's warns have been cleared
        static public Embed ClearWarns(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
        {
            return ColorMsg($":ok_hand: **{moderator.Username} cleared all warns for** {user.Username}.", 0x37FF68);
        }

        // Shows that a user's singular warn has been cleared
        static public Embed ClearWarn(SocketGuildUser moderator, Warn removedWarn)
        {
            return ColorMsg($":ok_hand: **{moderator.Username} cleared {removedWarn.UserName}'s warn**:\n{removedWarn.Reason}", 0x37FF68);
        }

        // Shows that a user has been warned (along with who issued it)
        static public Embed LogWarn(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            return ColorMsg($":warning: **{moderator} warned {user.Username}** in #{channel}: {reason}", 0x37FF68);
        }

        // Shows that a user has been muted
        static public Embed Mute(SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);
            return ColorMsg($":mute: **Muted {user.Username}**. {selectedServer.Strings.MuteMsg}", 0x37FF68);
        }

        // Shows that a user has been muted (along with who issued it)
        static public Embed LogMute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            return ColorMsg($":mute: **{moderator} muted {user.Username}** in #{channel}.", 0x37FF68);
        }

        // Shows that a user has been unmuted
        static public Embed Unmute(SocketGuildUser user)
        {
            Server selectedServer = Botsettings.SelectedServer(user.Guild.Id);
            return ColorMsg($":speaker: **Unmuted {user.Username}**. {selectedServer.Strings.UnmuteMsg}", 0x37FF68);
        }

        // Shows that a user has been unmuted (along with who issued it)
        static public Embed LogUnmute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            return ColorMsg($":speaker: **{moderator} unmuted {user.Username}** in #{channel}.", 0x37FF68);
        }

        // Shows that a user has been unmuted (along with who issued it)
        static public Embed Lock(ITextChannel channel)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);
            return ColorMsg($":lock: **Channel Locked.** {selectedServer.Strings.LockMsg}", 0x37FF68);
        }

        static public Embed LogLock(SocketGuildUser moderator, ITextChannel channel)
        {
            return ColorMsg($":lock: **{moderator.Username} locked** #{channel}.", 0x37FF68);
        }

        static public Embed Unlock(ITextChannel channel)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);
            return ColorMsg($":unlock: **Channel Unlocked.** {selectedServer.Strings.UnlockMsg}", 0x37FF68);
        }

        static public Embed LogUnlock(SocketGuildUser moderator, ITextChannel channel)
        {
            return ColorMsg($":unlock: **{moderator.Username} unlocked** #{channel}.", 0x37FF68);
        }

        static public Embed Kick(SocketGuildUser user, string reason)
        {
            return ColorMsg($":boot: **Kick {user.Username}**: {reason}", 0xD0021B);
        }

        static public Embed KickLog(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            return ColorMsg($":boot: **{moderator} kicked {user.Username}** in #{channel}: {reason}", 0xD0021B);
        }

        static public Embed Ban(SocketGuildUser user, string reason)
        {
            return ColorMsg($":hammer: **Ban {user.Username}**: {reason}", 0xD0021B);
        }

        static public Embed LogBan(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            return ColorMsg($":hammer: **{moderator} banned {user.Username}** in #{channel}: {reason}", 0xD0021B);
        }

        static public Embed PruneLurkers(int usersPruned)
        {
            return ColorMsg($":scissors: **Pruned** {usersPruned} Lurkers.", 0xD0021B);
        }

        static public Embed LogPruneLurkers(SocketGuildUser moderator, int usersPruned)
        {
            return ColorMsg($":scissors: **{moderator.Username} Pruned** {usersPruned} Lurkers.", 0xD0021B);
        }

        static public Embed PruneNonmembers(int usersPruned)
        {
            return ColorMsg($":scissors: **Pruned** {usersPruned} Non-members.", 0xD0021B);
        }

        static public Embed LogPruneNonmembers(SocketGuildUser moderator, int usersPruned)
        {
            return ColorMsg($":scissors: **{moderator.Username} Pruned** {usersPruned} Non-members.", 0xD0021B);
        }

        static public Embed LogMemberAdd(IGuildUser user)
        {
            return ColorMsg($":thumbsup: {user.Username} gained the **Member** role.", 0x37FF68);
        }

        public static Embed ShowWarns(IGuildChannel channel, SocketGuildUser user = null)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            List<string> warns = new List<string>();
            for (int i = 0; i < selectedServer.Warns.Count(); i++)
                warns.Add($"{i + 1}. **{selectedServer.Warns[i].UserName}**: {selectedServer.Warns[i].Reason}");

            return ColorMsg($"The following warns have been issued: \n\n{String.Join($"\n", warns.ToArray())}", 0xD0021B);

        }

        public static Embed ShowColors(IGuildChannel channel, List<string> colorRoleNames, ulong guildId)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            string desc = $"You can assign yourself the following color roles using " +
            $"``{selectedServer.Prefix}give color roleName``, " +
            $"or add your own using ``{selectedServer.Prefix}create color #hexValue roleName``: " +
            $"\n{String.Join(Environment.NewLine, colorRoleNames.ToArray())}";

            return ColorMsg(desc, 0xF5DA23);

        }

        public static Embed Pin(IGuildChannel channel, IMessage msg, IUser user)
        {
            var eBuilder = new EmbedBuilder()
            .WithTitle("Jump to message")
            .WithDescription(msg.Content)
            .WithUrl(msg.GetJumpUrl())
            .WithTimestamp(msg.Timestamp)
            .WithAuthor(msg.Author)
            .WithFooter($"Pinned by {user.Username} in #{channel.Name}", $"{user.GetAvatarUrl()}")
            .WithColor(new Color(0x0094FF));

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

            return ColorMsg($":money_with_wings: **{username} was awarded** {amount} {selectedServer.Strings.CurrencyName}.", 0x0094FF);

        }

        static public Embed LogAward(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{author.Username} awarded** {username} {amount} {selectedServer.Strings.CurrencyName}.", 0x0094FF);

        }

        static public Embed Redeem(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{username} redeemed** {amount} {selectedServer.Strings.CurrencyName}.", 0x0094FF);

        }

        static public Embed LogRedeem(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{author.Username} took {amount} {selectedServer.Strings.CurrencyName}.", 0x0094FF);

        }

        static public Embed Send(SocketGuildUser author, string username, int amount)
        {
            Server selectedServer = Botsettings.SelectedServer(author.Guild.Id);

            return ColorMsg($":money_with_wings: **{author.Username} sent** {username} {amount} {selectedServer.Strings.CurrencyName}.", 0x0094FF);

        }

        static public Embed Earn(string author, int amount, ulong guildId)
        {
            Server selectedServer = Botsettings.SelectedServer(guildId);

            return ColorMsg($":money_with_wings: **{author} earned** {amount} {selectedServer.Strings.CurrencyName}.", 0x0094FF);

        }
    }
}
