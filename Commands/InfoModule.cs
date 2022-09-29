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

                Task.Run(async () =>
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
        [SlashCommand("forum", "Make the bot sync the forum.")]
        public async Task ForumSync()
        {
            Phpbb.ForumSync();
            await ReplyAsync("Forum sync complete.");
        }
    }
}
