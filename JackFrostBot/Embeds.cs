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

namespace JackFrostBot
{
    class Embeds
    {
        static public Embed ColorMsg(string msg, uint color)
        {
            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color));
            return builder.Build();
        }

        static public Embed ColorMsg(string msg, uint color, ulong guildId)
        {
            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color))
            .WithThumbnailUrl(UserSettings.BotOptions.GetString("BotIconURL", guildId));
            return builder.Build();
        }

        static public Embed ColorMsg(string msg, uint color, ulong guildId, Tuple<string, string> field)
        {
            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color))
            .WithThumbnailUrl(UserSettings.BotOptions.GetString("BotIconURL", guildId))
            .AddField(field.Item1, field.Item2);
            return builder.Build();
        }

        static public Embed ColorMsg(string msg, uint color, ulong guildId, Tuple<string, string> field, Tuple<string, string> field2)
        {
            var builder = new EmbedBuilder()
            .WithDescription(msg)
            .WithColor(new Color(color))
            .WithThumbnailUrl(UserSettings.BotOptions.GetString("BotIconURL", guildId))
            .AddField(field.Item1, field.Item2)
            .AddField(field2.Item1, field2.Item2);
            return builder.Build();
        }

        static public Embed Keywords(ulong guildId)
        {
            string description = $"Use ``{UserSettings.BotOptions.CommandPrefix(guildId)}about <term>`` " +
                $"for info about the following. ``{UserSettings.BotOptions.CommandPrefix(guildId)}help`` for more info.";

            var tuple = new Tuple<string, string>("Keywords", String.Join("\n", UserSettings.Info.Keywords(guildId).ToArray()));

            return ColorMsg(description, 0x4A90E2, guildId, tuple);
        }

        static public Embed Help(ulong guildId, bool isModerator)
        {
            string commands = String.Join("\n", UserSettings.Commands.List(guildId, isModerator).ToArray());
            if (commands.Length > 2048)
                commands = commands.Remove(2048);

            return ColorMsg(commands, 0x4A90E2, guildId);
        }

        static public Embed FormatInfo(string format, ulong guildId)
        {
            string about = UserSettings.Info.About(guildId, format);
            string links = UserSettings.Info.Links(guildId, format);
            var tuple = new Tuple<string, string>("Links", links);

            return ColorMsg(about, 0x4A90E2, guildId, tuple);
        }

        static public Embed Warn(SocketGuildUser user, string reason)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":warning: **Warn** {user.Username}: {reason}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed ClearWarns(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":ok_hand: **{moderator.Username} cleared all warns for** {user.Username}.")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed ClearWarn(SocketGuildUser moderator, string removedWarn)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":ok_hand: **{moderator.Username} cleared the following warn**:\n{removedWarn}")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed LogWarn(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":warning: **{moderator} warned {user.Username}** in #{channel}: {reason}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed Mute(SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":mute: **Muted {user.Username}**. {UserSettings.BotOptions.GetString("MuteMessage", user.Guild.Id)}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed LogMute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":mute: **{moderator} muted {user.Username}** in #{channel}.")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed Unmute(SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":speaker: **Unmuted {user.Username}**. {UserSettings.BotOptions.GetString("UnmuteMessage", user.Guild.Id)}")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed LogUnmute(string moderator, ITextChannel channel, SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":speaker: **{moderator} unmuted {user.Username}** in #{channel}.")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed Lock(ITextChannel channel)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":lock: **Channel Locked.** {UserSettings.BotOptions.GetString("LockMessage", channel.Guild.Id)}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed LogLock(SocketGuildUser moderator, ITextChannel channel)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":lock: **{moderator.Username} locked** #{channel}.")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed Unlock(ITextChannel channel)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":unlock: **Channel Unlocked.** {UserSettings.BotOptions.GetString("UnlockMessage", channel.Guild.Id)}")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed LogUnlock(SocketGuildUser moderator, ITextChannel channel)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":unlock: **{moderator.Username} unlocked** #{channel}.")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed Kick(SocketGuildUser user, string reason)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":boot: **Kick {user.Username}**: {reason}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed KickLog(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":boot: **{moderator} kicked {user.Username}** in #{channel}: {reason}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed Ban(SocketGuildUser user, string reason)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":hammer: **Ban {user.Username}**: {reason}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed LogBan(string moderator, ITextChannel channel, SocketGuildUser user, string reason)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":hammer: **{moderator} banned {user.Username}** in #{channel}: {reason}")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed PruneLurkers(int usersPruned)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":scissors: **Pruned** {usersPruned} Lurkers.")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed LogPruneLurkers(SocketGuildUser moderator, int usersPruned)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":scissors: **{moderator.Username} Pruned** {usersPruned} Lurkers.")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed PruneNonmembers(int usersPruned)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":scissors: **Pruned** {usersPruned} Non-members.")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed LogPruneNonmembers(SocketGuildUser moderator, int usersPruned)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":scissors: **{moderator.Username} Pruned** {usersPruned} Non-members.")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed LogMemberAdd(IGuildUser user)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":thumbsup: {user.Username} gained the **Member** role.")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        public static Embed ShowWarns(IGuildChannel channel, SocketGuildUser user = null)
        {
            List<string> warns = UserSettings.Warns.List(channel.Guild.Id);
            var eBuilder = new EmbedBuilder()
            .WithDescription($"The following warns have been issued: \n\n{String.Join($"\n", warns.ToArray())}")
            .WithColor(new Color(0xD0021B));
            return eBuilder.Build();
        }

        public static Embed ShowMsgInfo(ulong guildId)
        {
            string msgInfo = File.ReadAllText($"Servers\\{guildId.ToString()}\\LastDeletedMsg.txt");

            var eBuilder = new EmbedBuilder()
            .WithDescription($"Here's what I know about the last message I automatically deleted: \n\n{msgInfo}")
            .WithColor(new Color(0x37FF68));
            return eBuilder.Build();
        }

        public static Embed ShowColors(IGuildChannel channel, List<string> colorRoleNames, ulong guildId)
        {
            var eBuilder = new EmbedBuilder()
            .WithDescription($"You can assign yourself the following color roles using " +
            $"``{UserSettings.BotOptions.CommandPrefix(guildId)}give color roleName``, " +
            $"or add your own using ``{UserSettings.BotOptions.CommandPrefix(guildId)}create color #hexValue roleName``: " +
            $"\n{String.Join(Environment.NewLine, colorRoleNames.ToArray())}")
            .WithColor(new Color(0xF5DA23));
            return eBuilder.Build();
        }

        public static Embed Pin(IGuildChannel channel, IMessage msg)
        {
            var eBuilder = new EmbedBuilder()
            .WithDescription(msg.Content)
            .WithTimestamp(msg.Timestamp)
            .WithAuthor(msg.Author)
            .WithColor(new Color(0xF5DA23));

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

        static public Embed LogJoin(SocketGuild guild, SocketGuildUser user)
        {
            //Try to narrow down the name of the user who created the invite
            var invites = guild.GetInvitesAsync().Result.ToArray();
            string inviterMsg = ".";
            try
            {
                foreach (var inviteData in invites)
                {
                    List<string> codes = JackFrostBot.UserSettings.Invites.GetCodes(guild.Id);
                    foreach (string code in codes)
                    {
                        if (!invites.Any(x => x.Code.ToString().Equals(code)) && invites.Count().Equals(codes.Count - 1))
                        {
                            ulong inviterID = JackFrostBot.UserSettings.Invites.GetUser(guild.Id, code);
                            var inviter = guild.GetUser(inviterID);
                            inviterMsg = $"by **{inviter.Username}** ({inviter.Id}) using invite ``{code}``";
                        }
                    }
                }
            }
            catch { }

            var builder = new EmbedBuilder()
            .WithDescription($":calling: **{user.Username}** was invited to the server{inviterMsg}")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }

        static public Embed LogCreateInvite(SocketGuildUser user, string inviteId)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":calling: **{user.Username}** created an **invite link** with ID ``{inviteId}``.")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }

        static public Embed Award(SocketGuildUser author, string username, int amount)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":money_with_wings: **{username} was awarded** {amount} {UserSettings.BotOptions.GetString("CurrencyName", author.Guild.Id)}.")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }

        static public Embed LogAward(SocketGuildUser author, string username, int amount)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":money_with_wings: **{author.Username} awarded** {username} {amount} {UserSettings.BotOptions.GetString("CurrencyName", author.Guild.Id)}.")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }

        static public Embed Redeem(SocketGuildUser author, string username, int amount)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":money_with_wings: **{username} redeemed** {amount} {UserSettings.BotOptions.GetString("CurrencyName", author.Guild.Id)}.")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }

        static public Embed LogRedeem(SocketGuildUser author, string username, int amount)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":money_with_wings: **{author.Username} took {amount} {UserSettings.BotOptions.GetString("CurrencyName", author.Guild.Id)}** from {username}.")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }

        static public Embed Send(SocketGuildUser author, string username, int amount)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":money_with_wings: **{author.Username} sent** {username} {amount} {UserSettings.BotOptions.GetString("CurrencyName", author.Guild.Id)}.")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }

        static public Embed Earn(string author, int amount, ulong guildId)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":money_with_wings: **{author} earned** {amount} {UserSettings.BotOptions.GetString("CurrencyName", guildId)}.")
            .WithColor(new Color(0x0094FF));
            return builder.Build();
        }
    }
}
