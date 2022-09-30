using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace FrostBot
{
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("say", "Repeat a message.")]
        public async Task Say([Summary(description: "Text to repeat.")] string text)
        {
            await ReplyAsync(text);
        }

        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [SlashCommand("embed", "Repeat an embedded message.")]
        public async Task Embed([Summary(description: "Whether to display embed author.")] bool showAuthor = false)
        {
            var mb = new ModalBuilder()
                .WithTitle("Embed Message")
                .WithCustomId("embed_menu")
                .AddTextInput("Title", "embed_title", placeholder: "", required: false)
                .AddTextInput("Description", "embed_desc", TextInputStyle.Paragraph)
                .AddTextInput("Title Url", "embed_url", placeholder: "", required: false)
                .AddTextInput("Image Url", "embed_img", placeholder: "", required: false)
                .AddTextInput("Hex Color", "embed_color", placeholder: "", required: false);
            if (showAuthor)
                mb.CustomId = "embed_menu_author";

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("color", "Change your username color.")]
        public async Task Color([Summary(description: "Existing color role to use.")] SocketRole existingRole = null, 
            [Summary(description: "Name of the role to create.")] string colorName = "",
            [Summary(description: "RGB value in hex of the role color.")] string hexValue = "")
        {
            var user = (SocketGuildUser)Context.User;
            // Remove prefix if user already entered it
            colorName = colorName.Replace("Color: ", "");
            hexValue = hexValue.TrimStart('#');

            if (String.IsNullOrEmpty(colorName) || String.IsNullOrEmpty(hexValue))
            {
                // If user chose an existing role...
                if (existingRole != null)
                {
                    // Ensure role doesn't have any permissions
                    if (existingRole.Name.StartsWith("Color: ") && existingRole.Permissions.ToList().Count == 0)
                    {
                        await Task.Run(async () =>
                        {
                            // Remove any other color role the user already has
                            foreach (var userRole in user.Roles)
                                if (userRole.Name.StartsWith("Color: "))
                                    await user.RemoveRoleAsync(userRole);
                            // Grant new color role to user
                            await user.AddRoleAsync(existingRole);
                        });

                        await RespondAsync("­", new Embed[] { Embeds.Build(existingRole.Color,
                            $":art: Joined Color Role: {existingRole.Name.Replace("Color: ","")} (#{Embeds.GetHexColor(existingRole.Color)})",
                            "Any other Color Roles you had previously have been removed." )},
                            ephemeral: true);
                    }
                    else
                        await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Red,
                            ":warning: Error: Could not assign Role", $"Selected Role \"{existingRole.Name}\" is not a Color Role.")}, 
                            ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Red,
                        $":warning: Error: Could not create Color Role", "Missing required input: name & hex value.")}, 
                        ephemeral: true);
            }
            else
            {
                Color color = Embeds.GetDiscordColor(hexValue);
                // Create color role
                var colorRole = await Context.Guild.CreateRoleAsync($"Color: {colorName}", GuildPermissions.None, color);

                await Task.Run(async () =>
                {
                    // Remove other color role user already has
                    foreach (var userRole in user.Roles)
                        if (userRole.Name.StartsWith("Color: "))
                            await user.RemoveRoleAsync(userRole);
                    // Grant new color role
                    await user.AddRoleAsync(colorRole);

                    // Reorder Color Roles by color value
                    var orderedRoles = Context.Guild.Roles.Where(x => x.Name.StartsWith("Color: ")).OrderBy(x => 
                        System.Drawing.Color.FromArgb(x.Color.R, x.Color.R, x.Color.G, x.Color.B).GetHue());
                    // Move to highest possible position before moderator roles
                    var lowestModeratorRole = Context.Guild.Roles.FirstOrDefault(x => !x.Permissions.Administrator).Position;
                    foreach (var clrRole in orderedRoles)
                        await clrRole.ModifyAsync(x => x.Position = lowestModeratorRole - 1);
                });

                await RespondAsync("­", new Embed[] { Embeds.Build(new Discord.Color(Embeds.GetRoleColor(colorRole)),
                    $":art: Created Color Role: {colorName} (#{Embeds.GetHexColor(colorRole.Color)})", 
                    "You have been assigned this role. Any other Color Roles you had previously have been removed.")},
                        ephemeral: true);
            }
        }

        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [SlashCommand("add-joinable-role", "Add a role to the joinable roles list.")]
        public async Task AddJoinableRole([Summary(description: "Existing role to make joinable.")] SocketRole existingRole)
        {
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
            
            // Ensure role doesn't have admin permissions
            if (!existingRole.Permissions.ToList().Any(x => 
            x.Equals(GuildPermission.Administrator) || x.Equals(GuildPermission.ModerateMembers) || x.Equals(GuildPermission.BanMembers) || x.Equals(GuildPermission.KickMembers)))
            {
                if (!serverSettings.OptInRoles.Any(x => x.RoleID.Equals(existingRole.Id.ToString())))
                {
                    Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString())).OptInRoles.Add(new OptInRole { RoleID = existingRole.Id.ToString(), RoleName = existingRole.Name });
                    Program.settings.Save();
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    $":busts_in_silhouette: Role has been made joinable: {existingRole.Name}",
                    "Selected Role has been added to the bot's server settings." )},
                        ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Gold,
                    ":warning: Warning: Role is already joinable", $"Selected Role \"{existingRole.Name}\" is present in the bot's server settings.")},
                        ephemeral: true);
            }
            else
                await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Red,
                    ":warning: Error: Could not make Role joinable", $"Selected Role \"{existingRole.Name}\" has administrative or moderation permissions (kick/ban/timeout).")},
                        ephemeral: true);
        }

        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [SlashCommand("remove-joinable-role", "Remove a role from the joinable roles list.")]
        public async Task RemoveJoinableRole([Summary(description: "Existing role to revoke from being joinable.")] SocketRole existingRole)
        {
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));

            if (serverSettings.OptInRoles.Any(x => x.RoleID.Equals(existingRole.Id.ToString())))
            {
                Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString())).OptInRoles.Remove(
                    serverSettings.OptInRoles.First(x => x.RoleID.Equals(existingRole.Id.ToString())));
                Program.settings.Save();
                await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                $":busts_in_silhouette: Joinable Role has been removed: {existingRole.Name}",
                "Selected Role is no longer in the bot's server settings." )},
                    ephemeral: true);
            }
            else
                await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Gold,
                ":warning: Warning: Role is already not joinable", $"Selected Role \"{existingRole.Name}\" is not present in the bot's server settings.")},
                    ephemeral: true);
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("join", "Join an opt-in role.")]
        public async Task JoinRole([Summary(description: "Existing role to join.")] SocketRole role)
        {
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
            var user = (SocketGuildUser)Context.User;

            if (!user.Roles.Any(x => x.Id.Equals(role.Id)))
            {
                if (serverSettings.OptInRoles.Any(x => x.RoleID.Equals(role.Id.ToString())))
                {
                    await user.AddRoleAsync(role);
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    $":busts_in_silhouette: Joined Role: {role.Name}",
                    $"Use ``/leave {role.Name}`` to remove the role if you change your mind." )},
                        ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Red,
                    ":warning: Error: Role is not joinable", $"Selected Role \"{role.Name}\" is not in the list of joinable roles.")},
                        ephemeral: true);
            }
            await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Gold,
                ":warning: Warning: Role already joined", $"Selected Role \"{role.Name}\" is already one of your roles!")},
                    ephemeral: true);
        }

        [RequireContext(ContextType.Guild)]
        [SlashCommand("leave", "Leave an opt-in role.")]
        public async Task LeaveRole([Summary(description: "Existing role to leave.")] SocketRole role)
        {
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
            var user = (SocketGuildUser)Context.User;

            if (user.Roles.Any(x => x.Id.Equals(role.Id)))
            {
                if (serverSettings.OptInRoles.Any(x => x.RoleID.Equals(role.Id.ToString())))
                {
                    await user.RemoveRoleAsync(role);
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    $":busts_in_silhouette: Left Role: {role.Name}",
                    $"Use ``/join {role.Name}`` to rejoin the role if you change your mind." )},
                        ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Red,
                    ":warning: Error: Role is not opt-in", $"Selected Role \"{role.Name}\" is not in the list of joinable roles.")},
                        ephemeral: true);
            }
            await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Gold,
                ":warning: Warning: Role not present", $"Selected Role \"{role.Name}\" is not one of your roles!")},
                    ephemeral: true);
        }

        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [SlashCommand("forum", "Make the bot sync the forum.")]
        public async Task ForumSync()
        {
            Phpbb.ForumSync();
            await ReplyAsync("Forum sync complete.");
        }
    }
}
