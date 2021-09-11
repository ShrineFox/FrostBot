﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClutteredMarkov;
using System.Web;
using FrostBot;
using static FrostBot.Config;

namespace FrostBot
{
    public class Processing
    {
        //Log each message and url that comes in
        public static async Task LogSentMessage(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;

            //Create txt if it doesn't exist
            string logPath = Program.ymlPath.Replace("settings.yml", $"Servers//{guild.Id}//Log.txt");
            if (!Directory.Exists(Path.GetDirectoryName(logPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            if (!File.Exists(logPath))
                File.Create(logPath);

            string logLine = ($"<{DateTime.Now.ToString("hh:mm")}> {user.Username} in #{message.Channel}: {message}");
            if (message.Attachments.Count > 0)
                logLine = logLine + message.Attachments.FirstOrDefault().Url;
            Console.WriteLine(logLine);
            File.AppendAllText(logPath, logLine + "\n");
        }

        // Write info to both the console and the log txt file
        public static void LogConsoleText(string text)
        {
            // Create txt if it doesn't exist
            string logPath = Program.ymlPath.Replace("settings.yml", $"Log.txt");
            if (!Directory.Exists(Path.GetDirectoryName(logPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            if (!File.Exists(logPath))
                File.Create(logPath);

            string logLine = ($"<{DateTime.Now.ToString("hh:mm")}>: {text}");
            Console.WriteLine(logLine);
            File.AppendAllText(logPath, logLine + "\n");
        }

        // Write info on the last deleted message to bot log channel
        public static async Task LogDeletedMessage(SocketMessage message, string reason)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;
            Server selectedServer = Botsettings.SelectedServer(guild.Id);

            // Log deletion and delete message
            await message.DeleteAsync();
            var botlog = (SocketTextChannel)user.Guild.GetChannelAsync(selectedServer.Channels.BotLogs).Result;
            var embed = Embeds.DeletedMessage(message, reason);
            await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);

            // Warn user depending on settings
            if (selectedServer.WarnOnAutoDelete)
                Moderation.Warn(Program.client.CurrentUser.Username, (ITextChannel)message.Channel, (SocketGuildUser)message.Author, reason);
        }

        public static void LogDebugMessage(string message)
        {
            #if DEBUG
            Console.WriteLine($"<{DateTime.Now.ToString("hh:mm")}>: {message}");
            #endif
        }

        public static async Task LogEmbed(Embed embed, ITextChannel channel, bool sendInPublicChannel = false)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            var botlog = await channel.Guild.GetTextChannelAsync(selectedServer.Channels.BotLogs);
            if (botlog != null)
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            if (sendInPublicChannel)
                await channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        // Check if a message is a duplicate and if so, delete it
        public static async Task DuplicateMsgCheck(SocketMessage message, SocketGuildChannel channel)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;
            Server selectedServer = Botsettings.SelectedServer(guild.Id);

            if (channel.Id != selectedServer.Channels.BotSandbox 
                && !Moderation.IsModerator((IGuildUser)message.Author, guild.Id) && !message.Author.IsBot)
            {
                int matches = 0;

                foreach (IMessage msg in (await message.Channel.GetMessagesAsync(10).FlattenAsync()))
                {
                    if ((msg.Content.ToString() == message.Content.ToString()) && (msg.Author == message.Author) && (message.Attachments.Count == 0))
                    {
                        if (((message.Timestamp - msg.Timestamp).TotalSeconds < selectedServer.DuplicateFreq) && (msg.Timestamp < message.Timestamp))
                        {
                            matches++;
                            if (matches >= selectedServer.MaxDuplicates)
                            {
                                await LogDeletedMessage(message, "Duplicate message");
                            }
                        }
                    }
                }
            }
        }

        //Check if a message is filtered 
        public static async Task FilterCheck(SocketMessage message, SocketGuildChannel channel)
        {
            Server selectedServer = Botsettings.SelectedServer(channel.Guild.Id);

            if (Convert.ToBoolean(selectedServer.EnableWordFilter))
                foreach (string term in selectedServer.WordFilter)
                {
                    if (Regex.IsMatch(message.Content, string.Format(@"\b{0}\b|\b{0}d\b|\b{0}s\b|\b{0}ing\b|\b{0}ed\b|\b{0}er\b|\b{0}ers\b", Regex.Escape(term))))
                    {
                        // Censor word in warn/delete reason
                        string asterisks = "";
                        for (int i = 0; i < term.Length - 2; i++)
                            asterisks += "*";
                        string deleteReason = $"Message included a filtered term: {term.ToCharArray().First()}{asterisks}{term.ToCharArray().Last()}";

                        // Warn on filter match depending on settings, then delete
                        if (selectedServer.WarnOnFilter)
                            Moderation.Warn(channel.Guild.CurrentUser.Username, (ITextChannel)channel, (SocketGuildUser)message.Author, deleteReason);
                        await LogDeletedMessage(message, deleteReason);
                    }
                }
        }

        //Log replies and respond with a random reply
        public static async Task Markov(SocketUserMessage message, SocketGuildChannel channel, Server selectedServer, bool ignoreFreq = false)
        {
            string binFilePath = Program.ymlPath.Replace("config.yml", $"Servers\\{channel.Guild.Id}\\{channel.Guild.Id}");

            // Create markov object, attempt to load from path
            Markov mkv = new Markov();
            try { mkv.LoadChainState(binFilePath); }
            catch { }

            // Add incoming message to markov data 
            // if it doesn't include mentions or links, and channel is considered public
            if (message.Content != "" && Moderation.IsPublicChannel(channel)
                && message.MentionedUsers.Count <= 0 && message.MentionedRoles.Count <= 0 && !message.MentionedEveryone
                && !message.Content.Contains("http"))
                mkv.Feed(message.Content);

            // Save markov data to path
            Directory.CreateDirectory(Path.GetDirectoryName(binFilePath));
            mkv.SaveChainState(binFilePath);

            // Create new markov string from data so far
            string markov = MarkovGenerator.Create(mkv);
            // Try to make string more than 100 characters (up to 20 tries)
            int runs = 0;
            while (markov.Length < 100 && runs < 20)
            {
                markov = MarkovGenerator.Create(mkv);
                runs++;
            }

            // Send new markov message to channel (percentage of chance based on frequency setting)
            var socketChannel = (ISocketMessageChannel)channel;
            Random rnd = new Random();
            int frequency = 100;
            if (!ignoreFreq)
                frequency = selectedServer.MarkovFreq;
            if (rnd.Next(1, 100) <= frequency)
            {
                if (!selectedServer.BotChannelMarkovOnly)
                    await socketChannel.SendMessageAsync(markov);
                else if (channel.Id == selectedServer.Channels.BotSandbox)
                    await socketChannel.SendMessageAsync(markov);
            }

            return;
        }

    }
}
