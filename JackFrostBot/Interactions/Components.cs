﻿using Discord;
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

            // Add moderation/markov options to settings
            if (customID.StartsWith("btn-") || customID.StartsWith("select-"))
            {
                switch (customID)
                {
                    case "select-mutelevel":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).MuteLevel = Convert.ToInt32(interactionValue);
                        break;
                    case "select-kicklevel":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).KickLevel = Convert.ToInt32(interactionValue);
                        break;
                    case "select-banlevel":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).BanLevel = Convert.ToInt32(interactionValue);
                        break;
                    case "btn-deletedupes-disable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).AutoDeleteDupes = false;
                        break;
                    case "btn-deletedupes-enable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).AutoDeleteDupes = true;
                        break;
                    case "select-duplicatefreq":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).DuplicateFreq = Convert.ToInt32(interactionValue);
                        break;
                    case "select-maxdupes":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).MaxDuplicates = Convert.ToInt32(interactionValue);
                        break;
                    case "btn-warnondelete-disable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).WarnOnAutoDelete = false;
                        break;
                    case "btn-warnondelete-enable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).WarnOnAutoDelete = true;
                        break;
                    case "btn-wordfilter-disable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).WarnOnFilter = false;
                        break;
                    case "btn-wordfilter-enable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).WarnOnFilter = true;
                        break;
                    case "btn-markov-disable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).AutoMarkov = false;
                        break;
                    case "btn-markov-enable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).AutoMarkov = true;
                        break;
                    case "btn-markovbotchannel-disable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).BotChannelMarkovOnly = false;
                        break;
                    case "btn-markovbotchannel-enable":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).BotChannelMarkovOnly = true;
                        break;
                    case "select-markovfreq":
                        Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).MarkovFreq = Convert.ToInt32(interactionValue);
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
                // New Setup => Auto-Moderation (Mute Level)
                case "setup-mutelevel":
                case "setup-automod":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Mute)**__ (1/8)\n" +
                            $"With the **{selectedServer.Prefix}warn command**, moderators can notify users of rule infractions.\n" +
                            "These infractions are tracked and managed by the bot as **warns**. You can set automatic penalties\n" +
                            "for accumulating too many **warns** such as **muting, kicking or banning**.\n\n" +
                            "How many warns should result in a 🔇 **mute**? (0 to never auto-mute)\n" +
                            $"**Current setting**: {selectedServer.MuteLevel}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show menu between 0 and 10
                    menu = new SelectMenuBuilder() { CustomId = "select-mutelevel" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString(), Value = i.ToString() });
                        msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                            .WithSelectMenu(menu).WithButton("Next Step", "setup-kicklevel").Build();
                    break;
                case "setup-kicklevel":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Kick)**__ (2/8)\n" +
                            $"With the **{selectedServer.Prefix}warn command**, moderators can notify users of rule infractions.\n" +
                            "These infractions are tracked and managed by the bot as **warns**. You can set automatic penalties\n" +
                            "for accumulating too many **warns** such as **muting, kicking or banning**.\n\n" +
                            "How many warns should result in a 👢 **kick**? (0 to never auto-kick)\n" +
                            $"**Current setting**: {selectedServer.KickLevel}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show menu between 0 and 10
                    menu = new SelectMenuBuilder() { CustomId = "select-kicklevel" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString(), Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithSelectMenu(menu).WithButton("Next Step", "setup-banlevel").Build();
                    break;
                case "setup-banlevel":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Ban)**__ (3/8)\n" +
                            "With the **{selectedServer.Prefix}warn command**, moderators can notify users of rule infractions.\n" +
                            "These infractions are tracked and managed by the bot as **warns**. You can set automatic penalties\n" +
                            "for accumulating too many **warns** such as **muting, kicking or banning**.\n\n" +
                            "How many warns should result in a 🔨 **ban**? (0 to never auto-ban)\n" +
                            $"**Current setting**: {selectedServer.BanLevel}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show menu between 0 and 10
                    menu = new SelectMenuBuilder() { CustomId = "select-banlevel" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString(), Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithSelectMenu(menu).WithButton("Next Step", "setup-deletedupes").Build();
                    break;
                case "setup-deletedupes":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Duplicates)**__ (4/8)\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "Would you like to **enable this feature?**",
                            0x4A90E2, guildChannel.GuildId);
                    cmdToggles = new ComponentBuilder();
                    unchk = new Emoji("🟦");
                    chk = new Emoji("☑️");
                    if (selectedServer.AutoDeleteDupes)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"btn-deletedupes-disable", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"btn-deletedupes-enable", ButtonStyle.Primary, unchk);
                    nextOptionName = "setup-maxdupes";
                    if (!selectedServer.AutoDeleteDupes)
                        nextOptionName = "setup-wordfilter";
                    msgProps.Components = cmdToggles.WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-dupefreq").Build();
                    break;
                case "setup-dupefreq":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Duplicates)**__ (5/8)\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "**How close together (in seconds)** should identical messages\n" +
                            "be in order to be considered duplicates? (0 for always)\n" +
                            $"**Current setting**: {selectedServer.DuplicateFreq}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show menu between 0 and 10
                    menu = new SelectMenuBuilder() { CustomId = "select-duplicatefreq" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString(), Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithSelectMenu(menu).WithButton("Next Step", "setup-maxdupes").Build();
                    break;
                case "setup-maxdupes":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Duplicates)**__ (6/8)\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "How many identical messages in succession should be\n" +
                            "considered duplicates? (0 for any)\n" +
                            $"**Current setting**: {selectedServer.MaxDuplicates}",
                            0x4A90E2, guildChannel.GuildId);
                    // Show menu between 0 and 10
                    menu = new SelectMenuBuilder() { CustomId = "select-maxdupes" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString(), Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder().WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithSelectMenu(menu).WithButton("Next Step", "setup-warnondelete").Build();
                    break;
                case "setup-warnondelete":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Duplicates)**__ (7/8)\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "Should it also issue an **automatic warn** when this occurs?",
                            0x4A90E2, guildChannel.GuildId);
                    cmdToggles = new ComponentBuilder();
                    unchk = new Emoji("🟦");
                    chk = new Emoji("☑️");
                    if (selectedServer.WarnOnAutoDelete)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"btn-warnondelete-disable", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"btn-warnondelete-enable", ButtonStyle.Primary, unchk);
                    msgProps.Components = cmdToggles.WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-wordfilter").Build();
                    break;
                case "setup-wordfilter":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Automatic Moderation (Word Filter)**__ (8/8)\n" +
                            "The bot can automatically detect and delete messages containing **filtered terms**.\n\n" +
                            "You will need to add these terms to ``settings.yml`` yourself.\n" +
                            "Would you like to issue an **automatic warn** when the filter is tripped?",
                            0x4A90E2, guildChannel.GuildId);
                    cmdToggles = new ComponentBuilder();
                    unchk = new Emoji("🟦");
                    chk = new Emoji("☑️");
                    if (selectedServer.WarnOnFilter)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"btn-wordfilter-disable", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"btn-wordfilter-enable", ButtonStyle.Primary, unchk);
                    msgProps.Components = cmdToggles.WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-markov").Build();
                    break;
                case "setup-markov":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Markov**__ (1/3)\n" +
                            "The bot can use incoming messages to build a **markov** dictionary.\n" +
                            "This allows it to respond with randomly generated sentences built from\n" +
                            "words sent by your server's users.\n\n" +
                            "These are pulled from public channels only and won't include @mentions,\n" +
                            "hyperlinks or any filtered terms. Would you like to **enable this feature**?",
                            0x4A90E2, guildChannel.GuildId);
                    cmdToggles = new ComponentBuilder();
                    unchk = new Emoji("🟦");
                    chk = new Emoji("☑️");
                    if (selectedServer.WarnOnAutoDelete)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"btn-markov-disable", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"btn-markov-enable", ButtonStyle.Primary, unchk);
                    msgProps.Components = cmdToggles.WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-markovbotchannel").Build();
                    break;
                case "setup-markovbotchannel":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Markov**__ (2/3)\n" +
                            "By default, **markov messages** are posted in response to random messages\n" +
                            "in public channels. Would you like to **restrict this to only the bot sandbox channel**?\n\n" +
                            "Don't worry, if you don't want random messages posted at all, you can set the frequency\n" +
                            $"to zero in the next step so that the **{selectedServer.Prefix}markov command** must be used instead.",
                            0x4A90E2, guildChannel.GuildId);
                    cmdToggles = new ComponentBuilder();
                    unchk = new Emoji("🟦");
                    chk = new Emoji("☑️");
                    if (selectedServer.WarnOnAutoDelete)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"btn-markovbotchannel-disable", ButtonStyle.Primary, chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"btn-markovbotchannel-enable", ButtonStyle.Primary, unchk);
                    msgProps.Components = cmdToggles.WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-markovfreq").Build();
                    break;
                case "setup-markovfreq":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    msgProps.Embed = Embeds.ColorMsg("__**Markov**__ (3/3)\n" +
                            "**How often** would you like the bot to respond to an incoming message\n" +
                            "with a **markov message**?\n" +
                            $"Note: The **{selectedServer.Prefix}markov command** will always" +
                            "\nsend a message 100% of the time.\n" +
                            $"**Current setting**: {selectedServer.MarkovFreq}% (0 = never)",
                            0x4A90E2, guildChannel.GuildId);
                    menu = new SelectMenuBuilder() { CustomId = "select-markovfreq" };
                    for (int i = 0; i < 105; i = i + 5)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString() + "%", Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder().WithButton("Complete Setup", "setup-complete", ButtonStyle.Primary).Build();
                    break;
                case "setup-complete":
                    // Ignore if user is not a moderator
                    if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                        return msgProps;
                    Program.settings.Servers.First(x => x.Id.Equals(guild.Id)).Configured = true;
                    msgProps.Embed = Embeds.ColorMsg("__**Setup Complete!**__\n" +
                            "Everything should be all set for you to start using the bot.\n" +
                            $" Use ``{selectedServer.Prefix}help`` in the bot sandbox channel\n" +
                            "for a list of commands you can use.",
                            0x37FF68, guildChannel.GuildId);
                    msgProps.Components = new ComponentBuilder().Build();
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

            if (!selectedServer.Configured)
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
                    .WithButton("Markov", "setup-markov");
            }

            await channel.SendMessageAsync(embed: embed, component: builder.Build());
        }
    }
}
