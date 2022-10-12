using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ShrineFox.IO;

namespace FrostBot
{
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; set; }

        [Group("set", "Set a parameter in the bot's settings.")]
        public class SetModule : InteractionModuleBase<SocketInteractionContext>
        {
            [RequireContext(ContextType.Guild)]
            [SlashCommand("color", "Change your username color.")]
            public async Task SetColor([Summary(description: "Existing color role to use.")] SocketRole existingRole = null,
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
            [SlashCommand("joinable-role", "Add a role to the joinable roles list.")]
            public async Task SetJoinableRole([Summary(description: "Existing role to make joinable.")] SocketRole existingRole)
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
            [SlashCommand("pin-channel", "Add a channel users can copy messages to with the pin command.")]
            public async Task SetPinChannel([Summary(description: "Existing channel to use as a pin destination.")] SocketChannel channel)
            {
                await SetChannel("PinChannel", channel.Id.ToString());
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("botlog-channel", "Add a channel where bot usage logs are sent.")]
            public async Task SetBotLogChannel([Summary(description: "Existing channel to use as bot logs destination.")] SocketChannel channel)
            {
                await SetChannel("BotLogChannel", channel.Id.ToString());
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("modmail-channel", "Add a channel where reported messages are sent.")]
            public async Task SetModMailChannel([Summary(description: "Existing channel to use as reported message destination.")] SocketChannel channel)
            {
                await SetChannel("ModMailChannel", channel.Id.ToString());
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("automarkov-channel", "Add a channel where reported messages are sent.")]
            public async Task SetAutoMarkovChannel([Summary(description: "Existing channel to send randomized messages to.")] SocketChannel channel)
            {
                await SetChannel("AutoMarkovChannel", channel.Id.ToString());
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("automarkov-rate", "Set percentage chance to send a random message in reply.")]
            public async Task SetAutoMarkovRate([Summary(description: "Number from 0 - 100 how often to send a random response.")] int percentChance)
            {
                // Keep value in range
                if (percentChance > 100)
                    percentChance = 100;
                if (percentChance < 0)
                    percentChance = 0;

                await SetInt("AutoMarkovRate", percentChance);
            }

            private async Task SetInt(string settingName, int value)
            {
                Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
                int currentValue = (int)serverSettings.GetType().GetProperty(settingName).GetValue(serverSettings, null);

                serverSettings.GetType().GetProperty(settingName).SetValue(serverSettings, value);
                for (int i = 0; i < Program.settings.Servers.Count; i++)
                    if (Program.settings.Servers[i].ServerID == serverSettings.ServerID)
                        Program.settings.Servers[i] = serverSettings; Program.settings.Save();

                Program.settings.Save();
                await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    $":busts_in_silhouette: {settingName} has been set to: {value}" )},
                    ephemeral: true);
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("automarkov-length", "Minimum number of characters randomized messages should be.")]
            public async Task SetAutoMarkovRate([Summary(description: "Existing channel to send randomized messages to.")] SocketChannel channel)
            {
                await SetChannel("AutoMarkovChannel", channel.Id.ToString());
            }

            private async Task SetChannel(string settingName, string value)
            {
                Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
                Channel channel = (Channel)serverSettings.GetType().GetProperty(settingName).GetValue(serverSettings, null);
                if (channel.ID.ToString() != value)
                {
                    serverSettings.GetType().GetProperty(settingName).SetValue(serverSettings, new Channel { ID = value, Name = settingName });
                    for (int i = 0; i < Program.settings.Servers.Count; i++)
                        if (Program.settings.Servers[i].ServerID == serverSettings.ServerID)
                            Program.settings.Servers[i] = serverSettings;

                    Program.settings.Save();
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    $":busts_in_silhouette: {settingName} has been set: {value}" )},
                        ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Gold,
                    $":warning: Warning: {settingName} already set", 
                    $"Selected Channel \"{value}\" is present in the bot's server settings.")},
                        ephemeral: true);
            }
        }
        

        [Group("unset", "Unset a parameter in the bot's settings.")]
        public class UnsetModule : InteractionModuleBase<SocketInteractionContext>
        {
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("joinable-role", "Remove a role from the joinable roles list.")]
            public async Task UnsetJoinableRole([Summary(description: "Existing role to revoke from being joinable.")] SocketRole existingRole)
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
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("pin-channel", "Unlink channel where messages are pinned to via pin command.")]
            public async Task UnsetPinChannel()
            {
                await UnsetChannel("PinChannel");
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("botlog-channel", "Unlink channel where usage logs are copied to.")]
            public async Task UnsetBotLogChannel()
            {
                await UnsetChannel("BotLogChannel");
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("modmail-channel", "Unlink channel where reported messages are copied to.")]
            public async Task UnsetModMailChannel()
            {
                await UnsetChannel("ModMailChannel");
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("modmail-channel", "Unlink channel where bot replies with randomized messages.")]
            public async Task UnsetAutoMarkovChannel()
            {
                await UnsetChannel("AutoMarkovChannel");
            }

            private async Task UnsetChannel(string settingName)
            {
                Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
                Channel channel = (Channel)serverSettings.GetType().GetProperty(settingName).GetValue(serverSettings, null);
                if (channel.ID.ToString() != "")
                {
                    serverSettings.GetType().GetProperty(settingName).SetValue(serverSettings, new Channel());
                    for (int i = 0; i < Program.settings.Servers.Count; i++)
                        if (Program.settings.Servers[i].ServerID == serverSettings.ServerID)
                            Program.settings.Servers[i] = serverSettings;
                    Program.settings.Save();

                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    $":busts_in_silhouette: {settingName} has been unset." )},
                        ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build( Discord.Color.Gold,
                    $":warning: Warning: {settingName} already unset",
                    $"No channel is present in the bot's server settings.")},
                        ephemeral: true);
            }
        }

        [Group("check", "Show data from the bot's settings.")]
        public class CheckModule : InteractionModuleBase<SocketInteractionContext>
        {
            [RequireContext(ContextType.Guild)]
            [SlashCommand("warns", "Display a list of a user's warns.")]
            public async Task CheckWarns([Summary(description: "User whose warns to check.")] SocketGuildUser user = null)
            {
                List<Warn> warns = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString())).Warns;
                if (user != null)
                    warns = warns.Where(x => x.UserID == user.Id.ToString()).ToList();
                string warnsList = "";
                for (int i = 0; i < warns.Count; i++)
                    warnsList += $"\n{i + 1}. {warns[i].Username}: {warns[i].Reason} ({warns[i].Date})";

                if (warns.Count > 0)
                {
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Red,
                    $":warning: **Warns List**", warnsList )},
                        ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Gold,
                    desc: $":warning: Warning: No warns to list!" )},
                        ephemeral: true);
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("int", "Show the value of an int from the bot's settings.")]
            public async Task CheckIntCmd([Summary(description: "Name of the int to check.")] string intName)
            {
                await CheckInt(intName);
            }

            private async Task CheckInt(string settingName)
            {
                Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
                int currentValue = (int)serverSettings.GetType().GetProperty(settingName).GetValue(serverSettings, null);

                Program.settings.Save();
                await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    $":busts_in_silhouette: {settingName} is currently set to: {currentValue}" )},
                    ephemeral: true);
            }
        }

        [Group("clear", "Remove data from the bot's settings.")]
        public class ClearModule : InteractionModuleBase<SocketInteractionContext>
        {
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.BanMembers)]
            [RequireBotPermission(GuildPermission.BanMembers)]
            [SlashCommand("warn", "Clear one of a user's warns.")]
            public async Task ClearWarn([Summary(description: "Number of the warn to clear.")] int warnNumber,
            [Summary(description: "User whose warn to clear.")] SocketGuildUser user = null)
            {
                List<Warn> warns = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString())).Warns;
                if (user != null)
                    warns = warns.Where(x => x.UserID == user.Id.ToString()).ToList();

                if (warns.Count >= warnNumber)
                {
                    Warn warn = warns[warnNumber - 1];
                    Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString())).Warns.Remove(warn);
                    Program.settings.Save();
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                    desc: $":ok_hand: **Warn Cleared**: {warn.Username}: {warn.Reason} ({warn.Date}" )},
                        ephemeral: true);
                }
                else
                    await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Red,
                    desc: $":warning: Error: Could not find warn to clear!" )},
                        ephemeral: true);
            }
        }

        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [SlashCommand("say", "Repeat a message.")]
        public async Task Say([Summary(description: "Text to repeat.")] string text)
        {
            await ReplyAsync(text);
        }

        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [SlashCommand("delete", "Deletes a set of messages in the channel.")]
        public async Task Delete([Summary(description: "ID of first message to delete.")] string startMessageID,
            [Summary(description: "ID of last message to delete.")] string endMessageID = "", 
            [Summary(description: "Author of the messages to delete.")] IGuildUser author = null,
            [Summary(description: "Number of messages to delete.")] int numberToDelete = 100)
        {
            if (numberToDelete > 500)
                await RespondAsync($"Choose a number less than 500 of messages to delete.", ephemeral: true);

            var messages = await Context.Channel.GetMessagesAsync(numberToDelete).FlattenAsync();
            var msgs = messages.Reverse().ToArray();

            Output.Log($"Downloaded {msgs.Count()} messages.");

            bool foundStartMsg = false;
            bool foundEndMsg = false;
            int deletedMessages = 0;

            for (int i = 0; i < msgs.Count(); i++)
            {
                if (msgs[i].Id == Convert.ToUInt64(startMessageID))
                {
                    Output.Log($"Found start message: {startMessageID}");
                    foundStartMsg = true;
                }

                if (foundStartMsg && !foundEndMsg)
                {
                    if (author == null || msgs[i].Author.Id == author.Id)
                    {
                        Output.Log($"Deleted message from {msgs[i].Author.Username} in \"{Context.Guild.Name}\" #{Context.Channel.Name}: " + msgs[i].Content, ConsoleColor.Red);
                        await msgs[i].DeleteAsync();
                        deletedMessages++;
                    }
                }

                if (endMessageID != "")
                    if (msgs[i].Id == Convert.ToUInt64(endMessageID))
                    {
                        Output.Log($"Found end message: {endMessageID}");
                        foundEndMsg = true;
                    }
            }
            await RespondAsync($":ok_hand: Deleted {deletedMessages} messages.", ephemeral: true);
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
        [SlashCommand("warn", "Warn a user.")]
        public async Task WarnUser([Summary(description: "User to warn.")] SocketGuildUser user, string reason)
        {
            // Record warn in Settings
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
            Warn warn = new Warn { Username = user.Username, UserID = user.Id.ToString(), 
                ModeratorName = Context.User.Username, Reason = reason, Date = DateTime.Now.ToString("MM/dd/yyyy") };
            serverSettings.Warns.Add(warn);
            Program.settings.Save();

            // Notify user of warn
            int warnCount = serverSettings.Warns.Where(x => x.UserID.Equals(user.Id.ToString())).Count();
            string warnCountMsg = $"User has {warnCount} warn";
            if (warnCount > 1)
                warnCountMsg += "s";

            await ReplyAsync(embed: Embeds.Build(Discord.Color.Red,
                    desc: $":warning: Warn {user.Mention}: {reason}",
                    foot: warnCountMsg));

            // Penalize user according to settings
            if (serverSettings.WarnOptions.TimeOutAfter != 0 && warnCount >= serverSettings.WarnOptions.TimeOutAfter)
            {
                if (serverSettings.WarnOptions.KickAfter != 0 && warnCount >= serverSettings.WarnOptions.KickAfter)
                {
                    if (serverSettings.WarnOptions.BanAfter != 0 && warnCount >= serverSettings.WarnOptions.BanAfter)
                    {
                        await ReplyAsync(embed: Embeds.Build(Discord.Color.Red,
                            desc: $"\n:hammer: **Banned** automatically for accumulating {serverSettings.WarnOptions.KickAfter} or more warns."
                            ));

                        await user.Guild.AddBanAsync(user, 0, reason);
                    }
                    else
                    {
                        await ReplyAsync(embed: Embeds.Build(Discord.Color.Red,
                            desc: $"\n:boot: **Kicked** automatically for accumulating {serverSettings.WarnOptions.KickAfter} or more warns."
                            ));

                        await user.KickAsync(reason);
                    }
                }
                else
                {
                    await user.SetTimeOutAsync(new TimeSpan(0, serverSettings.WarnOptions.TimeOutLength, 0, 0));

                    await ReplyAsync(embed: Embeds.Build(Discord.Color.Red,
                        desc: $"\n:mute: **Muted** automatically for accumulating {serverSettings.WarnOptions.TimeOutAfter} or more warns."
                        ));
                }
            }

            // Notify moderator of successful action
            await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Green,
                $"User has been warned successfully." )},
                ephemeral: true);
        }

        /* Message Commands */

        [RequireContext(ContextType.Guild)]
        [MessageCommand("Pin to Channel")]
        public async Task PinMessage(IMessage message)
        {
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));
            if (serverSettings.PinChannel.ID != "")
            {
                var msgChannel = Context.Guild.GetTextChannel(message.Channel.Id);
                var pinChannel = Context.Guild.GetTextChannel(Convert.ToUInt64(serverSettings.PinChannel.ID));

                await pinChannel.SendMessageAsync(embed: Embeds.Build(Embeds.GetUsernameColor((SocketGuildUser)message.Author),
                    title: $"#{msgChannel.Name} ({message.Timestamp.DateTime})",
                    url: message.GetJumpUrl(),
                    desc: message.Content,
                    foot: $"Pinned by {Context.User.Username} on {DateTime.Now}",
                    authorName: message.Author.Username,
                    authorImgUrl: message.Author.GetAvatarUrl()));
            }
            else
            {
                await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Red,
                    desc: $":warning: **Error**: No pinned message channel set!" )},
                    ephemeral: true);
            }
        }

        [RequireContext(ContextType.Guild)]
        [MessageCommand("Report Message")]
        public async Task ReportMessage(IMessage message)
        {
            Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(Context.Guild.Id.ToString()));

            if (serverSettings.ModMailChannel.ID != "")
            {
                var msgChannel = Context.Guild.GetTextChannel(message.Channel.Id);
                var modChannel = Context.Guild.GetTextChannel(Convert.ToUInt64(serverSettings.ModMailChannel.ID));

                var mb = new ModalBuilder()
                .WithTitle("Report Message to Mods")
                .WithCustomId("report_menu")
                .AddTextInput("Reason for report", "report_reason", TextInputStyle.Paragraph)
                .AddTextInput("Action you'd like taken", "report_action", TextInputStyle.Paragraph, required: false)
                .AddTextInput("Want a mod to follow up with you?", "report_followup", required: false);

                await modChannel.SendMessageAsync(embed: Embeds.Build(Discord.Color.Red,
                    title: $"Post Reported: #{msgChannel.Name} ({message.Timestamp.DateTime})",
                    url: message.GetJumpUrl(),
                    desc: message.Content,
                    foot: $"Reported by {Context.User.Username} on {DateTime.Now}",
                    authorName: message.Author.Username,
                    authorImgUrl: message.Author.GetAvatarUrl()));

                await Context.Interaction.RespondWithModalAsync(mb.Build());
            }
            else
            {
                await RespondAsync("­", new Embed[] { Embeds.Build(Discord.Color.Red,
                    desc: $":warning: **Error**: No mod mail channel set!" )},
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
