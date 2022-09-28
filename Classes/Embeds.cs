using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostBot
{
    class Embeds
    {

        public static uint GetRoleColor(SocketRole role)
        {
            return (uint)((role.Color.R << 16) | (role.Color.G << 8) | role.Color.B);
        }

        public static uint GetRoleColor(IRole role)
        {
            return (uint)((role.Color.R << 16) | (role.Color.G << 8) | role.Color.B);
        }

        public static Embed Build(string title = "", string desc = "", string foot = "", string url = "", List<Tuple<string, string>> fields = null)
        {
            var builder = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(desc)
            .WithFooter(foot)
            .WithUrl(url)
            .WithColor(new Color(0xD0021B));
            if (fields != null)
                foreach (var field in fields)
                    builder = builder.AddField(field.Item1, field.Item2);

            return builder.Build();
        }
    }
}
