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

namespace JackFrostBot
{
    public class Processing
    {
        //Log each message and url that comes in
        public static async Task LogSentMessage(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;

            //Create txt if it doesn't exist
            string logPath = $"Servers//{guild.Id}//Log.txt";
            if (!File.Exists(logPath))
                File.Create(logPath);

            string logLine = ($"<{DateTime.Now.ToString("hh:mm")}> {user.Username} in #{message.Channel}: {message}");
            if (message.Attachments.Count > 0)
                logLine = logLine + message.Attachments.FirstOrDefault().Url;
            Console.WriteLine(logLine);
            File.AppendAllText(logPath, logLine + "\n");
        }

        //Write info to both the console and the log txt file
        public static void LogConsoleText(string text, ulong guildId)
        {
            //Create txt if it doesn't exist
            string logPath = $"Servers//{guildId}//Log.txt";
            if (!File.Exists(logPath))
                File.Create(logPath);

            string logLine = ($"<{DateTime.Now.ToString("hh:mm")}>: {text}");
            Console.WriteLine(logLine);
            File.AppendAllText(logPath, logLine + "\n");
        }

        //Write info on the last deleted message to a txt file
        public static async Task LogDeletedMessage(SocketMessage message, string reason)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;

            //Create txt if it doesn't exist
            string logPath = $"Servers//{guild.Id}//LastDeletedMsg.txt";
            if (!File.Exists(logPath))
                File.Create(logPath);

            LogConsoleText(reason, guild.Id);
            await message.DeleteAsync();
            File.WriteAllText(logPath, $"**Author**: {user.Username} ({user.Id})\n**Time**: {message.Timestamp}\n**Reason**: {reason}");
        }

        //Grant a user a role if they type the password in the verification channel
        public static async Task VerificationCheck(SocketMessage message)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;
            if (Setup.IsVerificationChannel((IGuildChannel)message.Channel)) 
            {
                if (message.Content.ToLower() == UserSettings.Verification.VerificationMessage(guild.Id).ToLower())
                {
                    await message.DeleteAsync();
                    ulong memberRoleId = UserSettings.Verification.MemberRoleID(guild.Id);
                    if (memberRoleId != 0)
                    {
                        var memberRole = user.Guild.GetRole(memberRoleId);
                        await user.AddRoleAsync(memberRole);
                        var embed = Embeds.LogMemberAdd(user);
                        var user2 = (SocketGuildUser)user;
                        var botlog = (ITextChannel)user2.Guild.GetChannel(UserSettings.Channels.BotLogsId(user.Guild.Id));
                        await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                    }
                    else
                        await message.Channel.SendMessageAsync("Attempted to give the new member a role, but it has to be configured first!");
                }
                else
                    await message.DeleteAsync();
            }
        }

        //Delete text posts in media only channel
        public static async Task MediaOnlyCheck(SocketMessage message)
        {
            if (Setup.IsMediaOnlyChannel((IGuildChannel)message.Channel) && 
                (message.Attachments.Count == 0 && !message.Content.Contains("http") && !message.Content.Contains("www")))
            {
                await LogDeletedMessage(message, "User tried to talk in a media-only channel.");
            }
        }

        // Check if a message is a duplicate and if so, delete it
        public static async Task DuplicateMsgCheck(SocketMessage message, SocketGuildChannel channel)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;
            if ((channel.Id != UserSettings.Channels.BotChannelId(guild.Id)) && (Moderation.IsModerator((IGuildUser)message.Author) == false) && (message.Author.IsBot == false))
            {
                int runs = 0;
                int matches = 0;
                foreach (IMessage msg in (await message.Channel.GetMessagesAsync(10).FlattenAsync()))
                {
                    if (((runs > 0) && (msg.Content.ToString() == message.Content.ToString()) && (msg.Author == message.Author) && (message.Attachments.Count == 0)))
                    {
                        matches++;
                        if (matches >= UserSettings.BotOptions.MaximumDuplicates(channel.Guild.Id))
                        {
                            await LogDeletedMessage(message, "Duplicate message");
                            if (UserSettings.BotOptions.AutoWarnDuplicates(channel.Guild.Id))
                                Moderation.Warn(channel.Guild.CurrentUser.Username, (ITextChannel)channel, (SocketGuildUser)message.Author, "Stop posting the same thing over and over.");
                        }
                    }
                    runs++;
                }
            }
        }

        public static async Task MsgLengthCheck(SocketMessage message, SocketGuildChannel channel)
        {
            var user = (IGuildUser)message.Author;
            var guild = user.Guild;
            if ((Moderation.IsModerator((IGuildUser)message.Author) == false) && (message.Author.IsBot == false))
            {
                //Delete Messages that are too short
                int minimum = UserSettings.BotOptions.MinimumLength(channel.Guild.Id);
                int msgLength = message.Content.ToString().Trim(new char[] { ' ', '.' }).Length;
                if (message.Content.Length < minimum && message.Attachments.Count == 0)
                    await LogDeletedMessage(message, $"Message was too short (minimum is {minimum})");
                int minletters = UserSettings.BotOptions.MinimumLetters(channel.Guild.Id);
                //Delete messages that don't have enough alphanumeric characters
                if (message.Content.Count(char.IsLetterOrDigit) < minletters && message.Tags.Count <= 0 && !Regex.IsMatch(message.Content, @"\p{Cs}") && message.Attachments.Count <= 0)
                    await LogDeletedMessage(message, $"Message didn't have enough letters (minimum is {minletters})");
            }
        }

        //Check if a message is filtered 
        public static async Task FilterCheck(SocketMessage message, SocketGuildChannel channel)
        {
            if (Convert.ToBoolean(UserSettings.Filters.Enabled(channel.Guild.Id)))
                foreach (string term in UserSettings.Filters.List(channel.Guild.Id))
                {
                    if (Regex.IsMatch(message.Content, string.Format(@"\b{0}\b|\b{0}d\b|\b{0}s\b|\b{0}ing\b|\b{0}ed\b|\b{0}er\b|\b{0}ers\b", Regex.Escape(term))))
                    {
                        string deleteReason = $"Message included a filtered term: {term}";

                        //Check if a message is clearly bypassing the filter, if so then warn
                        if (UserSettings.Filters.TermCausesWarn(term, channel.Guild.Id))
                            Moderation.Warn(channel.Guild.CurrentUser.Username, (ITextChannel)channel, (SocketGuildUser)message.Author, deleteReason);
                        await LogDeletedMessage(message, deleteReason);
                    }
                }
        }

        //Log replies and respond with a random reply
        public static async Task Markov(string message, SocketGuildChannel channel, int frequency)
        {
            string binFilePath = $"Servers\\{channel.Guild.Id.ToString()}\\{channel.Guild.Id.ToString()}";
            Markov mkv = new Markov();
            try { mkv.LoadChainState(binFilePath); }
            catch { }
            //Sanitize message content
            string newMessage = message;
            var links = newMessage.Split("\t\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("http://") || s.StartsWith("www.") || s.StartsWith("https://") || s.StartsWith("@"));
            foreach (var link in links)
                newMessage = newMessage.Replace(link, "");

            if (newMessage != "" && Moderation.IsPublicChannel(channel))
                mkv.Feed(newMessage);

            mkv.SaveChainState(binFilePath);

            var socketChannel = (ISocketMessageChannel)channel;
            Random rnd = new Random();
            string markov = MarkovGenerator.Create(mkv);
            int runs = 0;
            while (markov.Length < UserSettings.BotOptions.MarkovMinimumLength(channel.Guild.Id) && runs < 20)
            {
                markov = MarkovGenerator.Create(mkv);
                runs++;
            }

            if (rnd.Next(1, 100) <= frequency)
            {
                if (!UserSettings.BotOptions.MarkovBotChannelOnly(channel.Guild.Id))
                    await socketChannel.SendMessageAsync(markov);
                else if (channel.Id == UserSettings.Channels.BotChannelId(channel.Guild.Id))
                    await socketChannel.SendMessageAsync(markov);
            }

            return;
        }

    }
}
