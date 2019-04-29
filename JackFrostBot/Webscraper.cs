using Discord;
using Discord.Commands;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JackFrostBot
{
    class Webscraper
    {
        private static readonly StringBuilder sBuilder = new StringBuilder(64);

        public static object HttpUtility { get; private set; }

        public static List<List<string>> DownloadForumPosts()
        {
            List<List<string>> threads = new List<List<string>>();

            using (var client = new WebClient())
            {
                //Download webpage
                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                //client.Proxy = null;
                string page = client.DownloadString("https://amicitiamods.jcink.net/index.php?showforum=13");

                //Load thread table
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                HtmlNodeCollection tdNodeCollection = doc
                                     .DocumentNode
                                     .SelectNodes("//div[@id='topic-list']//tr[@class='topic-row']//td");

                foreach (HtmlNode tdNode in tdNodeCollection)
                {
                    List<string> threadInfo = new List<string>();
                    if (tdNode.InnerHtml.Contains("This topic was started"))
                    {
                        HtmlNode link = tdNode.FirstChild.NextSibling;
                        string threadUrl = WebUtility.HtmlDecode(link.Attributes["href"].Value.ToString());
                        string threadTitle = WebUtility.HtmlDecode(link.InnerText);
                        string threadTime = WebUtility.HtmlDecode(link.Attributes["title"].Value.ToString());
                        threadInfo.Add(threadUrl);
                        threadInfo.Add(threadTitle);
                        threadInfo.Add(threadTime);

                        string threadAuthor = WebUtility.HtmlDecode(tdNode.NextSibling.NextSibling.FirstChild.InnerText);
                        string authorUrl = WebUtility.HtmlDecode(tdNode.NextSibling.NextSibling.FirstChild.Attributes["href"].Value.ToString());
                        threadInfo.Add(threadAuthor);
                        threadInfo.Add(authorUrl);
                        string postContents = DownloadPostContents(threadUrl);
                        threadInfo.Add(postContents);

                        threads.Add(threadInfo);
                    }
                }
            }

            return threads;
        }

        public static string DownloadPostContents(string threadUrl)
        {
            using (var client = new WebClient())
            {
                //Download webpage
                string page = client.DownloadString(threadUrl);

                //Load thread
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                HtmlNodeCollection postCollection = doc
                                     .DocumentNode
                                     .SelectNodes("//div[@class='postcolor']");

                string postContents = WebUtility.HtmlDecode(postCollection[0].InnerText);
                if (postContents.Length > 800)
                {
                    postContents = postContents.Substring(0,800);
                    postContents = $"{postContents}[...]";
                }

                return postContents;
            }
        }

        public static List<List<string>> DownloadWikiChanges()
        {
            List<List<string>> threads = new List<List<string>>();

            using (var client = new WebClient())
            {
                //Download webpage
                //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                //client.Proxy = null;
                string page = client.DownloadString("https://amicitia.miraheze.org/wiki/Special:RecentChanges?hidebots=1&limit=500&days=30&enhanced=1&urlversion=2");

                //Load changes table
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                HtmlNodeCollection tdNodeCollection = doc
                                     .DocumentNode
                                     .SelectNodes("//td[@class='mw-changeslist-line-inner']");

                foreach (HtmlNode tdNode in tdNodeCollection)
                {
                    try
                    {
                        List<string> threadInfo = new List<string>();
                        HtmlNode link = tdNode.FirstChild.NextSibling;
                        string threadUrl = WebUtility.HtmlDecode(link.FirstChild.Attributes["href"].Value.ToString());
                        threadUrl = threadUrl.Replace("(", "%28");
                        threadUrl = threadUrl.Replace(")", "%29");
                        string threadTitle = WebUtility.HtmlDecode(link.FirstChild.Attributes["title"].Value.ToString());
                        string threadDetails = WebUtility.HtmlDecode(tdNode.InnerText.ToString());
                        
                        threadDetails = threadDetails.Replace(threadTitle, "");
                        threadInfo.Add($"[{threadTitle}](https://amicitia.miraheze.org{threadUrl}) {threadDetails}");
                        
                        threads.Add(threadInfo);
                    }
                    catch { }
                }
            }

            return threads;
        }

        public static async Task NewForumPostCheck(IGuild guild)
        {
            if (guild.Id != 143149082582581258) { return;  }
            Processing.LogConsoleText("Beginning forum check...", guild.Id);
            bool writeNewTxt = false;
            StringBuilder sb = new StringBuilder();

            List<List<string>> threadList = Webscraper.DownloadForumPosts();
            foreach (List<string> thread in threadList)
            {
                if (!File.Exists("threads.txt")) { File.CreateText("threads.txt").Close(); }
                string threadsTxt = File.ReadAllText("threads.txt");
                bool containsThread = threadsTxt.Contains(thread[1]);

                if (!containsThread)
                {
                    writeNewTxt = true;
                    //Ping Helpers role
                    var helperRole = guild.GetRole(Setup.HelpersRoleId(guild.Id));
                    var forumPostChannel = await guild.GetTextChannelAsync(Setup.ForumPostsChannelId(guild.Id));
                    if (Setup.EnableHelpersPing(guild.Id))
                        await forumPostChannel.SendMessageAsync(helperRole.Mention);
                    //Post notification in designated channel
                    await NotifyAsync(guild, thread.ToArray());
                }
            }
            if (writeNewTxt)
            {
                //Create updated local doc if changes are found
                foreach (List<string> thread2 in threadList)
                {
                    foreach (string line in thread2)
                        sb.Append($"{line},");
                    sb.AppendLine();
                }
                //Once it's done comparing, overwrite original local txt doc with new one
                using (StreamWriter writer = new StreamWriter("threads2.txt", false))
                {
                    writer.Write(sb.ToString());
                }
                File.Delete("threads.txt");
                File.Move("threads2.txt", "threads.txt");
            }

            var defaultChannel = await guild.GetDefaultChannelAsync();
            try { await defaultChannel.SendMessageAsync(""); } catch { }
            Processing.LogConsoleText("Forum check complete", guild.Id);
        }

        public static async Task NewWikiChangeCheck(IGuild guild)
        {
            if (guild.Id != 143149082582581258) { return; }
            Processing.LogConsoleText("Beginning wiki check...", guild.Id);
            bool writeNewTxt = false;
            StringBuilder sb = new StringBuilder();

            List<string>[] changeList = Webscraper.DownloadWikiChanges().ToArray();
            Array.Reverse(changeList);
            foreach (List<string> changes in changeList)
            {
                if (!File.Exists("wikichanges.txt")) { File.CreateText("wikichanges.txt").Close(); }
                string threadsTxt = File.ReadAllText("wikichanges.txt");
                bool containsThread = threadsTxt.Contains(changes[0]);

                if (!containsThread)
                {
                    writeNewTxt = true;
                    //Post notification in designated channel
                    await NotifyWikiAsync(guild, changes.ToArray());
                }
            }
            if (writeNewTxt)
            {
                //Create updated local doc if changes are found
                foreach (List<string> change2 in changeList)
                {
                    foreach (string line in change2)
                        sb.Append($"{line},");
                    sb.AppendLine();
                }
                //Once it's done comparing, overwrite original local txt doc with new one
                using (StreamWriter writer = new StreamWriter("wikichanges2.txt", false))
                {
                    writer.Write(sb.ToString());
                }
                File.Delete("wikichanges.txt");
                File.Move("wikichanges2.txt", "wikichanges.txt");
            }

            var defaultChannel = await guild.GetDefaultChannelAsync();
            try { await defaultChannel.SendMessageAsync(""); } catch { }
            Processing.LogConsoleText("Wiki check complete", guild.Id);
        }

        public static Embed Notify(string[] threadInfo)
        {
            var builder = new EmbedBuilder()
                .WithDescription($"A new thread has been added to the Troubleshooting forum. {threadInfo[2]}")
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl("https://i.imgur.com/5I5Vos8.png")
                .AddField("Thread Title", $"[{threadInfo[1]}]({threadInfo[0]}) ")
                .AddField("Author", $"[{threadInfo[3]}]({threadInfo[4]}) ");
            var embed = builder.Build();

            return embed;
        }


        public static async Task NotifyAsync(IGuild guild, string[] threadInfo)
        {
            var builder = new EmbedBuilder()
                .WithDescription($"**A new thread has been added to the Troubleshooting forum. {threadInfo[2]}** \n\n{threadInfo[5]}")
                .WithColor(new Color(0x4A90E2))
                .WithThumbnailUrl("https://i.imgur.com/5I5Vos8.png")
                .AddField("Thread Title", $"[{threadInfo[1]}]({threadInfo[0]}) ")
                .AddField("Author", $"[{threadInfo[3]}]({threadInfo[4]}) ");
            var embed = builder.Build();

            var forumPostChannel = await guild.GetTextChannelAsync(Setup.ForumPostsChannelId(guild.Id));
            await forumPostChannel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        public static async Task NotifyWikiAsync(IGuild guild, string[] threadInfo)
        {
            string txt = threadInfo[0];
            Color clr = new Color(0x4A90E2);
            //Clean up formatting garbage
            txt = txt.Replace("â€Ž", "");
            txt = txt.Replace("Ã—", "");
            txt = txt.Replace(". .", " ");
            txt = txt.Replace("\n", " ");
            txt = txt.Replace("(talk | contribs)", "");
            txt = txt.Replace("(diff | hist)", "");
            txt = txt.Replace(" | history)", ")");

            //Set color based on whether lines were added or removed
            if (txt.Contains("(+"))
            {
                clr = new Color(0x7ED321);
            }
            else if (txt.Contains("(-"))
            {
                clr = new Color(0xD0021B);
            }

            var builder = new EmbedBuilder()
                .WithDescription(txt)
                .WithColor(clr);
            var embed = builder.Build();

            var wikiChangesChannel = await guild.GetTextChannelAsync(Setup.WikiChangesChannelId(guild.Id));
            await wikiChangesChannel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        string RemoveBetween(string s, string begin, string end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }
    }
}
