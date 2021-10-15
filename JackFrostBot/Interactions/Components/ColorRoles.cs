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
    public class ColorRoles
    {
        public static MessageProperties Start(SocketGuildUser user, Server selectedServer, string customID, string interactionValue)
        {
            MessageProperties msgProps = new MessageProperties();
            ComponentBuilder builder = new ComponentBuilder();
            SocketGuild guild = Moderation.GetGuild(selectedServer.Id);
            string selected = "";
            uint color = 0;
            int page = 1;

            if (customID.StartsWith("colorroles-start") || customID == "colorroles-select-color")
            {
                if (guild.Roles.Any(x => x.Name.ToLower().StartsWith("color: ")))
                {
                    if (interactionValue != "")
                    {
                        try
                        {
                            var role = guild.Roles.First(x => x.Id.Equals(Convert.ToUInt64(interactionValue)));
                            if (role != null)
                                selected = $"\n\n**Current Selection:** {role.Name.Replace("Color: ", "")}";
                            color = Embeds.GetRoleColor(role);
                            builder = builder
                                .WithButton("Choose Color", $"colorroles-btn-color-{role.Id}");
                        }
                        catch { }
                    }

                    // Set up pages
                    var colorRoles = Moderation.GetGuild(selectedServer.Id).Roles.OrderBy(p => p.Position).Where(x => x.Name.ToLower().StartsWith("color: "));
                    if (customID.Contains("page"))
                        page = Convert.ToInt32(customID.Replace("colorroles-start-page-", ""));
                    if (page > 1 && colorRoles.Skip(25 * page).Count() > 0)
                        colorRoles = colorRoles.Skip(25 * page).ToList();
                    else
                    {
                        page = 1;
                        colorRoles = colorRoles.Take(25).ToList();
                    }

                    msgProps.Embed = Embeds.ColorMsg(
                            "🎨 __**Color Roles**__\n\n" +
                            "Change your **username color** by choosing a cosmetic role.\n\n" +
                            $"You can add your own colors using the command ``{selectedServer.Prefix}color <#HEXVALUE> <color name>``. " +
                            "Need help picking a hex value? Try an [online color picker](https://htmlcolorcodes.com/color-picker/)!" +
                            selected,
                            selectedServer.Id, color, true);
                    var menu = new SelectMenuBuilder() { CustomId = "colorroles-select-color" };
                    foreach (var role in colorRoles)
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = role.Name, Value = role.Id.ToString() });
                    if (menu.Options.Count > 0)
                        builder = builder.WithSelectMenu(menu);
                    builder = builder.WithButton("More", $"colorroles-start-page-{page + 1}");
                    msgProps.Components = builder.WithButton("Exit", "colorroles-btn-complete", ButtonStyle.Danger).Build();
                }
                else
                {
                    msgProps.Embed = Embeds.ColorMsg(
                            "🎨 __**Color Roles**__\n\n" +
                            "Change your **username color** by choosing a cosmetic role.\n\n" +
                            "**This server has no color roles yet**!\n" +
                            $"You can add your own colors using the command ``{selectedServer.Prefix}color <#HEXVALUE> <color name>``. " +
                            "Need help picking a hex value? Try an [online color picker](https://htmlcolorcodes.com/color-picker/)!",
                            selectedServer.Id, 0, true);
                    msgProps.Components = builder.Build();
                }
            }
            else if (customID.StartsWith("colorroles-btn-color-"))
            {
                ulong roleID = Convert.ToUInt64(customID.Replace("colorroles-btn-color-",""));
                try
                {
                    SocketRole role = guild.GetRole(Convert.ToUInt64(roleID));
                    // Remove other color role user already has
                    foreach (var userRole in user.Roles)
                        if (userRole.Name.ToLower().StartsWith("color: "))
                            user.RemoveRoleAsync(userRole);
                    // Grant new color role
                    user.AddRoleAsync(role);
                    // Announce change
                    msgProps.Embed = Embeds.ColorMsg($"🎨 **Username color updated** for {user.Mention}: **{role.Name.Replace("Color: ","")}**",
                            selectedServer.Id, Embeds.GetRoleColor(role));
                    msgProps.Components = builder.Build();
                }
                catch { }
            }
            else
            {
                msgProps.Embed = Embeds.ColorMsg($"🎨 **Username color change** operation cancelled.", selectedServer.Id, Embeds.red);
                msgProps.Components = builder.Build();
            }

            return msgProps;
        }
    }
}
