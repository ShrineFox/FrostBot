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
                Processing.LogConsoleText($"Checking threads in Forum ID {forumIDs[i]}...", channel.GuildId);
                using (var client = new WebClient())
                {
                    string page = client.DownloadString($"https://shrinefox.com/forum/viewforum.php?f={forumIDs[i]}");
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(page);

                    //Get thread list
                    HtmlNodeCollection nodeCollection = doc
                                         .DocumentNode
                                         .SelectNodes("//ul[@class='topiclist topics']//dt//div//a[@class='topictitle']");

                    //Return thread info
                    foreach (var node in nodeCollection)
                    {
                        //Subnodes
                        var dateNode = node.ParentNode.Descendants("div").First(x => x.Attributes["class"].Value.Contains("responsive")); 

                        //Title Node
                        string threadUrl = WebUtility.HtmlDecode(node.Attributes["href"].Value.ToString()).Replace("./", "https://shrinefox.com/forum/");
                        threadUrl = threadUrl.Replace(threadUrl.Split('&')[2], "").TrimEnd('&');
                        string threadTitle = WebUtility.HtmlDecode(node.InnerText);
                        string tsvLine = "";

                        //Ignore sample thread
                        if (threadTitle != "New Mod Submission System")
                        {
                            //Date Node
                            var authorNode = dateNode.Descendants("a").First(x => x.Attributes["class"].Value.Contains("username"));
                            string authorUrl = WebUtility.HtmlDecode(authorNode.Attributes["href"].Value.ToString()).Replace("./", "https://shrinefox.com/forum/");
                            string authorName = WebUtility.HtmlDecode(authorNode.InnerText);
                            string date = WebUtility.HtmlDecode(dateNode.InnerText).Replace("\t", "").Split('\n')[1].Replace($"by {authorName} » ", "");

                            //Ignore thread if URL is already added or thread is older than last update
                            DateTime threadTime = new DateTime();
                            DateTime.TryParse(date, out threadTime);
                            DateTime fileTime = File.GetLastWriteTime("threads.txt");
                            if (!File.ReadAllText("threads.txt").Contains(threadUrl) && !File.ReadAllText("threads.txt").Contains(threadTitle) && threadTime > fileTime)
                            {
                                //Download thread and check for spoiler if link/title isn't already found in threads.txt
                                tsvLine = GetTSVLine(threadUrl, channel);
                                Processing.LogConsoleText($"Downloading Thread: {threadTitle}", channel.GuildId);
                            }
                            else
                            {
                                Processing.LogConsoleText($"Already found or outdated, skipping: {threadTitle}", channel.GuildId);
                            }

                            List<string> threadInfo = new List<string>();
                            threadInfo.Add(threadUrl); //0
                            threadInfo.Add(threadTitle); //1
                            threadInfo.Add(tsvLine); //2
                            threadInfo.Add(authorUrl); //3
                            threadInfo.Add(authorName); //4
                            threadInfo.Add(date); //5
                            threads.Add(threadInfo);
                        }
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

                //Load thread
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                //Find TSV Line spoiler and replace placeholder
                HtmlNodeCollection spoiler = doc
                    .DocumentNode
                    .SelectNodes("//div[@class='codebox']//pre");
                if (spoiler != null)
                    if (spoiler[0].FirstChild.InnerText.Contains("\t"))
                        tsvLine = spoiler[0].FirstChild.InnerText.Replace("mPt1FCy1Kzs1", threadUrl).Replace("THREAD_URL", threadUrl);

                //Return empty string or TSV row text
                return tsvLine;
            }
        }

        public static async Task NewForumPostCheck(IGuildChannel channel)
        {
            if (!File.Exists("threads.txt")) { File.CreateText("threads.txt").Close(); } //Create threads.txt

            var defaultChannel = (ITextChannel)channel; //Bot logs channel or wherever command is used
            var modShowcaseChannel = await channel.Guild.GetTextChannelAsync(UserSettings.Channels.ModShowcaseChannelId(channel.GuildId));
            int newSubmissions = 0; //Increments when new threads are detected

            await defaultChannel.SendMessageAsync("Beginning forum check, please wait...");
            
            //Create stringbuilder and add existing lines from threads.txt
            StringBuilder sb = new StringBuilder();
            foreach (var line in File.ReadLines("threads.txt"))
                sb.AppendLine(line);

            List<List<string>> threadList = Webscraper.DownloadForumPosts(channel); //Get threads in format: url|title|tsv|
            foreach (List<string> thread in threadList)
            {
                bool containsThread = false; //If threads.txt already contains entry
                foreach (var line in File.ReadLines("threads.txt"))
                {
                    if (line.StartsWith(thread[2].Split('\t')[0]))
                        containsThread = true;
                }
                if (!containsThread)
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
            string[] splitTSV = threadInfo[2].Split('\t');
            DateTime dateTime = new DateTime();
            DateTime.TryParse(threadInfo[5], out dateTime);

            var builder = new EmbedBuilder()
                .WithTitle(threadInfo[1])
                .WithUrl(threadInfo[0])
                .WithDescription(splitTSV[7])
                .WithColor(new Color(0x4A90E2))
                .AddField("Game", splitTSV[2], true)
                .AddField("Author", $"[{threadInfo[4]}]({threadInfo[3]})", true)
                .AddField("Download", $"[{splitTSV[10]}]({splitTSV[10]})");
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
                    Processing.LogConsoleText($"TSV DATA FOUND! Adding {modID} to mods.tsv", channel.GuildId);
                    File.AppendAllText(tsvPath, line + Environment.NewLine);
                }
                else
                {
                    Processing.LogConsoleText($"No TSV data found.", channel.GuildId);
                }
            }
            else
                Processing.LogConsoleText("Could not find Amicitia.github.io directory specified in BotOptions.xml", channel.GuildId);
        }

        public static void BuildHtml(ulong guildID)
        {
            Process cmd = new Process();
            cmd.StartInfo.WorkingDirectory = UserSettings.BotOptions.GetString("AmicitiaGithubIoPath", guildID);
            cmd.StartInfo.FileName = Path.Combine(UserSettings.BotOptions.GetString("AmicitiaGithubIoPath", guildID), "bin\\Debug\\Amicitia.github.io.exe");
            //cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
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