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
            var interactionValue = "";
            if (interaction.Data.Values != null && interaction.Data.Values.Count > 0)
                interactionValue = interaction.Data.Values.First();

            string customID = interaction.Data.CustomId;

            // Add selected channel ID to settings
            if (customID.StartsWith("select-") && customID.EndsWith("-channel"))
            {
                string channelName = customID.Replace("select-","").Replace("-channel","");
                switch (channelName)
                {
                    case "general":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Channels.General = Convert.ToUInt64(interactionValue);
                        break;
                    case "botsandbox":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Channels.BotSandbox = Convert.ToUInt64(interactionValue);
                        break;
                    case "botlogs":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Channels.BotLogs = Convert.ToUInt64(interactionValue);
                        break;
                    case "pins":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Channels.Pins = Convert.ToUInt64(interactionValue);
                        break;
                }

                // Save changes to config
                Botsettings.Save(Program.settings);
            }

            // Toggle command setting
            if (customID.StartsWith("cmd-"))
            {
                var splitCustomID = customID.Split('-');
                bool toggle = true;
                switch(splitCustomID[2])
                {
                    case "enabled":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Commands.First(x => x.Name.Equals(splitCustomID[1])).Enabled = Convert.ToBoolean(splitCustomID[3]);
                        break;
                    case "moderatorsonly":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Commands.First(x => x.Name.Equals(splitCustomID[1])).ModeratorsOnly = Convert.ToBoolean(splitCustomID[3]);
                        break;
                    case "botchannelonly":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Commands.First(x => x.Name.Equals(splitCustomID[1])).BotChannelOnly = Convert.ToBoolean(splitCustomID[3]);
                        break;
                    case "isslashcmd":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Commands.First(x => x.Name.Equals(splitCustomID[1])).IsSlashCmd = Convert.ToBoolean(splitCustomID[3]);
                        break;
                }

                // Save changes to config
                Botsettings.Save(Program.settings);

                // Return to command config
                customID = "select-command";
                if (splitCustomID.Length == 5)
                    customID += "-2";
                interactionValue = splitCustomID[1];
            }

            var selectedServer = Program.settings.Servers.First(x => x.Id.Equals(guild.Id));
            switch (customID)
            {
                // New Setup => Moderator Roles
                case "setup-moderators":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Moderator Roles**__ (1/2)\n" +
                            "Please choose a \"Moderator\" Role.\n\n" +
                            "Users with this role will be able to use exclusive server/bot management commands.\n" +
                            "For instance, creating opt-in roles, kicking users, changing bot settings...\n\n" +
                            "You may choose multiple roles, or a new create one.\n" +
                            "Due to limitations, only the first 25 are shown.",
                            0x4A90E2, guildChannel.GuildId);
                    // Show list of non-mod roles in component as dropdown menu
                    var menu = new SelectMenuBuilder() { CustomId = "select-moderators" };
                    foreach (var role in guild.Roles.Where(x => !x.IsEveryone && !Program.settings.Servers.First(w => w.Id.Equals(guild.Id)).Roles.Any(y => y.Id.Equals(x.Id))))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = role.Name, Value = role.Id.ToString() });
                    // Show menu if there's at least 1 role to select
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Create New Role", "create-moderatorrole", ButtonStyle.Success).WithSelectMenu(menu).WithButton("Skip", "setup-channels").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Create New Role", "create-moderatorrole", ButtonStyle.Success).WithButton("Next Step", "setup-channels").Build();
                    break;
                // Moderator Roles => Choose Role
                case "select-moderators":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    // Add role to moderators in config
                    var modRole = new Role { Name = guild.Roles.Where(x => x.Id.Equals(Convert.ToUInt64(interactionValue))).First().Name,
                        Id = Convert.ToUInt64(interactionValue), Moderator = true, CanCreateColors = true, 
                        CanCreateRoles = true, CanPin = true };
                    Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Roles.Add(modRole);
                    // Show selected role in embed
                    msgProps.Embed = Embeds.ColorMsg($"__**Moderator Roles**__ (2/2)\n" + 
                        "**Successfully added \"{modRole.Name}\" as a Moderator Role**.\n\n" +
                        "Choose another role to add as a moderator, or press **Next Step** to continue.",
                            0x37FF68, guildChannel.GuildId);
                    // Show list of non-mod roles in component as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = "select-moderators" };
                    foreach (var role in guild.Roles.Where(x => !x.IsEveryone && !Program.settings.Servers.First(w => w.Id.Equals(guild.Id)).Roles.Any(y => y.Id.Equals(x.Id))).Take(25))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = role.Name, Value = role.Id.ToString() });
                    // Show menu if there's at least 1 role to select
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Create New Role", "create-moderatorrole", ButtonStyle.Success).WithSelectMenu(menu).WithButton("Next Step", "setup-channels").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Create New Role", "create-moderatorrole", ButtonStyle.Success).WithButton("Next Step", "setup-channels").Build();
                    break;
                // Setup Channels => General
                case "setup-general-channel":
                case "setup-channels":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Channels**__ (1/4)\n" +
                            "Please choose a **\"General\" channel.**\n\n" +
                            "This should be a public channel where custom messages appear\n" +
                            "when a new user joins the server.\n\n" +
                            $"**Currently Selected**: {selectedServer.Channels.General}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show list of unselected public channels in component as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = "select-general-channel" };
                    foreach (var channel in guild.TextChannels.Take(25))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = channel.Name, Value = channel.Id.ToString() });
                    // Show menu if there's at least 1 channel to select
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithSelectMenu(menu).WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Skip", "setup-botsandbox-channel").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Next Step","setup-botsandbox-channel").Build();
                    break;
                // General => BotSandbox
                case "setup-botsandbox-channel":
                case "select-general-channel":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Channels**__ (2/4)\n" +
                            "**Please choose a \"Bot Sandbox\" channel.**\n\n" +
                            "This is a public channel where users are encouraged to use bot commands.\n" +
                            "Later, you can set certain commands to only work in this channel.\n\n" +
                            $"**Currently Selected**: {selectedServer.Channels.BotSandbox}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show list of unselected public channels in component as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = "select-botsandbox-channel" };
                    foreach (var channel in guild.TextChannels.Take(25))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = channel.Name, Value = channel.Id.ToString() });
                    // Show menu if there's at least 1 channel to select
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithSelectMenu(menu).WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Skip", "setup-botlogs-channel").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Next Step", "setup-botlogs-channel").Build();
                    break;
                // BotSandbox => BotLogs
                case "setup-botlogs-channel":
                case "select-botsandbox-channel":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Channels**__ (3/4)\n" +
                            "**Please choose a \"Bot Logs\" channel.**\n\n" +
                            "This is a **staff-only** channel that serves as an audit log for moderation commands.\n" +
                            "i.e. Who banned what user in what channel, at what time and for what reason.\n\n" +
                            "**This channel must have View Messages disabled for @everyone**." +
                            $"**Currently Selected**: {selectedServer.Channels.BotLogs}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show list of unselected private channels in component as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = "select-botlogs-channel" };
                    foreach (var channel in guild.TextChannels.Take(25))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = channel.Name, Value = channel.Id.ToString() });
                    // Show menu if there's at least 1 channel to select
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithSelectMenu(menu).WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Skip", "setup-pins-channel").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Next Step", "setup-pins-channel").Build();
                    break;
                // BotLogs => Pins
                case "setup-pins-channel":
                case "select-botlogs-channel":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Channels**__ (4/4)\n" +
                            "**Please choose a \"Pins\" channel.**\n\n" +
                            "This is a public channel where messages \"pinned\" with a bot command will go.\n" +
                            "This feature is intended as a workaround to the 50 message pin limit on channels.\n\n" +
                            $"**Currently Selected**: {selectedServer.Channels.Pins}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show list of unselected public channels in component as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = "select-pins-channel" };
                    foreach (var channel in guild.TextChannels.Take(25))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = channel.Name, Value = channel.Id.ToString() });
                    // Show menu
                    if (menu.Options.Count() > 0)
                        msgProps.Components = new ComponentBuilder().WithSelectMenu(menu).WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Next Step", "setup-commands").Build();
                    else
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithButton("Next Step", "setup-commands").Build();
                    break;
                // Pins => Commands
                case "setup-commands":
                case "setup-commands-2":
                case "select-pins-channel":
                    string additional = "";
                    if (customID.EndsWith("-2"))
                        additional = "-2";
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    // Show a different message depending on page of commands
                    string embedMsg = "__**Commands**__ (1/2)\n" +
                        "Choose more **commands** to configure.\n\n" +
                        "For each command, you can toggle the following:\n" +
                        "**Enabled**: Can be used at all.\n" +
                        "**ModeratorsOnly**: Can be used only by moderators.\n" +
                        "**BotChannelOnly**: Can be used only in the bot sandbox channel.\n" +
                        "**IsSlashCmd**: Can be used as a slash command.\n\n" +
                        "By default, these are all true.";
                    if (customID.EndsWith("-2"))
                        embedMsg = "__**Commands**__ (2/2)\n" +
                        "Choose more **commands** to configure.\n\n" +
                        "These ones are different than the last set.\n" +
                        "Due to Discord limitations, only 25 can be shown at a time.";
                    msgProps.Embed = Embeds.ColorMsg(embedMsg, 0x4A90E2, guildChannel.GuildId);
                    // Show list of commands as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = $"select-command{additional}" };
                    var commands = new List<Command>();
                    if (customID.EndsWith("-2"))
                        commands = selectedServer.Commands.Skip(25).ToList();
                    else
                        commands = selectedServer.Commands.Take(25).ToList();
                    foreach (var command in commands)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = command.Name, Value = command.Name });
                    // Show menu
                    string nextOptionName = "setup-commands-2";
                    if (customID == nextOptionName)
                        nextOptionName = "setup-automod";
                    msgProps.Components = new ComponentBuilder().WithSelectMenu(menu).WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", nextOptionName).Build();
                    break;
                // Commands => Selected Command
                case "select-command":
                case "select-command-2":
                    // Decide what set of commands to show
                    additional = "";
                    if (customID.EndsWith("-2"))
                        additional = "-2";
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                    return msgProps;
                    var cmd = selectedServer.Commands.First(x => x.Name.Equals(interactionValue));
                    string summary = Program.commands.Modules.Where(m => m.Parent == null).First(x => x.Commands.Any(z => z.Name.Equals("say"))).Commands.First(y => y.Name.Equals(interactionValue)).Summary;
                    var parameters = Program.commands.Modules.Where(m => m.Parent == null).First(x => x.Commands.Any(z => z.Name.Equals("say"))).Commands.First(y => y.Name.Equals(interactionValue)).Parameters;
                    string paramList = "";
                    foreach (var param in parameters)
                        paramList += $" <{param.Name}>";
                    // Show selected command in embed
                    msgProps.Embed = Embeds.ColorMsg($"__**{selectedServer.Prefix}{cmd.Name} Command**__\n" +
                        $"**{selectedServer.Prefix}{cmd.Name}**{paramList}\n" +
                        $"**Description**: {summary}\n\n" +
                        "**Enabled**: Can be used at all.\n" +
                        "**ModeratorsOnly**: Can be used only by moderators.\n" +
                        "**BotChannelOnly**: Can be used only in the bot sandbox channel.\n" +
                        "**IsSlashCmd**: Can be used as a slash command.\n\n" +
                        "By default, these are all true.\n" +
                        "When finished, choose another command or press **Next Step**.",
                            0x4A90E2, guildChannel.GuildId);
                    // Show 
                    IEmote unchk = new Emoji("🟦");
                    IEmote chk = new Emoji("☑️");
                    var cmdToggles = new ComponentBuilder();
                    // Enabled
                    if (cmd.Enabled)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"cmd-{cmd.Name}-enabled-false{additional}", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Enabled", $"cmd-{cmd.Name}-enabled-true{additional}", ButtonStyle.Primary, unchk);
                    // ModeratorsOnly
                    if (cmd.ModeratorsOnly)
                        cmdToggles = cmdToggles.WithButton("ModeratorsOnly", $"cmd-{cmd.Name}-moderatorsonly-false{additional}", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("ModeratorsOnly", $"cmd-{cmd.Name}-moderatorsonly-true{additional}", ButtonStyle.Primary, unchk);
                    // BotChannelOnly
                    if (cmd.BotChannelOnly)
                        cmdToggles = cmdToggles.WithButton("BotChannelOnly", $"cmd-{cmd.Name}-botchannelonly-false{additional}", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("BotChannelOnly", $"cmd-{cmd.Name}-botchannelonly-true{additional}", ButtonStyle.Primary, unchk);
                    // IsSlashCmd
                    if (cmd.IsSlashCmd)
                        cmdToggles = cmdToggles.WithButton("IsSlashCmd", $"cmd-{cmd.Name}-isslashcmd-false{additional}", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("IsSlashCmd", $"cmd-{cmd.Name}-isslashcmd-true{additional}", ButtonStyle.Primary, unchk);
                    // Show list of commands as dropdown menu
                    menu = new SelectMenuBuilder() { CustomId = $"select-command{additional}" };
                    commands = new List<Command>();
                    if (customID.EndsWith("-2"))
                        commands = selectedServer.Commands.Skip(25).ToList();
                    else
                        commands = selectedServer.Commands.Take(25).ToList();
                    foreach (var command in commands)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = command.Name, Value = command.Name });
                    // Show menu
                    nextOptionName = "setup-commands-2";
                    if (customID == "select-command-2")
                        nextOptionName = "setup-automod";
                    msgProps.Components = cmdToggles.WithSelectMenu(menu).WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-commands-2").Build();
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
                builder = new ComponentBuilder().WithButton("Cancel", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Begin Setup", "setup-moderators");
            }
            else
            {
                embed = Embeds.ColorMsg("**Please have a Moderator complete this setup.**\n\n" +
                        "Thank you for choosing [FrostBot](https://github.com/ShrineFox/JackFrost-Bot) by [ShrineFox](https://github.com/ShrineFox)!\n\n" +
                        "Choose one of the options below to reconfigure.", 0x4A90E2, guildChannel.Guild.Id);
                builder = new ComponentBuilder()
                    .WithButton("Cancel", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Moderator Roles", "setup-moderators")
                    .WithButton("Channels", "setup-channels")
                    .WithButton("Commands", "setup-commands")
                    .WithButton("Auto-Moderation", "setup-automod")
                    .WithButton("Text Strings", "setup-strings")
                    .WithButton("Opt-In Roles", "setup-optin");
            }

            await channel.SendMessageAsync(embed: embed, component: builder.Build());
        }
    }
}
