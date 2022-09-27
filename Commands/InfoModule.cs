using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;

namespace FrostBot
{
    public class InfoModule : ModuleBase
    {
        [SlashCommand("say", "Repeat a message.")]
        public async Task Say([Remainder, Discord.Interactions.Summary("The text to repeat.")] string message)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(message);
        }

        [Command("forum"), Discord.Commands.Summary("Make the bot sync the forum.")]
        public async Task ForumSync()
        {
            await Context.Message.DeleteAsync();
            Phpbb.ForumSync();
        }
    }
}
