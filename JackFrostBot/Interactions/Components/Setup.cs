using Discord;
using Discord.WebSocket;
using FrostBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrostBot.Config;

namespace FrostBot.Interactions
{
    public class Setup
    {
        public static MessageProperties Begin(SocketGuildUser user, SocketGuild guild, string customID, string interactionValue, Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();
            string[] customIDParts = customID.Split('-');
            if (customIDParts.Length >= 2)
            {
                // Narrow down type of setup interaction by component ID
                switch (customIDParts[1])
                {
                    case "begin":
                        msgProps = Start(selectedServer);
                        break;
                    case "moderators":
                        msgProps = Moderators(user, guild, customID, interactionValue, selectedServer);
                        break;
                    case "channels":
                        msgProps = Channels(user, guild, customID, interactionValue, selectedServer);
                        break;
                    case "commands":
                        msgProps = Commands(customID, interactionValue, selectedServer);
                        break;
                    case "automod":
                        msgProps = AutoMod(user, customID, interactionValue, selectedServer);
                        break;
                    case "markov":
                        msgProps = Markov(user, customID, interactionValue, selectedServer);
                        break;
                    case "complete":
                        msgProps = Complete(selectedServer);
                        break;
                }
            }

            return msgProps;
        }

        public static MessageProperties Start(Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();

            if (!selectedServer.Configured)
            {
                msgProps.Embed = Embeds.ColorMsg(
                        "☃️ __**FrostBot Setup**__\n\n" +
                        "Thank you for choosing [FrostBot](https://github.com/ShrineFox/JackFrost-Bot) '" +
                        "by [ShrineFox](https://github.com/ShrineFox)!\n" +
                        "You're almost ready to make moderating easier and more enjoyable for your server.\n" +
                        "**Someone with administrator privileges must initiate setup.**",
                        selectedServer.Id, 0, true);
                msgProps.Components = new ComponentBuilder()
                    .WithButton("Cancel", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Begin Setup", "setup-moderators")
                    .Build();
            }
            else
            {
                msgProps.Embed = Embeds.ColorMsg(
                    "☃️ __**FrostBot Setup**__\n\n" +
                        "**Please have a Moderator complete this setup.**\n" +
                        "Thank you for choosing [FrostBot](https://github.com/ShrineFox/JackFrost-Bot) " +
                        "by [ShrineFox](https://github.com/ShrineFox)!\n\n" +
                        "Choose one of the options below to reconfigure.",
                        selectedServer.Id, 0, true);
                msgProps.Components = new ComponentBuilder()
                    .WithButton("Moderator Roles", "setup-moderators")
                    .WithButton("Channels", "setup-channels-btn-general")
                    .WithButton("Commands", "setup-commands-btn-1")
                    .WithButton("Auto-Moderation", "setup-automod-btn-mutelevel")
                    .WithButton("Markov", "setup-markov-btn-enabled")
                    .WithButton("Cancel", "setup-complete", ButtonStyle.Danger)
                    .Build();
            }

            return msgProps;
        }

        private static MessageProperties Moderators(SocketGuildUser user, SocketGuild guild, string customID, string interactionValue, Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();
            // Add selected Moderator Role to settings and save
            if (customID == "setup-moderators-select")
            {
                var modRole = new Role
                {
                    Name = guild.Roles.Where(x => x.Id.Equals(Convert.ToUInt64(interactionValue))).First().Name,
                    Id = Convert.ToUInt64(interactionValue),
                    Moderator = true,
                    CanCreateColors = true,
                    CanCreateRoles = true,
                    CanPin = true
                };
                selectedServer.Roles.Add(modRole);
                IRole role = guild.GetRole(modRole.Id);
                if (role != null)
                    Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} added {role.Mention}** as a ⚔️ **Moderator Role** for the bot.", guild.Id), guild);
            }

            // Return Moderator selection prompt with list of currently selected modroles
            string modRoles = "";
            foreach (var modRole in selectedServer.Roles.Where(x => x.Moderator))
            {
                try
                {
                    IRole role = guild.GetRole(modRole.Id);
                    if (role != null)
                        modRoles += $"{role.Mention}\n";  
                }
                catch { }
            }
            msgProps.Embed = Embeds.ColorMsg(
                        "⚔️ __**Moderator Roles**__\n\n" +
                        "**Choose some \"Moderator\" Roles.**\n" +
                        "Users with the selected roles can **manage the server using special bot commands**.\n" +
                        "For instance, _creating opt-in roles, kicking users, changing bot settings..._\n\n" +
                        $"**Current selection:**\n{modRoles}\n" +
                        "Due to limitations, only the top 25 roles are shown.",
                        guild.Id, 0, true);
            // Show list of non-mod roles in component as dropdown menu
            var menu = new SelectMenuBuilder() { CustomId = "setup-moderators-select" };
            foreach (var role in guild.Roles.OrderBy(p => p.Position).Where(x => !x.IsEveryone && !selectedServer.Roles.Any(i => i.Id.Equals(x.Id))))
                menu.AddOption(new SelectMenuOptionBuilder() { Label = role.Name, Value = role.Id.ToString() });
            // Show menu if there's at least 1 role to select
            if (menu.Options.Count() > 0)
                msgProps.Components = new ComponentBuilder()
                    .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                    .WithSelectMenu(menu)
                    .WithButton("Skip", "setup-channels-btn-general")
                    .Build();
            else
                msgProps.Components = new ComponentBuilder()
                    .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Next Step", "setup-channels-btn-general")
                    .Build();

            Botsettings.UpdateServer(selectedServer);
            return msgProps;
        }

        private static MessageProperties Channels(SocketGuildUser user, SocketGuild guild, string customID, string interactionValue, Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();
            string[] customIDParts = customID.Split('-');
            string embedTxt = "";
            string selectedTxt = "";
            string nextBtnID = "";

            // Make sure there's at least 4 parts to avoid crash
            if (customIDParts.Length >= 4)
            {
                if (customIDParts[2] == "select" && (interactionValue != ""))
                {
                    // Set channel if menu item selected
                    switch (customIDParts[3])
                    {
                        case "general":
                            selectedServer.Channels.General = Convert.ToUInt64(interactionValue);
                            var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.General);
                            if (channel != null)
                                Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} selected {channel.Mention}** as the 👥 **General Channel** for the bot.", guild.Id), guild);
                            break;
                        case "botsandbox":
                            selectedServer.Channels.BotSandbox = Convert.ToUInt64(interactionValue);
                            channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.BotSandbox);
                            if (channel != null)
                                Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} selected {channel.Mention}** as the 👥 **Bot Sandbox Channel** for the bot.", guild.Id), guild);
                            break;
                        case "botlogs":
                            selectedServer.Channels.BotLogs = Convert.ToUInt64(interactionValue);
                            channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.BotLogs);
                            if (channel != null)
                                Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} selected {channel.Mention}** as the 👥 **Bot Logs Channel** for the bot.", guild.Id), guild);
                            break;
                        case "pins":
                            selectedServer.Channels.Pins = Convert.ToUInt64(interactionValue);
                            channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.Pins);
                            if (channel != null)
                                Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} selected {channel.Mention}** as the 👥 **Pins Channel** for the bot.", guild.Id), guild);
                            break;
                    }
                    Botsettings.UpdateServer(selectedServer);
                }
                // Display embed for specific channel
                switch (customIDParts[3])
                {
                    case "general":
                        embedTxt = "👥 __**Channels (General)**__ (1/4)\n\n" +
                        "Please choose a \"General\" channel.\n" +
                        "This should be a public channel where custom messages appear " +
                        "when a new user joins the server.";
                        nextBtnID = "setup-channels-btn-botsandbox";
                        if (selectedServer.Channels.General != 0)
                        {
                            try
                            {
                                var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.General);
                                if (channel != null)
                                    selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
                            }
                            catch { }
                        }
                        break;
                    case "botsandbox":
                        embedTxt = "👥 __**Channels (Bot Sandbox)**__ (2/4)\n" +
                            "**Please choose a \"Bot Sandbox\" channel.**\n" +
                            "This is a public channel where users are encouraged to use bot commands.\n" +
                            "Later, you can set certain commands to only work in this channel.";
                        nextBtnID = "setup-channels-btn-botlogs";
                        if (selectedServer.Channels.BotSandbox != 0)
                        {
                            try
                            {
                                var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.BotSandbox);
                                if (channel != null)
                                    selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
                            }
                            catch { }
                        }
                        break;
                    case "botlogs":
                        embedTxt = " 👥 __** Channels (Bot Logs)**__ (3/4)\n\n" +
                            "**Please choose a \"Bot Logs\" channel.**\n" +
                            "This is a **staff-only** channel that serves as an audit log for moderation commands.\n" +
                            "i.e. _Who banned what user in what channel, at what time and for what reason_.";
                        nextBtnID = "setup-channels-btn-pins";
                        if (selectedServer.Channels.BotLogs != 0)
                        {
                            try
                            {
                                var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.BotLogs);
                                if (channel != null)
                                    selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
                            }
                            catch { }
                        }
                        break;
                    case "pins":
                        embedTxt = "👥 __**Channels (Pins)**__ (4/4)\n\n" +
                            "**Please choose a \"Pins\" channel.**\n" +
                            "This is a public channel where messages \"pinned\" with a bot command will go.\n" +
                            "This feature is intended as a workaround to the 50 message pin limit on channels.";
                        nextBtnID = "setup-commands-btn-1";
                        if (selectedServer.Channels.Pins != 0)
                        {
                            try
                            {
                                var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.Pins);
                                if (channel != null)
                                    selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
                            }
                            catch { }
                        }
                        break;
                }
            }
            msgProps.Embed = Embeds.ColorMsg(embedTxt + selectedTxt +
                "\nNote: Only the first 25 channels are shown due to limitations.",
                guild.Id, 0, true);
            // Show list of unselected public channels in component as dropdown menu
            var menu = new SelectMenuBuilder() { CustomId = $"setup-channels-select-{customIDParts[3]}" };
            foreach (var channel in guild.TextChannels.OrderBy(p => p.Position).Take(25))
                menu.AddOption(new SelectMenuOptionBuilder() { Label = "#" + channel.Name, Value = channel.Id.ToString() });
            // Show menu if there's at least 1 channel to select
            if (nextBtnID == "")
                nextBtnID = "setup-commands-btn-1";
            if (menu.Options.Count() > 0)
                msgProps.Components = new ComponentBuilder()
                    .WithSelectMenu(menu)
                    .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Next Step", nextBtnID)
                    .Build();
            else
                msgProps.Components = new ComponentBuilder()
                    .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Next Step", nextBtnID)
                    .Build();

            return msgProps;
        }

        private static MessageProperties Commands(string customID, string interactionValue, Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();
            string[] customIDParts = customID.Split('-');
            bool page2 = false;
            string additional = "-1";

            var cmdToggles = new ComponentBuilder();
            var cmd = new Command();

            // Make sure there's at least 4 parts to avoid crash
            if (customIDParts.Length >= 4)
            {
                // Go to page 2 if selection or button ended with "-2"
                if (customIDParts[3] == "2")
                    page2 = true;
                // If menu option or button with command name in ID picked...
                if ((interactionValue != "" && customIDParts[2] == "select") || customIDParts.Length == 7)
                {
                    // For the menu item, get command by name to show options
                    if (interactionValue != "" && customIDParts[2] == "select")
                        cmd = selectedServer.Commands.First(x => x.Name.Equals(interactionValue));
                    else if (customIDParts.Length == 7)
                    {
                        // Get command by name
                        interactionValue = customIDParts[4];
                        cmd = selectedServer.Commands.First(x => x.Name.Equals(interactionValue));
                        // For the button press, toggle selection in settings
                        switch (customIDParts[5])
                        {
                            case "enabled":
                                selectedServer.Commands.First(x => x.Name.Equals(interactionValue)).Enabled = Convert.ToBoolean(customIDParts[6]);
                                break;
                            case "moderatorsonly":
                                selectedServer.Commands.First(x => x.Name.Equals(interactionValue)).ModeratorsOnly = Convert.ToBoolean(customIDParts[6]);
                                break;
                            case "botchannelonly":
                                selectedServer.Commands.First(x => x.Name.Equals(interactionValue)).BotChannelOnly = Convert.ToBoolean(customIDParts[6]);
                                break;
                            case "isslashcmd":
                                selectedServer.Commands.First(x => x.Name.Equals(interactionValue)).IsSlashCmd = Convert.ToBoolean(customIDParts[6]);
                                break;
                        }
                        // Update settings
                        Botsettings.UpdateServer(selectedServer);
                    }
                    // Show buttons
                    if (page2)
                        additional = "-2";
                    if (cmd.Enabled)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"setup-commands-btn{additional}-{cmd.Name}-enabled-false", ButtonStyle.Primary, Components.chk, null, false, 0);
                    else
                        cmdToggles = cmdToggles.WithButton("Enabled", $"setup-commands-btn{additional}-{cmd.Name}-enabled-true", ButtonStyle.Secondary, Components.unchk, null, false, 0);
                    // ModeratorsOnly
                    if (cmd.ModeratorsOnly)
                        cmdToggles = cmdToggles.WithButton("Moderators Only", $"setup-commands-btn{additional}-{cmd.Name}-moderatorsonly-false", ButtonStyle.Primary, Components.chk, null, false, 0);
                    else
                        cmdToggles = cmdToggles.WithButton("Moderators Only", $"setup-commands-btn{additional}-{cmd.Name}-moderatorsonly-true", ButtonStyle.Secondary, Components.unchk, null, false, 0);
                    // BotChannelOnly
                    if (cmd.BotChannelOnly)
                        cmdToggles = cmdToggles.WithButton("Bot Channel Only", $"setup-commands-btn{additional}-{cmd.Name}-botchannelonly-false", ButtonStyle.Primary, Components.chk, null, false, 0);
                    else
                        cmdToggles = cmdToggles.WithButton("Bot Channel Only", $"setup-commands-btn{additional}-{cmd.Name}-botchannelonly-true", ButtonStyle.Secondary, Components.unchk, null, false, 0);
                    // IsSlashCmd
                    if (cmd.IsSlashCmd)
                        cmdToggles = cmdToggles.WithButton("Is Slash Command", $"setup-commands-btn{additional}-{cmd.Name}-isslashcmd-false", ButtonStyle.Primary, Components.chk, null, false, 0);
                    else
                        cmdToggles = cmdToggles.WithButton("Is Slash Command", $"setup-commands-btn{additional}-{cmd.Name}-isslashcmd-true", ButtonStyle.Secondary, Components.unchk, null, false, 0);
                    // Create embed contents
                    string summary = Program.commands.Modules.Where(m => m.Parent == null).First(x => x.Commands.Any(z => z.Name.Equals("say"))).Commands.First(y => y.Name.Equals(interactionValue)).Summary;
                    var parameters = Program.commands.Modules.Where(m => m.Parent == null).First(x => x.Commands.Any(z => z.Name.Equals("say"))).Commands.First(y => y.Name.Equals(interactionValue)).Parameters;
                    string paramList = "";
                    foreach (var param in parameters)
                        paramList += $" <{param.Name}>";
                    // Show selected command info in embed
                    msgProps.Embed = Embeds.ColorMsg($"💬 __**{selectedServer.Prefix}{cmd.Name} Command**__\n\n" +
                        $"**Usage:** ``{selectedServer.Prefix}{cmd.Name}{paramList}``\n" +
                        $"**Description**: {summary}\n\n" +
                        "When finished, choose another command or press **Next Step**.",
                            selectedServer.Id, 0, true);
                }
                else if (customIDParts[2] == "btn" && customIDParts.Length == 4)
                {
                    // Show instructions without buttons if next/previous page button and no selected command
                    if (customIDParts.Length == 4)
                    {
                        if (!page2)
                            msgProps.Embed = Embeds.ColorMsg("💬 __**Commands**__ (1/2)\n\n" +
                                "Choose any **command** to configure.\n" +
                                "You can toggle how each command can be used.\n" +
                                "By default, commands only work for moderators, and must " +
                                "be used in the bot sandbox channel.",
                                selectedServer.Id, 0, true);
                        else
                            msgProps.Embed = Embeds.ColorMsg("💬 __**Commands**__ (2/2)\n\n" +
                                "Choose more **commands** to configure.\n\n" +
                                "These ones are different than the last set.\n" +
                                "Due to Discord limitations, only 25 can be shown at a time.",
                                selectedServer.Id, 0, true);
                    }
                }
            }
            // Menu with commands list
            var menu = new SelectMenuBuilder() { CustomId = $"setup-commands-select{additional}" };
            var commands = selectedServer.Commands;
            if (page2)
                commands = selectedServer.Commands.Skip(25).ToList();
            else
                commands = selectedServer.Commands.Take(25).ToList();
            foreach (var command in commands)
            {
                Console.WriteLine(command.Name);
                var desc = Program.commands.Modules.Where(m => m.Parent == null).First(x => x.Commands.Any(z => z.Name.Equals("say"))).Commands.First(y => y.Name.Equals(command.Name)).Summary;
                menu.AddOption(new SelectMenuOptionBuilder() { Label = selectedServer.Prefix + command.Name + " - " + desc, Value = command.Name });
            }
            string nextOptionName = "setup-commands-btn-2";
            if (page2)
                nextOptionName = "setup-automod-btn-mutelevel";
            msgProps.Components = cmdToggles.WithSelectMenu(menu)
                .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                .WithButton("Next Step", nextOptionName)
                .Build();
            return msgProps;
        }

        private static MessageProperties AutoMod(SocketGuildUser user, string customID, string interactionValue, Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();
            string[] customIDParts = customID.Split('-');

            // If menu option, set new int in settings
            if (customIDParts.Length == 4 && customIDParts[2] == "select" && interactionValue != "")
            {
                switch (customID)
                {
                    case "setup-automod-select-mutelevel":
                        selectedServer.MuteLevel = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🛡 Mute Level** to {selectedServer.MuteLevel} warns.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-select-muteduration":
                        selectedServer.MuteDuration = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🛡 Mute Duration** to {selectedServer.MuteDuration} minutes.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-select-kicklevel":
                        selectedServer.KickLevel = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🛡 Kick Level** to {selectedServer.KickLevel} warns.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-select-banlevel":
                        selectedServer.BanLevel = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🛡 Ban Level** to {selectedServer.BanLevel} warns.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-select-lockduration":
                        selectedServer.LockDuration = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🛡 Lock Duration** to {selectedServer.LockDuration} minutes.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-select-dupefreq":
                        selectedServer.DuplicateFreq = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🛡 Duplicate Frequency Threshold** to {selectedServer.DuplicateFreq} seconds.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-select-maxdupes":
                        selectedServer.MaxDuplicates = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🛡 Maximum Allowed Duplicates** to {selectedServer.MaxDuplicates} consecutive identical posts.", user.Guild.Id), user.Guild);
                        break;
                }
            }
            // If toggle button, set opposite of current value in settings
            if (customIDParts.Length == 5 && customIDParts[2] == "btn")
            {
                switch (customID)
                {
                    case "setup-automod-btn-deletedupes-toggle":
                        selectedServer.AutoDeleteDupes = !selectedServer.AutoDeleteDupes;
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set 🛡 Auto-Delete Duplicates** to {selectedServer.AutoDeleteDupes}.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-btn-warnondelete-toggle":
                        selectedServer.WarnOnAutoDelete = !selectedServer.WarnOnAutoDelete;
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set 🛡 Warn on Auto-Delete** to {selectedServer.WarnOnAutoDelete}.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-automod-btn-wordfilter-toggle":
                        selectedServer.WarnOnFilter = !selectedServer.WarnOnFilter;
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set 🛡 Warn on Filtered Message** to {selectedServer.WarnOnFilter}.", user.Guild.Id), user.Guild);
                        break;
                }
            }
            // Continue as if regular menu was reached
            customID = customID.Replace("select", "btn").Replace("-toggle", "");
            Botsettings.UpdateServer(selectedServer);
            // After saving, show current value and options
            switch (customID)
            {
                case "setup-automod-btn-mutelevel":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Mute)**__ (1/10)\n\n" +
                            $"With the ``{selectedServer.Prefix}warn`` command, moderators can **notify users of rule infractions**.\n" +
                            "These infractions are tracked and managed by the bot as **warns**. You can set automatic penalties " +
                            "for accumulating too many warns, such as *muting, kicking, or banning*.\n\n" +
                            "**How many warns should result in a 🔇 mute**? (0 to never auto-mute)\n" +
                            $"**Current setting**: {selectedServer.MuteLevel}",
                            selectedServer.Id, 0, true);
                    var menu = new SelectMenuBuilder() { CustomId = "setup-automod-select-mutelevel" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString() + " warn(s) before mute", Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-muteduration")
                        .Build();
                    break;
                case "setup-automod-btn-muteduration":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Mute Duration)**__ (2/10)\n\n" +
                            $"With the ``{selectedServer.Prefix}unmute`` command, moderators can **restore speaking privileges** to muted users.\n" +
                            "However, you could also _allow a mute to automically expire_. This may be useful when a user gets auto-muted and no " +
                            "moderators are around, or if you simply forget to unmute them.\n\n" +
                            "**What should be the max duration before an 🔈 unmute**? (0 to never auto-unmute)\n" +
                            $"**Current setting**: {selectedServer.MuteDuration} minutes",
                            selectedServer.Id, 0, true);
                    menu = new SelectMenuBuilder()
                    {
                        CustomId = "setup-automod-select-muteduration",
                        Options = new List<SelectMenuOptionBuilder> {
                            new SelectMenuOptionBuilder() { Label = "🔇 Never Auto-unmute", Value = "0" },
                            new SelectMenuOptionBuilder() { Label = "🕐 5 minutes", Value = "5" },
                            new SelectMenuOptionBuilder() { Label = "🕑 15 minutes", Value = "15" },
                            new SelectMenuOptionBuilder() { Label = "🕔 30 minutes", Value = "30" },
                            new SelectMenuOptionBuilder() { Label = "🕤 1 hour", Value = "60" },
                            new SelectMenuOptionBuilder() { Label = "🕙 4 hours", Value = "240" },
                            new SelectMenuOptionBuilder() { Label = "🕛 12 hours", Value = "720" },
                            new SelectMenuOptionBuilder() { Label = "🕠 1 day", Value = "1440" }}
                    };
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-kicklevel")
                        .Build();
                    break;
                case "setup-automod-btn-kicklevel":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Kick)**__ (3/10)\n\n" +
                            $"With the ``{selectedServer.Prefix}warn`` command, moderators can **notify users of rule infractions**.\n" +
                            "These infractions are tracked and managed by the bot as **warns**. You can set automatic penalties " +
                            "for accumulating too many warns, such as *muting, kicking, or banning*.\n\n" +
                            "**How many warns should result in a 👢 kick**? (0 to never auto-kick)\n" +
                            $"**Current setting**: {selectedServer.KickLevel}",
                            selectedServer.Id, 0, true);
                    menu = new SelectMenuBuilder() { CustomId = "setup-automod-select-kicklevel" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString() + " warn(s) before kick", Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-banlevel")
                        .Build();
                    break;
                case "setup-automod-btn-banlevel":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Ban)**__ (4/10)\n\n" +
                            $"With the ``{selectedServer.Prefix}warn`` command, moderators can **notify users of rule infractions**.\n" +
                            "These infractions are tracked and managed by the bot as **warns**. You can set automatic penalties " +
                            "for accumulating too many warns, such as *muting, kicking, or banning*.\n\n" +
                            "**How many warns should result in a 🔨 ban**? (0 to never auto-ban)\n" +
                            $"**Current setting**: {selectedServer.BanLevel}",
                            selectedServer.Id, 0, true);
                    menu = new SelectMenuBuilder() { CustomId = "setup-automod-select-banlevel" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString() + " warn(s) before ban", Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-lockduration")
                        .Build();
                    break;
                case "setup-automod-btn-lockduration":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Lock Duration)**__ (5/10)\n\n" +
                            $"With the ``{selectedServer.Prefix}unlock`` command, moderators can **restore speaking privileges** to " +
                            $"a channel previously locked using the ``{selectedServer.Prefix}lock`` command.\n" +
                            "However, you could also _allow a lock to automically expire_. This may be useful when no " +
                            "moderators are around, or if you simply forget to unlock a channel.\n\n" +
                            "**What should be the max duration before an 🔓 unlock**? (0 to never auto-unlock)\n" +
                            $"**Current setting**: {selectedServer.LockDuration} minutes",
                            selectedServer.Id, 0, true);
                    menu = new SelectMenuBuilder()
                    {
                        CustomId = "setup-automod-select-lockduration",
                        Options = new List<SelectMenuOptionBuilder> {
                            new SelectMenuOptionBuilder() { Label = "🔇 Never Auto-unlock", Value = "0" },
                            new SelectMenuOptionBuilder() { Label = "🕐 5 minutes", Value = "5" },
                            new SelectMenuOptionBuilder() { Label = "🕑 15 minutes", Value = "15" },
                            new SelectMenuOptionBuilder() { Label = "🕔 30 minutes", Value = "30" },
                            new SelectMenuOptionBuilder() { Label = "🕤 1 hour", Value = "60" },
                            new SelectMenuOptionBuilder() { Label = "🕙 4 hours", Value = "240" },
                            new SelectMenuOptionBuilder() { Label = "🕛 12 hours", Value = "720" },
                            new SelectMenuOptionBuilder() { Label = "🕠 1 day", Value = "1440" }}
                    };
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-deletedupes")
                        .Build();
                    break;
                case "setup-automod-btn-deletedupes":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Delete Duplicates)**__ (6/10)\n\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "Would you like to **enable this feature?**\n",
                            selectedServer.Id, 0, true);
                    var cmdToggles = new ComponentBuilder();
                    if (selectedServer.AutoDeleteDupes)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"setup-automod-btn-deletedupes-toggle", ButtonStyle.Primary, Components.chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"setup-automod-btn-deletedupes-toggle", ButtonStyle.Secondary, Components.unchk);
                    msgProps.Components = cmdToggles
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-dupefreq")
                        .Build();
                    break;
                case "setup-automod-btn-dupefreq":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Duplicate Freq)**__ (7/10)\n\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "**How close together (in seconds)** should identical messages " +
                            "be in order to be considered duplicates? (0 for always)\n" +
                            $"**Current setting**: {selectedServer.DuplicateFreq}",
                            selectedServer.Id, 0, true);
                    menu = new SelectMenuBuilder() { CustomId = "setup-automod-select-dupefreq" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString() + " seconds apart", Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-maxdupes")
                        .Build();
                    break;
                case "setup-automod-btn-maxdupes":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Max Duplicates)**__ (8/10)\n\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "After **how many identical messages in succession** should messages " +
                            "be considered duplicates? (0 for any)\n" +
                            $"**Current setting**: {selectedServer.MaxDuplicates}",
                            selectedServer.Id, 0, true);
                    menu = new SelectMenuBuilder() { CustomId = "setup-automod-select-maxdupes" };
                    for (int i = 0; i < 11; i++)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = i.ToString(), Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-warnondelete")
                        .Build();
                    break;
                case "setup-automod-btn-warnondelete":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Warn Duplicates)**__ (9/10)\n\n" +
                            "The bot can automatically detect and delete **duplicate messages**.\n\n" +
                            "Should it also issue an **automatic warn when duplicates are posted**?",
                                selectedServer.Id, 0, true);
                    cmdToggles = new ComponentBuilder();
                    if (selectedServer.WarnOnAutoDelete)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"setup-automod-btn-warnondelete-toggle", ButtonStyle.Primary, Components.chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"setup-automod-btn-warnondelete-toggle", ButtonStyle.Secondary, Components.unchk);
                    msgProps.Components = cmdToggles
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-automod-btn-wordfilter")
                        .Build();
                    break;
                case "setup-automod-btn-wordfilter":
                    msgProps.Embed = Embeds.ColorMsg("🛡 __**Automatic Moderation (Word Filter)**__ (10/10)\n\n" +
                            "The bot can automatically detect and delete messages containing **filtered terms**.\n\n" +
                            "You will need to add these terms to ``settings.yml`` yourself.\n" +
                            "Would you like to issue an **automatic warn when the filter is tripped?**",
                            selectedServer.Id, 0, true);
                    cmdToggles = new ComponentBuilder();
                    if (selectedServer.WarnOnFilter)
                        cmdToggles = cmdToggles.WithButton("Enabled", $"setup-automod-btn-wordfilter-toggle", ButtonStyle.Primary, Components.chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", $"setup-automod-btn-wordfilter-toggle", ButtonStyle.Secondary, Components.unchk);
                    msgProps.Components = cmdToggles
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-markov-btn-enabled")
                        .Build();
                    break;
            }

            return msgProps;
        }

        private static MessageProperties Markov(SocketGuildUser user, string customID, string interactionValue, Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();
            string[] customIDParts = customID.Split('-');

            // If menu option, set new int in settings
            if (customIDParts.Length == 4 && customIDParts[2] == "select" && interactionValue != "")
            {
                switch (customID)
                {
                    case "setup-markov-select-freq":
                        selectedServer.MarkovFreq = Convert.ToInt32(interactionValue);
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set the 🤖 Markov Frequency** to {selectedServer.MarkovFreq}.", user.Guild.Id), user.Guild);
                        break;
                }
            }
            // If toggle button, set opposite of current value in settings
            if (customIDParts.Length == 5 && customIDParts[2] == "btn")
            {
                switch (customID)
                {
                    case "setup-markov-btn-enabled-toggle":
                        selectedServer.AutoMarkov = !selectedServer.AutoMarkov;
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set 🤖 Auto-Markov Enabled** to {selectedServer.AutoMarkov}.", user.Guild.Id), user.Guild);
                        break;
                    case "setup-markov-btn-botchannelonly-toggle":
                        selectedServer.BotChannelMarkovOnly = !selectedServer.BotChannelMarkovOnly;
                        Moderation.SendToBotLogs(Embeds.ColorMsg($"⚙️ **{user.Mention} set 🤖 Auto-Markov Bot Channel Only** to {selectedServer.BotChannelMarkovOnly}.", user.Guild.Id), user.Guild);
                        break;
                }
            }
            // Continue as if regular menu was reached
            customID = customID.Replace("select", "btn").Replace("-toggle", "");
            Botsettings.UpdateServer(selectedServer);
            // After saving, show current value and options
            switch (customID)
            {
                case "setup-markov-btn-enabled":
                    msgProps.Embed = Embeds.ColorMsg("🤖 __**Markov (Auto)**__ (1/3)\n\n" +
                            "The bot can use incoming messages to build a **markov** dictionary.\n" +
                            "This allows it to sometimes respond with personalized, randomly generated " +
                            "sentences built from your users' vocabulary.\n\n" +
                            "These are pulled from public channels only and _won't include @mentions,_ " +
                            "_hyperlinks or any filtered terms_. Would you like to **enable the auto-markov feature**?",
                            selectedServer.Id, 0, true);
                    var cmdToggles = new ComponentBuilder();
                    if (selectedServer.AutoMarkov)
                        cmdToggles = cmdToggles.WithButton("Enabled", "setup-markov-btn-enabled-toggle", ButtonStyle.Primary, Components.chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", "setup-markov-btn-enabled-toggle", ButtonStyle.Secondary, Components.unchk);
                    msgProps.Components = cmdToggles
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-markov-btn-botchannelonly")
                        .Build();
                    break;
                case "setup-markov-btn-botchannelonly":
                    msgProps.Embed = Embeds.ColorMsg("🤖 __**Markov (Bot Sandbox Only)**__ (2/3)\n\n" +
                            "By default, **markov messages** are posted in response to random messages " +
                            "in public channels. Would you like to **restrict this to only the bot sandbox channel**?\n\n" +
                            "Don't worry, if you don't want random messages posted at all, in the next step, you can set the frequency " +
                            $"to zero so that the ``{selectedServer.Prefix}markov`` command must be used instead.",
                            selectedServer.Id, 0, true);
                    cmdToggles = new ComponentBuilder();
                    if (selectedServer.BotChannelMarkovOnly)
                        cmdToggles = cmdToggles.WithButton("Enabled", "setup-markov-btn-botchannelonly-toggle", ButtonStyle.Primary, Components.chk);
                    else
                        cmdToggles = cmdToggles.WithButton("Disabled", "setup-markov-btn-botchannelonly-toggle", ButtonStyle.Secondary, Components.unchk);
                    msgProps.Components = cmdToggles
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                        .WithButton("Next Step", "setup-markov-btn-freq")
                        .Build();
                    break;
                case "setup-markov-btn-freq":
                    msgProps.Embed = Embeds.ColorMsg("🤖 __**Markov (Frequency)**__ (3/3)\n\n" +
                            "**How often** would you like the bot to respond to an incoming message " +
                            "with a **markov message**?\n" +
                            $"Note: The **{selectedServer.Prefix}markov command** will always " +
                            "reply with a message 100% of the time.\n\n" +
                            $"**Current setting**: {selectedServer.MarkovFreq}% (0 = never)",
                            selectedServer.Id, 0, true);
                    var menu = new SelectMenuBuilder() { CustomId = "setup-markov-select-freq" };
                    for (int i = 0; i < 105; i = i + 5)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = "Randomly respond " + i.ToString() + "% of the time", Value = i.ToString() });
                    msgProps.Components = new ComponentBuilder()
                        .WithSelectMenu(menu)
                        .WithButton("End Setup", "setup-complete", ButtonStyle.Success)
                        .Build();
                    break;
            }

            return msgProps;
        }

        private static MessageProperties Complete(Server selectedServer)
        {
            MessageProperties msgProps = new MessageProperties();
            selectedServer.Configured = true;
            Botsettings.UpdateServer(selectedServer);

            msgProps.Embed = Embeds.ColorMsg("👍 __**Setup Complete!**__\n\n" +
                           "Everything should be all set for you to start using the bot.\n\n" +
                           $" Use ``{selectedServer.Prefix}help`` in the bot sandbox channel " +
                           "for a list of commands you can use. You can change these settings " +
                           $"anytime by visiting ``{selectedServer.Prefix}setup``. Enjoy!",
                           selectedServer.Id, 0, true);
            msgProps.Components = new ComponentBuilder().Build();

            return msgProps;
        }

    }
}
