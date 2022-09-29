using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static Discord.Color GetDiscordColor(string hexColor)
        {
            return new Discord.Color(uint.Parse(hexColor.TrimStart('#'), NumberStyles.HexNumber));
        }

        public static string GetHexColor(Color color)
        {
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public static Embed Build(Color color, string title = "", string desc = "", string foot = "", string url = "", 
            string imgUrl = "", List<Tuple<string, string>> fields = null,
            string authorName = "", string authorUrl = "", string authorImgUrl = "")
        {
            var builder = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(desc)
            .WithFooter(foot)
            .WithUrl(url)
            .WithColor(color)
            .WithImageUrl(imgUrl)
            .WithAuthor(new EmbedAuthorBuilder() { Name = authorName, IconUrl = authorImgUrl, Url = authorUrl });
            if (fields != null)
                foreach (var field in fields)
                    builder = builder.AddField(field.Item1, field.Item2);

            return builder.Build();
        }
    }
}
