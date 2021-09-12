using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrostBot.Config;

namespace FrostBot
{
    public class Components
    {
        public static MessageProperties GetProperties(SocketMessageComponent interaction)
        {
            var guildChannel = (IGuildChannel)interaction.Channel;
            var guild = (SocketGuild)guildChannel.Guild;
            var user = (SocketGuildUser)interaction.User;
            var msgProps = new MessageProperties();
            var selectedServer = Program.settings.Servers.First(x => x.Id.Equals(guild.Id));

            switch (interaction.Data.CustomId)
            {
                // New Setup => Moderator Roles
                case "setup-new":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("**Please choose all \"Moderator\" Roles.**\n\n" +
                            "All users with these roles will be able to use exclusive server/bot management commands.\n" +
                            "For instance, creating opt-in roles, kicking users, changing bot settings...",
                            0x4A90E2, guildChannel.GuildId);
                    // Show list of non-mod roles in component as dropdown menu
                    var menu = new SelectMenuBuilder() { CustomId = "select-moderators" };
                    foreach (var role in guild.Roles.Where(x => !x.IsEveryone && !Program.settings.Servers.First(w => w.Id.Equals(guild.Id)).Roles.Any(y => y.Id.Equals(x.Id))))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = role.Name, Value = role.Id.ToString() });
                    // Show menu if there's at least 1 role to select
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithSelectMenu(menu).WithButton("Next", 
                            "setup-channel").WithButton("End Setup", "setup-complete").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("Next",
                            "setup-channel").WithButton("End Setup", "setup-complete").Build();
                    break;
                // Moderator Roles => Choose Role
                case "select-moderators":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    // Add role to moderators in config
                    var modRole = new Role { Name = guild.Roles.Where(x => x.Id.Equals(Convert.ToUInt64(interaction.Data.Values.First()))).First().Name,
                        Id = Convert.ToUInt64(interaction.Data.Values.First()), Moderator = true, CanCreateColors = true, 
                        CanCreateRoles = true, CanPin = true, IsVerifiedRole = true };
                    Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Roles.Add(modRole);
                    // Show selected role in embed
                    msgProps.Embed = Embeds.ColorMsg($"**Added \"{modRole.Name}\" as a Moderator Role**.\n\n" +
                        "Choose another role to add as a moderator, or press Next to continue.",
                            0x4A90E2, guildChannel.GuildId);
                    // Show list of non-mod roles in component as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = "select-moderators" };
                    foreach (var role in guild.Roles.Where(x => !x.IsEveryone && !Program.settings.Servers.First(w => w.Id.Equals(guild.Id)).Roles.Any(y => y.Id.Equals(x.Id))))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = role.Name, Value = role.Id.ToString() });
                    // Show menu if there's at least 1 role to select
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithSelectMenu(menu).WithButton("Next",
                            "setup-channel").WithButton("End Setup", "setup-complete").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("Next",
                            "setup-channel").WithButton("End Setup", "setup-complete").Build();
                    break;
            }

            // Save changes to config
            Botsettings.Save(Program.settings);

            return msgProps;
        }

        public static async void BeginSetup(IMessageChannel channel)
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
    }
}
