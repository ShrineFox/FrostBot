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

namespace JackFrostBot
{
    class Embeds
    {
        static public Embed List()
        {
            List<string> filetypes = new List<string>();
            List<string> programs = new List<string>();
            List<string> links = new List<string>();

            XmlDocument xdc = new XmlDocument();
            xdc.Load(@"Lists.xml");
            var nodes = xdc.SelectNodes("//FileTypes/Type//keywords");
            foreach (XmlNode node in nodes)
            {
                filetypes.Add(node.InnerText);
            }
            nodes = xdc.SelectNodes("//Programs/Type//keywords");
            foreach (XmlNode node in nodes)
            {
                programs.Add(node.InnerText);
            }
            xdc.Load(@"Links.xml");
            nodes = xdc.SelectNodes("//Links/Type//keywords");
            foreach (XmlNode node in nodes)
            {
                links.Add(node.InnerText);
            }

            var builder = new EmbedBuilder()
                .WithDescription("Use ``?about <term>`` for filetypes and programs, or ``?link <term>`` for links. ``?help`` for more info.")
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl("https://i.imgur.com/5I5Vos8.png")
                .AddField("Filetypes", string.Join("\n", filetypes.ToArray()))
                .AddField("Links", string.Join("\n", links.ToArray()))
                .AddField("Programs", string.Join("\n", programs.ToArray()));
            var embed = builder.Build();

            return embed;
        }

        static public Embed BotInfo()
        {
            var builder = new EmbedBuilder()
            .WithDescription(@"__**Moderation**__
:mag: **?show warns @username** show a numbered list of all warnings a user has received.
@username is optional, and will show a list of all warnings ever issued if omitted.
:mag_right: **?show msginfo** show the time, author and reason of the last auto-deleted message.

__**Fun**__
:microphone2: **?say message** make Jack Frost say something (when used in #bot-sandbox).

__**Modding**__
:pencil: **?list** show a list of all available keywords in #bot-sandbox.
:question: **?about keyword** gives all available resources on a keyword along with a description.
:grey_question: **?link keyword** used to link directly to available resources.

__**Roles**__
:sparkles: **?grant serious** grant yourself the Serious Talk role.
:leaves: **?remove serious** revoke your access to the Serious Talk channel.
:sparkles: **?grant artist** grant yourself the Artists role.
                            
** Note:** While Jack Frost anonymously issues these commands, a local log is kept of who uses them. Please use responsibly!")
            .WithColor(new Color(0x4A90E2))
            .WithThumbnailUrl("https://i.imgur.com/5I5Vos8.png");
            var embed = builder.Build();

            return embed;
        }

        static public Embed GetLinks(string format)
        {
            List<string> keywords = new List<string>();

            XmlDocument xdc = new XmlDocument();
            xdc.Load(@"Links.xml");
            var nodes = xdc.SelectNodes("//keywords");
            foreach (XmlNode node in nodes)
            {
                string[] splitKeywords = node.InnerText.Split(new string[] { ", " }, StringSplitOptions.None);
                foreach (string keyword in splitKeywords)
                {
                    keywords.Add(keyword);
                }
            }

            string name = "n/a";
            string description = "n/a";
            string downloads = "n/a";

            foreach (string keyword in keywords)
            {
                if (format == keyword)
                {
                    //Name and Description
                    var node = xdc.SelectSingleNode($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Name");
                    name = node.InnerText;
                    node = xdc.SelectSingleNode($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Description");
                    description = node.InnerText;
                    //Downloads
                    nodes = xdc.SelectNodes($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Downloads//Page");
                    List<string> dls = new List<string>();
                    foreach (XmlNode page in nodes)
                    {
                        dls.Add($"[{page.FirstChild.InnerText}]({page.LastChild.InnerText})");
                    }
                    if (dls.ToArray() != null && dls.ToArray().Length != 0)
                        downloads = string.Join("\n", dls.ToArray());
                }
            }

            var builder = new EmbedBuilder()
                .WithTitle(name)
                .WithDescription(description)
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl("https://i.imgur.com/5I5Vos8.png")
                .AddField("Downloads", downloads);
            var embed = builder.Build();
            return embed;
        }

        static public Embed FormatInfo(string format)
        {
            List<string> keywords = new List<string>();

            XmlDocument xdc = new XmlDocument();
            xdc.Load(@"Lists.xml");
            var nodes = xdc.SelectNodes("//keywords");
            foreach (XmlNode node in nodes)
            {
                string[] splitKeywords = node.InnerText.Split(new string[] { ", " }, StringSplitOptions.None);
                foreach (string keyword in splitKeywords)
                {
                    keywords.Add(keyword);
                }
            }

            string name = "n/a";
            string description = "n/a";
            string downloads = "n/a";
            string wikis = "n/a";
            string guides = "n/a";

            foreach (string keyword in keywords)
            {
                if (format == keyword)
                {
                    //Name and Description
                    var node = xdc.SelectSingleNode($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Name");
                    name = node.InnerText;
                    node = xdc.SelectSingleNode($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Description");
                    description = node.InnerText;
                    //Downloads
                    nodes = xdc.SelectNodes($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Downloads//Page");
                    List<string> dls = new List<string>();
                    foreach (XmlNode page in nodes)
                    {
                        dls.Add($"[{page.FirstChild.InnerText}]({page.LastChild.InnerText})");
                    }
                    if (dls.ToArray() != null && dls.ToArray().Length != 0)
                        downloads = string.Join("\n", dls.ToArray());
                    //Wiki Pages
                    nodes = xdc.SelectNodes($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Wiki//Page");
                    List<string> wiki = new List<string>();
                    foreach (XmlNode page in nodes)
                    {
                        wiki.Add($"[{page.FirstChild.InnerText}]({page.LastChild.InnerText})");
                    }
                    if (wiki.ToArray() != null && wiki.ToArray().Length != 0)
                        wikis = string.Join("\n", wiki.ToArray());
                    //Resources
                    nodes = xdc.SelectNodes($"//*[text()[contains(., '{keyword}')] or @*[contains(., '{keyword}')]]/../Resources//Page");
                    List<string> rsrc = new List<string>();
                    foreach (XmlNode page in nodes)
                    {
                        rsrc.Add($"[{page.FirstChild.InnerText}]({page.LastChild.InnerText})");
                    }
                    if (rsrc.ToArray() != null && rsrc.ToArray().Length != 0)
                        guides = string.Join("\n", rsrc.ToArray());
                }
            }

            var builder = new EmbedBuilder()
                .WithTitle(name)
                .WithDescription(description)
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl("https://i.imgur.com/5I5Vos8.png")
                .AddField("Downloads", downloads)
                .AddField("Related Wiki Pages", wikis)
                .AddField("Guides & Resources", guides);
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
    }
}
