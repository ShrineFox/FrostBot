using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static FrostBot.Config;

namespace FrostBot
{
    class Setup
    {
        public static async void Begin(IMessageChannel channel)
        {
            var guildChannel = (IGuildChannel)channel;
            var selectedServer = Botsettings.SelectedServer(guildChannel.Guild.Id);

            var embed = Embeds.ColorMsg("", 0x0);
            var builder = new ComponentBuilder();

            if (selectedServer.Channels.General == 0)
            {
                embed = Embeds.ColorMsg("**FrostBot Setup**\n\n" +
                        "Thank you for choosing [FrostBot](https://github.com/ShrineFox/JackFrost-Bot) by [ShrineFox](https://github.com/ShrineFox)!\n" +
                        "You're ready to make moderating much more fun and easy for your server.\n" +
                        "**Someone with administrator privileges must initiate setup.**", 0x4A90E2, guildChannel.Guild.Id);
                builder = new ComponentBuilder()
                    .WithButton("Begin Setup", "setup-new");
            }
            else
            {
                embed = Embeds.ColorMsg("**Please have a Moderator complete this setup.**\n\n" +
                        "Thank you for choosing [FrostBot](https://github.com/ShrineFox/JackFrost-Bot) by [ShrineFox](https://github.com/ShrineFox)!\n\n" +
                        "Choose one of the options below to reconfigure.", 0x4A90E2, guildChannel.Guild.Id);
                builder = new ComponentBuilder()
                    .WithButton("Moderator Roles", "setup-moderator")
                    .WithButton("Channels", "setup-channel")
                    .WithButton("Commands", "setup-command")
                    .WithButton("Auto-Moderation", "setup-automod")
                    .WithButton("Text Strings", "setup-strings");
            }

            await channel.SendMessageAsync(embed: embed, component: builder.Build());
        }

        public static async void ModeratorRoles(IMessageChannel channel)
        {
            var guildChannel = (IGuildChannel)channel;

            var embed = Embeds.ColorMsg("", 
                    0x4A90E2, guildChannel.Guild.Id);
            var builder = new ComponentBuilder()
                .WithButton("label", "custom-id");

            await channel.SendMessageAsync("Here is a button!", component: builder.Build());
        }

        public static async void HandleSetupInteraction(SocketMessageComponent interaction)
        {
            switch (interaction.Data.CustomId)
            {
                case "setup-new":
                    await interaction.UpdateAsync(x =>
                    {
                        x.Content = $"**Please choose all Roles you would consider \"Moderator Roles\".**\n" +
                    "All users with these roles will be able to use exclusive server/bot management commands.";
                        x.Components = new ComponentBuilder().WithButton("label", "custom-id").Build();
                    });
                    break;
            }
        }
    }
}
