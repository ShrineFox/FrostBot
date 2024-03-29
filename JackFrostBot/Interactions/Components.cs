﻿using Discord;
using Discord.WebSocket;
using FrostBot.Interactions;
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
        public static Emoji unchk = new Emoji("🟦");
        public static Emoji chk = new Emoji("☑️");

        public static MessageProperties GetProperties(SocketMessageComponent interaction)
        {
            var msgProps = new MessageProperties();

            var user = (SocketGuildUser)interaction.User;
            var guild = user.Guild;
            var selectedServer = Program.settings.Servers.First(x => x.Id.Equals(guild.Id));

            // Value of menu selection
            string interactionValue = "";
            if (interaction.Data.Values != null && interaction.Data.Values.Count > 0)
                interactionValue = interaction.Data.Values.First();
            // Name of button selection
            string customID = interaction.Data.CustomId;

            // Log to console
            Processing.LogDebugMessage($"interaction: {interactionValue}");
            Processing.LogDebugMessage($"customID: {customID}");

            // Proceed with bot configuration
            if (customID.StartsWith("setup-"))
            {
                // Ignore if user is not a moderator
                if (!user.Roles.Any(x => selectedServer.Roles.Where(z => z.Moderator).Any(y => y.Id.Equals(x.Id))))
                    return msgProps;
                // Return new setup embed/components
                msgProps = Setup.Begin(user, guild, customID, interactionValue, selectedServer);
            }
            else if (customID.StartsWith("colorroles-"))
            {
                msgProps = ColorRoles.Start(user, selectedServer, customID, interactionValue);
            }
            else if (customID.StartsWith("optinroles-"))
            {
                msgProps = OptInRoles.Start(user, selectedServer, customID, interactionValue);
            }

            return msgProps;
        }
    }
}

