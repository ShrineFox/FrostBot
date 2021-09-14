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
        public static MessageProperties Begin(MessageProperties msgProps, SocketGuild guild, string customID, string interactionValue, Server selectedServer)
        {
            string[] customIDParts = customID.Split('-');
            if (customIDParts.Length >= 2)
            {
                // Narrow down type of setup interaction by component ID
                switch(customIDParts[1])
                {
                    case "begin":
                        msgProps = Start(msgProps, selectedServer);
                        break;
                    case "moderators":
                        msgProps = Moderators(msgProps, guild, customID, interactionValue, selectedServer);
                        break;
                    case "channels":
                        msgProps = Channels(msgProps, guild, customID, interactionValue, selectedServer);
                        break;
                    case "commands":
                        msgProps = Commands(msgProps, guild, customID, interactionValue, selectedServer);
                        break;
                    case "automod":
                        msgProps = AutoMod(msgProps, guild, customID, interactionValue, selectedServer);
                        break;
                    case "markov":
                        msgProps = AutoMod(msgProps, guild, customID, interactionValue, selectedServer);
                        break;
                    case "complete":
                        msgProps = Complete(msgProps, guild, customID, interactionValue, selectedServer);
                        break;
                }
            }

            return msgProps;
        }

        private static MessageProperties Commands(MessageProperties msgProps, SocketGuild guild, string customID, string interactionValue, Server selectedServer)
        {
            
        }

        private static MessageProperties Channels(MessageProperties msgProps, SocketGuild guild, string customID, string interactionValue, Server selectedServer)
        {
            string[] customIDParts = customID.Split('-');
            string embedTxt = "";
            string selectedTxt = "";
            string nextBtnID = "";

            // Make sure there's at least 4 parts to avoid crash
            if (customIDParts.Length >= 4)
            {
                // On button press...
                if (customIDParts[2] == "btn")
                {
                    // Set "next" button destination
                    switch (customIDParts[3])
                    {
                        case "general":
                            nextBtnID = "setup-channel-btn-botsandbox";
                            break;
                        case "botsandbox":
                            nextBtnID = "setup-channel-btn-botlogs";
                            break;
                        case "botlogs":
                            nextBtnID = "setup-channel-btn-pins";
                            break;
                        case "pins":
                            nextBtnID = "setup-commands";
                            break;
                    }
                }
                // Display embed for specific channel
                switch (customIDParts[3])
                {
                    case "general":
                        embedTxt = "__**Channels (General)**__ (1/4)\n" +
                        "Please choose a \"General\" channel.\n\n" +
                        "This should be a public channel where custom messages appear " +
                        "when a new user joins the server.";
                        if (interactionValue != "")
                            selectedServer.Channels.General = Convert.ToUInt64(interactionValue);
                        if (selectedServer.Channels.General != 0)
                        {
                            var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.General);
                            selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
                        }
                        break;
                    case "botsandbox":
                        embedTxt = "__**Channels (Bot Sandbox)**__ (2/4)\n" +
                            "**Please choose a \"Bot Sandbox\" channel.**\n\n" +
                            "This is a public channel where users are encouraged to use bot commands.\n" +
                            "Later, you can set certain commands to only work in this channel.";
                        if (interactionValue != "")
                            selectedServer.Channels.BotSandbox = Convert.ToUInt64(interactionValue);
                        if (selectedServer.Channels.BotSandbox != 0)
                        {
                            var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.BotSandbox);
                            selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
                        }
                        break;
                    case "botlogs":
                        embedTxt = "__** Channels (Bot Logs)**__ (3/4)\n" +
                            "**Please choose a \"Bot Logs\" channel.**\n\n" +
                            "This is a **staff-only** channel that serves as an audit log for moderation commands.\n" +
                            "i.e. _Who banned what user in what channel, at what time and for what reason_.";
                        if (interactionValue != "")
                            selectedServer.Channels.BotLogs = Convert.ToUInt64(interactionValue);
                        if (selectedServer.Channels.BotLogs != 0)
                        {
                            var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.BotLogs);
                            selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
                        }
                        break;
                    case "pins":
                        embedTxt = "__**Channels (Pins)**__ (4/4)\n" +
                            "**Please choose a \"Pins\" channel.**\n\n" +
                            "This is a public channel where messages \"pinned\" with a bot command will go.\n" +
                            "This feature is intended as a workaround to the 50 message pin limit on channels.";
                        if (interactionValue != "")
                            selectedServer.Channels.Pins = Convert.ToUInt64(interactionValue);
                        if (selectedServer.Channels.Pins != 0)
                        {
                            var channel = (ITextChannel)guild.GetChannel(selectedServer.Channels.Pins);
                            selectedTxt = $"\n\n**Current selection**: {channel.Mention}";
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
            if (menu.Options.Count() > 0)
                msgProps.Components = new ComponentBuilder().WithSelectMenu(menu)
                    .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Skip", nextBtnID)
                    .Build();
            else
                msgProps.Components = new ComponentBuilder()
                    .WithButton("End Setup", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Next Step", nextBtnID)
                    .Build();

            Botsettings.UpdateServer(selectedServer);
            return msgProps;
        }

        private static MessageProperties Moderators(MessageProperties msgProps, SocketGuild guild, string customID, string interactionValue, Server selectedServer)
        {
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
            }

            // Return Moderator selection prompt with list of currently selected modroles
            string modRoles = "";
            foreach (var modRole in selectedServer.Roles.Where(x => x.Moderator))
            {
                IRole role = guild.GetRole(modRole.Id);
                modRoles += $"{role.Mention}\n";
            }
            msgProps.Embed = Embeds.ColorMsg(
                        "__**Moderator Roles**__\n" +
                        "Choose some \"Moderator\" Roles.\n\n" +
                        "Users with the selected roles can **manage the server using special bot commands**.\n" +
                        "For instance, _creating opt-in roles, kicking users, changing bot settings..._\n\n" +
                        $"**Current selection:**\n{modRoles}\n" +
                        "Due to limitations, only the top 25 roles are shown.",
                        guild.Id, 0, true);
            // Show list of non-mod roles in component as dropdown menu
            var menu = new SelectMenuBuilder() { CustomId = "select-moderators" };
            foreach (var role in guild.Roles.OrderBy(p => p.Position).Where(x => !x.IsEveryone && selectedServer.Roles.Any(i => i.Id.Equals(x.Id))))
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
                    .WithButton("Next Step", "setup-channels")
                    .Build();

            Botsettings.UpdateServer(selectedServer);
            return msgProps;
        }

        private static MessageProperties Start(MessageProperties msgProps, Server selectedServer)
        {
            if (!selectedServer.Configured)
            {
                msgProps.Embed = Embeds.ColorMsg(
                        "__**FrostBot Setup**__\n\n" +
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
                    "__**FrostBot Setup**__\n\n" +
                        "**Please have a Moderator complete this setup.**\n" +
                        "Thank you for choosing [FrostBot](https://github.com/ShrineFox/JackFrost-Bot) " +
                        "by [ShrineFox](https://github.com/ShrineFox)!\n\n" +
                        "Choose one of the options below to reconfigure.",
                        selectedServer.Id, 0, true);
                msgProps.Components = new ComponentBuilder()
                    .WithButton("Cancel", "setup-complete", ButtonStyle.Danger)
                    .WithButton("Moderator Roles", "setup-moderators")
                    .WithButton("Channels", "setup-channels-btn-general")
                    .WithButton("Commands", "setup-commands")
                    .WithButton("Auto-Moderation", "setup-automod")
                    .WithButton("Markov", "setup-markov")
                    .Build();
            }

            return msgProps;
        }
    }
}
