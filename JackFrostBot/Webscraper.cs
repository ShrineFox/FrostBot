using Discord;
using Discord.Commands;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JackFrostBot
{
    class Webscraper
    {
        public static List<List<string>> DownloadForumPosts(IGuildChannel channel)
        {
            List<List<string>> threads = new List<List<string>>();
            int[] forumIDs = new int[] { 11, 12, 13, 15, 16, 18, 19, 20, 27, 29, 30 };
            for (int i = 0; i < forumIDs.Count(); i++)
            {
                using (var client = new WebClient())
                {
                    //Download webpage
                    //client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    //client.Proxy = null;
                    string page = client.DownloadString($"https://shrinefox.com/forum/viewforum.php?f={forumIDs[i]}");

                    //Load thread table
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(page);

                    //Check for spoiler
                    HtmlNodeCollection nodeCollection = doc
                                         .DocumentNode
                                         .SelectNodes("//ul[@class='topiclist topics']//dt//div//a[@class='topictitle']");

                    //Return thread info
                    foreach (HtmlNode node in nodeCollection)
                    {
                        string threadUrl = WebUtility.HtmlDecode(node.Attributes["href"].Value.ToString()).Replace("./", "https://shrinefox.com/forum/");
                        string threadTitle = WebUtility.HtmlDecode(node.InnerText);
                        string tsvLine = "";
                        if (!File.ReadAllText("threads.txt").Contains(threadUrl))
                        {
                            tsvLine = GetTSVLine(threadUrl, channel);
                            Processing.LogConsoleText($"Downloading {threadUrl}...", channel.GuildId);
                        }

                        List<string> threadInfo = new List<string>();
                        threadInfo.Add(threadUrl.Replace(threadUrl.Split('&')[2], "").TrimEnd('&')); //0
                        threadInfo.Add(threadTitle); //1
                        threadInfo.Add(tsvLine); //2
                        threads.Add(threadInfo);
                    }
                }
            }

            return threads;
        }

        public static string GetTSVLine(string threadUrl, IGuildChannel channel)
        {
            //Get TSV row from spreadsheet if detected
            string tsvLine = "";
            using (var client = new WebClient())
            {
                //Download webpage
                string page = client.DownloadString(threadUrl);
                Processing.LogConsoleText($"Downloading {threadUrl}...", channel.GuildId);

                //Load thread
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                //Find TSV Line spoiler and replace placeholder
                HtmlNodeCollection spoiler = doc
                    .DocumentNode
                    .SelectNodes("//div[@class='codebox']//pre");
                if (spoiler != null)
                    if (spoiler[0].FirstChild.InnerText.Contains("mPt1FCy1Kzs1"))
                        tsvLine = spoiler[0].FirstChild.InnerText.Replace("mPt1FCy1Kzs1", threadUrl);

                //Return empty string or TSV row text
                return tsvLine;
            }
        }

        public static async Task NewForumPostCheck(IGuildChannel channel)
        {
            var defaultChannel = (ITextChannel)channel; //Bot logs channel or wherever command is used
            var modShowcaseChannel = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.ModShowcaseChannelId(channel.GuildId));
            int newSubmissions = 0; //Increments when new threads are detected

            await defaultChannel.SendMessageAsync("Beginning forum check, please wait...");
            StringBuilder sb = new StringBuilder();

            List<List<string>> threadList = Webscraper.DownloadForumPosts(channel); //Get threads in format: url|title|tsv|
            foreach (List<string> thread in threadList)
            {
                bool containsThread = false; //If threads.txt already contains entry
                if (!File.Exists("threads.txt")) { File.CreateText("threads.txt").Close(); } //Create threads.txt
                foreach (var line in File.ReadLines("threads.txt"))
                {
                    if (line.StartsWith(thread[2].Split('\t')[0]))
                        containsThread = true;
                }
                if (!containsThread && thread[2] != "")
                {
                    newSubmissions++; //tsv line starting with matching ID found
                    //Announce finding in mod showcase channel
                    await NotifyAsync(modShowcaseChannel, thread.ToArray());
                    UpdateTSV(channel, thread[2]);
                    //Update threads.txt stringbuilder
                    foreach (string splitLine in thread)
                        sb.Append($"{splitLine}|");
                    sb.AppendLine();
                }    
            }

            if (newSubmissions > 0)
            {
                //Once it's done comparing, overwrite original local txt doc with new one
                using (StreamWriter writer = new StreamWriter("threads2.txt", false))
                    writer.Write(sb.ToString());
                File.Delete("threads.txt");
                File.Move("threads2.txt", "threads.txt");

                await defaultChannel.SendMessageAsync("Forum check complete! Building HTML...");
                BuildHtml(channel.GuildId);
                Thread.Sleep(8000); //Wait for html to be built
                await defaultChannel.SendMessageAsync("HTML finished buildiing! Committing changes...");
                Thread.Sleep(8000); //Wait for git to finish committing
                GitCommit(channel.GuildId);
                await defaultChannel.SendMessageAsync("Changes committed! Pushing to Github...");
                Thread.Sleep(12000); //Wait for commit to be pushed
                GitPush(channel.GuildId);
                await defaultChannel.SendMessageAsync($"Update complete! Found {newSubmissions} new submissions.");
            }
            else
            {
                await defaultChannel.SendMessageAsync("Update complete! No new submissions found.");
            }
        }

        public static async Task NotifyAsync(ITextChannel channel, string[] threadInfo)
        {
            var builder = new EmbedBuilder()
                .WithDescription($"**New mod submission!**")
                .WithColor(new Color(0x4A90E2))
                .AddField("Thread Title", $"[{threadInfo[1]}]({threadInfo[0]})");
            var embed = builder.Build();

            await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }


        public static void UpdateTSV(IGuildChannel channel, string line)
        {
            string githubPath = UserSettings.BotOptions.GetString("AmicitiaGithubIoPath", channel.GuildId);
            string tsvPath = Path.Combine(githubPath, "db//mods.tsv");
            string modID = line.Split('\t')[0];

            if (File.Exists(tsvPath))
            {
                if (!File.ReadAllText(tsvPath).Contains(modID + "\t"))
                {
                    Processing.LogConsoleText($"Adding {modID} to mods.tsv", channel.GuildId);
                    File.AppendAllText(tsvPath, line + Environment.NewLine);
                }
            }
            else
                Processing.LogConsoleText("Could not find Amicitia.github.io directory specified in BotOptions.xml", channel.GuildId);
        }

        public static void BuildHtml(ulong guildID)
        {
            Process cmd = new Process();
            cmd.StartInfo.WorkingDirectory = UserSettings.BotOptions.GetString("AmicitiaGithubIoPath", guildID);
            cmd.StartInfo.FileName = "cmd.exe";
            //cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.Arguments = $"/C \"{Path.Combine(UserSettings.BotOptions.GetString("AmicitiaGithubIoPath", guildID), "bin\\Debug\\Amicitia.github.io.exe")}\"";
            cmd.Start();
            cmd.WaitForExit();
        }

        public static void GitCommit(ulong guildID)
        {
            Process cmd = new Process();
            cmd.StartInfo.WorkingDirectory = UserSettings.BotOptions.GetString("AmicitiaGithubIoPath", guildID);
            //cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = $"/C git commit -a -m \"Update database via JackFrostBot\"";
            cmd.Start();
            cmd.WaitForExit();
        }

        public static void GitPush(ulong guildID)
        {
            Process cmd = new Process();
            cmd.StartInfo.WorkingDirectory = UserSettings.BotOptions.GetString("AmicitiaGithubIoPath", guildID);
            //cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = $"/C git push";
            cmd.Start();
            cmd.WaitForExit();
        }
    }
}