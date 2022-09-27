using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace FrostBot
{
    public class InfoModule : ModuleBase
    {
        [Command("say"), Summary("Make the bot repeat a message.")]
        public async Task Say([Remainder, Summary("The text to repeat.")] string message)
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(message);
        }

        [Command("forum"), Summary("Make the bot sync the forum.")]
        public async Task ForumSync()
        {
            await Context.Message.DeleteAsync();
            Phpbb.ForumSync();
        }
    }
}
