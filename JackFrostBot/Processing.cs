using Discord;
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
            foreach (Embed embed in message.Embeds)
                if (embed.Description != null)
                    logLine += "\n" + embed.Description;
                
            if (message.Attachments.Count > 0)
                logLine += "\n" + message.Attachments.FirstOrDefault().Url;
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
        public static async Task LogDeletedMessage(IMessage message, string reason)
        {
            var user = (IGuildUser)message.Author;
            var botUser = Moderation.GetUser(user.Guild.Id, Program.client.CurrentUser.Id);
            Server selectedServer = Botsettings.GetServer(user.Guild.Id);

            // Delete message and log deletion
            await message.DeleteAsync();
            await Moderation.SendToBotLogs(Embeds.DeletedMessage(message, reason), user.Guild);

            // Warn user depending on settings
            if (selectedServer.WarnOnAutoDelete)
                Moderation.Warn((SocketGuildUser)message.Author, botUser, (ITextChannel)message.Channel, reason);
        }

        public static void LogDebugMessage(string message)
        {
            #if DEBUG
            LogConsoleText(message);
            #endif
        }

        // Check if a message is a duplicate and if so, delete it
        public static async Task DuplicateMsgCheck(SocketMessage message, SocketGuildChannel channel)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;
            Server selectedServer = Botsettings.GetServer(guild.Id);

            int matches = 0;
            if (selectedServer.AutoDeleteDupes && channel.Id != selectedServer.Channels.BotSandbox 
                && !Moderation.IsModerator((SocketGuildUser)message.Author, guild.Id) && !message.Author.IsBot)
            {
                
                foreach (IMessage msg in (await message.Channel.GetMessagesAsync(10).FlattenAsync()))
                    if ((msg.Content.ToString() == message.Content.ToString()) && (msg.Author == message.Author) && (message.Attachments.Count == 0))
                        if (((message.Timestamp - msg.Timestamp).TotalSeconds < selectedServer.DuplicateFreq) && (msg.Timestamp < message.Timestamp))
                            matches++;
            }

            if (matches > selectedServer.MaxDuplicates)
                await LogDeletedMessage(message, $"Duplicate message sent {selectedServer.MaxDuplicates} times within {selectedServer.DuplicateFreq} seconds.");
        }

        // Check if a message is filtered 
        public static async Task FilterCheck(IMessage message, ITextChannel channel)
        {
            Server selectedServer = Botsettings.GetServer(channel.Guild.Id);

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

                        // Auto-warn on filter match depending on settings, then delete message
                        var botUser = await channel.Guild.GetUserAsync(Program.client.CurrentUser.Id);
                        if (selectedServer.WarnOnFilter)
                            Moderation.Warn((SocketGuildUser)message.Author, (SocketGuildUser)botUser, channel, deleteReason);
                        await LogDeletedMessage(message, deleteReason);
                    }
                }
        }

        // Log replies and respond with a random reply
        public static async Task Markov(SocketUserMessage message, SocketGuildChannel channel, Server selectedServer, bool ignoreFreq = false)
        {
            string binFilePath = Program.ymlPath.Replace("settings.yml", $"Servers\\{channel.Guild.Id}\\markov");
            if (!Directory.Exists(Path.GetDirectoryName(binFilePath)))
                Directory.CreateDirectory(binFilePath);

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

        public static async Task UnarchiveThreads(SocketGuild guild)
        {
            var selectedServer = Botsettings.GetServer(guild.Id);
            // Unarchive threads listed in bot config automatically
            foreach (var threadChannel in guild.ThreadChannels)
            {
                if (threadChannel.Archived && selectedServer.ThreadsToUnarchive.Any(x => x.Equals(threadChannel.Id)))
                    await threadChannel.ModifyAsync(x => x.Archived = false);
            }
        }

        public static async Task PublishNews(SocketGuild guild)
        {
            // Crosspost last 25 messages in each announcement channel
            foreach (var newsChannel in guild.Channels.Where(x => x.GetType().Equals(ChannelType.News)))
            {
                LogDebugMessage($"Publishing news in {guild.Name} #{newsChannel.Name}...");
                var newsChan = (INewsChannel)newsChannel;
                var messages = await newsChan.GetMessagesAsync(25).FlattenAsync();
                foreach (var message in messages)
                {
                    var usermsg = (IUserMessage)message;
                    await usermsg.CrosspostAsync();
                }
            }
        }

        internal static void UnmuteCheck(SocketGuildChannel channel)
        {
            var server = Botsettings.GetServer(channel.Guild.Id);
            if (server.Mutes != null && server.Mutes.Count > 0)
            {
                // For each mute with a duration...
                foreach (var mute in server.Mutes.Where(x => x.Duration > 0))
                {
                    var unmuteTime = DateTime.Parse(mute.CreatedAt).AddMinutes(mute.Duration);
                    var doUnmute = DateTime.Now.CompareTo(unmuteTime);
                    // If mute has expired...
                    if (doUnmute > 0)
                    {
                        // If user is in the server...
                        if (channel.Guild.Users.Any(x => x.Id.Equals(mute.UserID)))
                        {
                            // Unmute user and remove warn
                            var user = channel.Guild.Users.First(x => x.Id.Equals(mute.UserID));
                            var unmuteChannel = (ITextChannel)channel;
                            // Announce in #general if specified, otherwise last messaged channel
                            if (channel.Guild.Channels.Any(x => x.Id.Equals(server.Channels.General)))
                                unmuteChannel = (ITextChannel)channel.Guild.Channels.First(x => x.Id.Equals(server.Channels.General));
                            Moderation.Unmute(user, Moderation.GetUser(channel.Guild.Id, Program.client.CurrentUser.Id), unmuteChannel);
                        }
                        // Remove mute and update settings
                        server.Mutes.Remove(mute);
                        Botsettings.UpdateServer(server);
                    }
                }
            }
        }

        internal static void UnlockCheck(SocketGuildChannel channel)
        {
            var server = Botsettings.GetServer(channel.Guild.Id);
            if (server.Locks != null && server.Locks.Count > 0)
            {
                // For each mute with a duration...
                foreach (var channelLock in server.Locks.Where(x => x.Duration > 0))
                {
                    var unlockTime = DateTime.Parse(channelLock.CreatedAt).AddMinutes(channelLock.Duration);
                    var doUnlock = DateTime.Now.CompareTo(unlockTime);
                    // If lock has expired...
                    if (doUnlock > 0)
                    {
                        // If channel is in the server...
                        if (channel.Guild.Channels.Any(x => x.Id.Equals(channelLock.ChannelID)))
                        {
                            // Unlock channel and announce
                            var lockedChannel = channel.Guild.TextChannels.First(x => x.Id.Equals(channelLock.ChannelID));
                            Moderation.Unlock(Moderation.GetUser(channel.Guild.Id, Program.client.CurrentUser.Id), lockedChannel);
                        }
                        // Remove mute and update settings
                        server.Locks.Remove(channelLock);
                        Botsettings.UpdateServer(server);
                    }
                }
            }
        }
    }
}
