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
    public class OptInRoles
    {
        public static MessageProperties Start(SocketGuildUser user, Server selectedServer, string customID, string interactionValue)
        {
            MessageProperties msgProps = new MessageProperties();
            ComponentBuilder builder = new ComponentBuilder();
            SocketGuild guild = Moderation.GetGuild(selectedServer.Id);
            string selected = "";
            uint color = 0;

            if (selectedServer.Roles.Any(x => x.Joinable))
            {
                if (customID == "optinroles-start" || customID == "optinroles-select-role")
                {
                    if (interactionValue != "")
                    {
                        try
                        {
                            var role = guild.Roles.First(x => x.Id.Equals(Convert.ToUInt64(interactionValue)));
                            if (role != null)
                                selected = $"\n\n**Current Selection:** {role.Name}";
                            color = Embeds.GetRoleColor(role);
                            builder = builder
                                .WithButton("Join Role", $"optinroles-btn-join-{role.Id}", ButtonStyle.Success)
                                .WithButton("Leave Role", $"optinroles-btn-leave-{role.Id}", ButtonStyle.Danger);
                        }
                        catch { }
                    }

                    msgProps.Embed = Embeds.ColorMsg(
                            "🔑 __**Opt-In Roles**__\n\n" +
                            "Add or remove any **descriptive roles** that are available to you. These roles might " +
                            "even grant additional access or permissions in various areas of the server!" +
                            selected,
                            selectedServer.Id, color, true);
                    var menu = new SelectMenuBuilder() { CustomId = "optinroles-select-role" };
                    foreach (var role in Moderation.GetGuild(selectedServer.Id).Roles.OrderBy(p => p.Position).Where(x => selectedServer.Roles.Any(y => y.Joinable && y.Id.Equals(x.Id))))
                        menu.AddOption(new SelectMenuOptionBuilder() { Label = role.Name, Value = role.Id.ToString() });
                    if (menu.Options.Count > 0)
                        builder = builder.WithSelectMenu(menu);
                    msgProps.Components = builder.WithButton("Exit", "optinroles-btn-complete", ButtonStyle.Danger).Build();
                }
                else if (customID.StartsWith("optinroles-btn-join-"))
                {
                    ulong roleID = Convert.ToUInt64(customID.Replace("optinroles-btn-join-", ""));
                    try
                    {
                        SocketRole role = guild.GetRole(Convert.ToUInt64(roleID));
                        // Grant role to user
                        user.AddRoleAsync(role);
                        // Announce change
                        msgProps.Embed = Embeds.ColorMsg($"🔑 **{user.Mention} joined the Role**: {role.Name}",
                                selectedServer.Id, Embeds.GetRoleColor(role));
                        msgProps.Components = builder.Build();
                    }
                    catch { }
                }
                else if (customID.StartsWith("optinroles-btn-leave-"))
                {
                    ulong roleID = Convert.ToUInt64(customID.Replace("optinroles-btn-leave-", ""));
                    SocketRole role = guild.GetRole(Convert.ToUInt64(roleID));
                    // Remove role from user
                    user.RemoveRoleAsync(role);
                    // Announce change
                    msgProps.Embed = Embeds.ColorMsg($"🔑 **{user.Mention} left the Role**: {role.Name}",
                            selectedServer.Id, Embeds.red);
                    msgProps.Components = builder.Build();
                }
                else
                {
                    msgProps.Embed = Embeds.ColorMsg($"🔑 **Opt-In Role** operation cancelled.", selectedServer.Id, Embeds.red);
                    msgProps.Components = builder.Build();
                }
            }
            else
            {
                msgProps.Embed = Embeds.ColorMsg($"🔑 There are no **Opt-In Roles** available in this server. Have an admin add some using ``{selectedServer.Prefix}setup``.", selectedServer.Id, Embeds.red);
                msgProps.Components = builder.Build();
            }
            

            return msgProps;
        }
    }
}
