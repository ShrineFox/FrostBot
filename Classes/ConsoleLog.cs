using Discord;
using Discord.WebSocket;
using ShrineFox.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostBot
{
    class ConsoleLog
    {
        public static string String(IGuildUser user, string location, string message)
        {
            return $"{user.DisplayName} ({user.Username}#{user.Discriminator}) in {location} {message}";
        }

        public async Task Message(SocketMessage message)
        {
            IGuildUser user = (IGuildUser)message.Author;
            string logLine = ConsoleLog.String(user, $"\"{user.Guild.Name}\" #{message.Channel}", $": {message}");
            // Include attachment URL
            if (message.Attachments.Count > 0)
            {
                logLine += "\n\tAttachments:";
                foreach (var attachment in message.Attachments)
                    logLine += $"\n\t\t{attachment.Url}";
            }
            Output.Log(logLine);

            await Task.CompletedTask;
            return;
        }

        private Task Log(LogMessage msg)
        {
            Output.Log(msg.Message, ConsoleColor.DarkGray);
            if (msg.Exception != null)
                LogException(msg.Exception);

            return Task.CompletedTask;
        }

        private void LogException(Exception exception)
        {
            if (exception.Message.Contains("WebSocket connection was closed"))
                Output.Log("Connection Lost", ConsoleColor.Red);
            else
                Output.Log(exception.Message, ConsoleColor.Red);
        }
    }
}
