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
        static public Embed List(ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            parser.Parser.Configuration.CaseInsensitive = true;
            IniData data = parser.ReadFile($"{guildId}\\info.ini");

            string keywords = "";

            foreach (var section in data.Sections)
            {
                keywords = $"{keywords}\n{section.SectionName}";
            }

            var builder = new EmbedBuilder()
                .WithDescription($"Use ``{Setup.CommandPrefix(guildId)}about <term>`` for info about the following. ``{Setup.CommandPrefix(guildId)}help`` for more info.")
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl(Setup.BotIconImage(guildId))
                .AddField("Keywords", keywords);
            var embed = builder.Build();

            return embed;
        }

        static public Embed BotInfo(ulong guildId)
        {
            var builder = new EmbedBuilder()
            .WithDescription($@"__**Moderation**__
:mag: **{Setup.CommandPrefix(guildId)}show warns <@username>** show a numbered list of all warnings a user has received.
@username is optional, and will show a list of all warnings ever issued if omitted.
:mag_right: **{Setup.CommandPrefix(guildId)}show msginfo** show the time, author and reason of the last auto-deleted message.

__**Fun**__
:microphone2: **{Setup.CommandPrefix(guildId)}say <message>** make {Setup.BotName(guildId)} say something (when used in the bot sandbox channel).
:crayon: **{Setup.CommandPrefix(guildId)}create color <#hexvalue> <roleName>** create a new role with a specific hexidecimal color.
:crayon: **{Setup.CommandPrefix(guildId)}update color <#hexvalue> <roleName>** show a list of all color roles you can pick from.
:crayon: **{Setup.CommandPrefix(guildId)}give color <roleName>** assign yourself a role with a specific color.
:crayon: **{Setup.CommandPrefix(guildId)}show colors** show a list of all color roles you can pick from.

__**Modding**__
:pencil: **{Setup.CommandPrefix(guildId)}list** show a list of all available keywords in the bot sandbox channel.
:question: **{Setup.CommandPrefix(guildId)}about <keyword>** gives all available resources on a keyword along with a description.
:warning: **{Setup.CommandPrefix(guildId)}release <url> <description>** announces a mod release if you have the Modders role. Images and YouTube links are automatically embedded.
                            
** Note:** While the bot anonymously issues these commands, a local log is kept of who uses them. Please use responsibly!")
            .WithColor(new Color(0x4A90E2))
            .WithThumbnailUrl(Setup.BotIconImage(guildId));
            var embed = builder.Build();

            return embed;
        }

        static public Embed FormatInfo(string format, ulong guildId)
        {
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";
            parser.Parser.Configuration.CaseInsensitive = true;
            IniData data = parser.ReadFile($"{guildId}\\info.ini");

            var builder = new EmbedBuilder()
                .WithTitle(format)
                .WithDescription(data[format]["Description"])
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl(Setup.BotIconImage(guildId))
                .AddField("Downloads", data[format]["Downloads"].ToString().Replace(@"\n", Environment.NewLine))
                .AddField("Related Wiki Pages", data[format]["Wiki"].Replace(@"\n", Environment.NewLine))
                .AddField("Guides & Resources", data[format]["Resources"].Replace(@"\n", Environment.NewLine));
            var embed = builder.Build();
            return embed;
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
            .WithDescription($":mute: **Muted {user.Username}**. {Setup.MuteMsg(user.Guild.Id)}")
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
            .WithDescription($":speaker: **Unmuted {user.Username}**. {Setup.UnmuteMsg(user.Guild.Id)}")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed LogUnmute(SocketGuildUser moderator, ITextChannel channel, SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":speaker: **{moderator.Username} unmuted {user.Username}** in #{channel}.")
            .WithColor(new Color(0x37FF68));
            return builder.Build();
        }

        static public Embed SlowMode()
        {
            var builder = new EmbedBuilder()
            .WithDescription($":timer: **Slowmode Activated.** Take your time with your replies or your messages will be de-hee-ted, ho!")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed LogSlowMode(SocketGuildUser moderator, ITextChannel channel)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":timer: **{moderator.Username} slowed down** #{channel}.")
            .WithColor(new Color(0xD0021B));
            return builder.Build();
        }

        static public Embed Lock(ITextChannel channel)
        {
            var builder = new EmbedBuilder()
            .WithDescription($":lock: **Channel Locked.** {Setup.LockMsg(channel.Guild.Id)}")
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
            .WithDescription($":unlock: **Channel Unlocked.** {Setup.UnlockMsg(channel.Guild.Id)}")
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
            string warnsTxtPath = $"{channel.Guild.Id.ToString()}\\warns.txt";
            List<string> warnsToShow = new List<string>();

            foreach (string line in File.ReadLines(warnsTxtPath))
            {
                if (user == null)
                {
                    warnsToShow.Add(line);
                }
                else if (Convert.ToUInt64(line.Split(' ')[0]) == user.Id)
                {
                    warnsToShow.Add(line);
                }
            }

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < warnsToShow.Count; i++ )
            {
                sBuilder.AppendLine($"#{i + 1}: {warnsToShow[i]}");
            }
            int muteLevel = Setup.MuteLevel(channel.Guild.Id);
            int kickLevel = Setup.KickLevel(channel.Guild.Id);
            int banLevel = Setup.BanLevel(channel.Guild.Id);
            if (user != null && warnsToShow.Count > 1)
            {
                sBuilder.AppendLine("\nWith this number of warns, the user should have been ");
                if (warnsToShow.Count >= banLevel)
                    sBuilder.Append("**Banned**.");
                else if (warnsToShow.Count >= kickLevel)
                    sBuilder.Append("**Kicked**.");
                else if (warnsToShow.Count >= muteLevel)
                    sBuilder.Append("**Muted**.");
            }

            var eBuilder = new EmbedBuilder()
            .WithDescription($"The following warns have been issued: \n{sBuilder.ToString()}")
            .WithColor(new Color(0xD0021B));
            return eBuilder.Build();
        }

        public static Embed ShowMsgInfo(ulong guildId)
        {
            string msgInfo = File.ReadAllText($"{guildId.ToString()}\\LastDeletedMsg.txt");

            var eBuilder = new EmbedBuilder()
            .WithDescription($"Here's what I know about the last message I automatically deleted: \n\n{msgInfo}")
            .WithColor(new Color(0x37FF68));
            return eBuilder.Build();
        }

        public static Embed PostRelease(string message, string username, string download)
        {
            var eBuilder = new EmbedBuilder()
            .WithDescription($"**{username}** released a mod:\n\n{message}\n**Download:** {download}")
            .WithColor(new Color(0xF5DA23));
            return eBuilder.Build();
        }

        public static Embed ShowColors(IGuildChannel channel, List<string> colorRoleNames, ulong guildId)
        {
            var eBuilder = new EmbedBuilder()
            .WithDescription($"You can assign yourself the following color roles using ``{Setup.CommandPrefix(guildId)}give color roleName``, or add your own using ``{Setup.CommandPrefix(guildId)}create color #hexValue roleName``: \n{String.Join(Environment.NewLine, colorRoleNames.ToArray())}")
            .WithColor(new Color(0xF5DA23));
            return eBuilder.Build();
        }
    }
}
