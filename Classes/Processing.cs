using ClutteredMarkov;
using Discord;
using Discord.WebSocket;
using ShrineFox.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FrostBot
{
    public class Processing
    {
        public static string CreateMarkovString(Server server, int length = 0)
        {
            string markov = "";
            string markovBin = Path.Combine(Path.Combine(Path.Combine(Exe.Directory(), "Servers"), server.ServerID), server.MarkovFileName);
            if (!Directory.Exists(Path.GetDirectoryName(markovBin)))
                Directory.CreateDirectory(Path.GetDirectoryName(markovBin));

            // Create markov object, attempt to load from path
            Markov mkv = new Markov();
            if (!File.Exists(markovBin + ".bin"))
            {
                mkv.Feed("hee-ho");
                mkv.SaveChainState(markovBin);
            }
            else
                mkv.LoadChainState(markovBin);

            if (length == 0)
                length = server.AutoMarkovLength;

            // Create new markov string from data so far
            markov = MarkovGenerator.Create(mkv);
            // Try to get string longer than character limit (up to 10 tries)
            int runs = 0;
            while (string.IsNullOrEmpty(markov) || (markov.Length < length && runs < 10))
            {
                markov = MarkovGenerator.Create(mkv);
                runs++;
            }
            if (string.IsNullOrEmpty(markov))
                return "hee-ho";

            // Embed emotes when possible
            if (markov.Contains(":"))
            {
                var matches = Regex.Matches(markov, @":(.+?):");
                foreach (var guild in Program.client.Guilds.Where(x => x.Id == Convert.ToUInt64(server.ServerID)))
                {
                    foreach (var match in matches)
                    {
                        string name = match.ToString().Replace(":", "");
                        if (guild.Emotes.Any(x => x.Name == name))
                            markov = markov.Replace(match.ToString(), $"{guild.Emotes.First(x => x.Name == name)}");
                    }
                }
            }

            return markov;
        }

        public static void FeedMarkovString(Server server, string input)
        {
            string markovBin = Path.Combine(Path.Combine(Path.Combine(Exe.Directory(), "Servers"), server.ServerID), server.MarkovFileName);
            if (!Directory.Exists(Path.GetDirectoryName(markovBin)))
                Directory.CreateDirectory(Path.GetDirectoryName(markovBin));

            // Create markov object, attempt to load from path
            Markov mkv = new Markov();
            if (!File.Exists(markovBin + ".bin"))
            {
                mkv.Feed("hee-ho");
                mkv.SaveChainState(markovBin);
            }
            else
                mkv.LoadChainState(markovBin);

            mkv.Feed(input);
            mkv.SaveChainState(markovBin);
        }

        public static async Task SendToBotLogs(SocketGuild guild, string text, Discord.Color color, IUser user = null)
        {
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(guild.Id.ToString()));
            if (!string.IsNullOrEmpty(serverSettings.BotLogChannel.ID))
            {
                var botLogs = guild.GetTextChannel(Convert.ToUInt64(serverSettings.BotLogChannel.ID));
                if (user != null)
                    await botLogs.SendMessageAsync(embed: Embeds.Build(color,
                    desc: text, authorName: user.Username, authorImgUrl: user.GetAvatarUrl()));
                else
                    await botLogs.SendMessageAsync(embed: Embeds.Build(color,
                    desc: text));
            }
            
            await Task.CompletedTask;
        }
    }
}
