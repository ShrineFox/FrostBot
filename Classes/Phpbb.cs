using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostBot
{
    class Phpbb
    {
        internal static Task ForumUpdate(SocketUserMessage message, SocketGuildChannel channel)
        {
            // If channel is a forum channel and a matching forum url is in the settings...
            // Log in as bot account
            // Reply to thread as bot

            return Task.CompletedTask;
        }

        internal static void ForumSync()
        {
            // Log in as bot account

            // For each forum channel in settings, get a list of threads
            // For each thread, get posts from forum
            // For each thread, get messages in thread
            // For each forum message, post in the channel if it's not already there
            // For each channel message, post on site's forum thread if it's not already there
            // If message has been edited on forum (different from cache), edit on discord
            // If message has been edited on discord (different from cache), edit on forum
        }
    }
}
