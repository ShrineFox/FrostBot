using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Globalization;
using ImageProcessor.Plugins.Cair;
using ImageProcessor;
using System.Net;
using SoundInTheory.DynamicImage;
using System.Windows.Media.Imaging;
using ImageResizer;
using System.Drawing.Imaging;
using System.Windows.Media;
using static FrostBot.Config;

namespace FrostBot
{
    public class InfoModule : ModuleBase
    {
        // Make the bot repeat a message
        [Command("setup"), Summary("Configure the bot.")]
        public async Task Setup()
        {
            if (Moderation.CommandAllowed("setup", Context))
            {
                await Context.Message.DeleteAsync();
                var msgProps = Interactions.Setup.Start(Botsettings.GetServer(Context.Guild.Id));
                var components = (MessageComponent)msgProps.Components;
                await Context.Channel.SendMessageAsync(embed: (Embed)msgProps.Embed, component: components );
            }
        }

        // Make the bot repeat a message
        [Command("say"), Summary("Make the bot repeat a message.")]
        public async Task Say([Remainder, Summary("The format to get info about.")] string message)
        {
            if (Moderation.CommandAllowed("say", Context))
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync(message);
            }
        }

        // Get info from a wiki page
        [Command("wiki"), Summary("Get info from a wiki page.")]
        public async Task GetInfo([Remainder, Summary("The page to get info from.")] string keyword)
        {
            if (Moderation.CommandAllowed("wiki", Context))
            {
                var embed = Embeds.Wiki(keyword, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        // Get info about bot commands available to you
        [Command("help"), Summary("Get info about using the bot.")]
        public async Task GetHelp()
        {
            if (Moderation.CommandAllowed("help", Context))
            {
                var embed = Embeds.Help(Context.Guild.Id, Moderation.IsModerator((IGuildUser)Context.Message.Author, Context.Guild.Id));
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        // Direct users in a channel to another channel
        [Command("unarchive"), Summary("Set thread to remain unarchived.")]
        public async Task Unarchive()
        {
            if (Moderation.CommandAllowed("unarchive", Context))
            {
                if (Context.Channel.GetType().Equals(ChannelType.PublicThread) 
                    || Context.Channel.GetType().Equals(ChannelType.PublicThread)
                    || Context.Channel.GetType().Equals(ChannelType.NewsThread))
                {
                    var selectedServer = Botsettings.GetServer(Context.Guild.Id);
                    if (!selectedServer.ThreadsToUnarchive.Any(x => x.Equals(Context.Channel.Id)))
                    {
                        Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).ThreadsToUnarchive.Add(Context.Channel.Id);
                        await Context.Channel.SendMessageAsync("Thread will be unarchived automatically from now on!");
                    }
                    else
                    {
                        Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).ThreadsToUnarchive.Remove(Context.Channel.Id);
                        await Context.Channel.SendMessageAsync("Thread will **no longer** be automatically unarchived.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync("That command only works in threads!");
                }
            }
        }

        // Change bot's currently Playing text
        [Command("set game"), Summary("Change the Currently Playing text.")]
        public async Task SetGame([Remainder, Summary("The text to set as the Game.")] string game)
        {
            if (Moderation.CommandAllowed("set game", Context))
            {
                var client = (DiscordSocketClient)Context.Client;
                await client.SetGameAsync(game);
            }
        }

        // Grant yourself the specified opt-in role
        [Command("grant"), Summary("Grant yourself the specified opt-in role.")]
        public async Task GrantRole([Remainder, Summary("The name of the role.")] string roleName)
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("grant", Context))
            {
                var user = (IGuildUser)Context.Message.Author;
                ulong roleId = 0;
                roleId = selectedServer.Roles.First(x => x.Name.ToLower().Equals(roleName.ToLower())).Id;

                if (roleId != 0)
                {
                    await user.AddRoleAsync(roleId);
                    await Context.Channel.SendMessageAsync("Role successfully added!");
                }
                else
                    await Context.Channel.SendMessageAsync("The specified role isn't available for opt-in!");
            }
        }

        // Remove specified role from yourself
        [Command("remove"), Summary("Remove the specified role from yourself.")]
        public async Task RemoveRole([Remainder, Summary("The name of the role.")] string roleName)
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("remove", Context))
            {
                var user = (IGuildUser)Context.Message.Author;
                ulong roleId = 0;
                roleId = selectedServer.Roles.First(x => x.Name.ToLower().Equals(roleName.ToLower())).Id;

                if (roleId != 0)
                {
                    await user.RemoveRoleAsync(roleId);
                    await Context.Channel.SendMessageAsync("Role successfully removed!");
                }
                else
                    await Context.Channel.SendMessageAsync("The specified role cannot be found!");
            }
        }

        // Warn a user and log it
        [Command("warn"), Summary("Warn a user.")]
        public async Task Warn([Summary("The user to warn.")] IGuildUser mention, [Summary("The reason for the warn."), Remainder] string reason = "No reason given.")
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("warn", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Warn((IGuildUser)Context.User, (ITextChannel)Context.Channel, mention, reason);
            }
        }

        // Mute a user and log it
        [Command("mute"), Summary("Mute a user.")]
        public async Task Mute([Summary("The user to mute.")] SocketGuildUser mention, [Summary("The reason for the mute."), Remainder] string reason = "No reason given.")
        {
            if (Moderation.CommandAllowed("mute", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Mute(Context.User.Username, (ITextChannel)Context.Channel, mention);
            }
        }

        // Unmute a user and log it
        [Command("unmute"), Summary("Unmute a muted user.")]
        public async Task Unmute([Summary("The user to unmute.")] SocketGuildUser mention)
        {
            if (Moderation.CommandAllowed("unmute", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Unmute(Context.User.Username, (ITextChannel)Context.Channel, mention);
            }
        }

        // Lock a channel and log it
        [Command("lock"), Summary("Lock a channel.")]
        public async Task Lock()
        {
            if (Moderation.CommandAllowed("lock", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Lock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
            }
        }

        // Unlock a channel and log it
        [Command("unlock"), Summary("Unlock a channel.")]
        public async Task Unlock()
        {
            if (Moderation.CommandAllowed("unlock", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Unlock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
            }
        }

        // Kick a user and log it
        [Command("kick"), Summary("Kick a user.")]
        public async Task Kick([Summary("The user to kick.")] SocketGuildUser mention, [Summary("The reason for the kick."), Remainder] string reason = "No reason given.")
        {
            if (Moderation.CommandAllowed("kick", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Kick(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
            }
        }

        // Ban a user and log it
        [Command("ban"), Summary("Ban a user.")]
        public async Task Ban([Summary("The user to ban.")] SocketGuildUser mention, [Summary("The reason for the ban."), Remainder] string reason = "No reason given.")
        {
            if (Moderation.CommandAllowed("ban", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Ban(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
            }
        }

        // Direct users in a channel to another channel
        [Command("redirect"), Summary("Redirect discussion to another channel.")]
        public async Task Redirect([Summary("The channel to move discussion to.")] ITextChannel channel)
        {
            if (Moderation.CommandAllowed("redirect", Context))
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync($"Move this discussion to <#{channel.Id}>!");
            }
        }

        // Remove all of a user's warns
        [Command("clear warns"), Summary("Clears all warns that a user received.")]
        public async Task ClearWarns([Summary("The user whose warns to clear.")] SocketGuildUser mention)
        {
            if (Moderation.CommandAllowed("clear warns", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.ClearWarns((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, mention);
            }
        }

        // Remove one of a user's warns
        [Command("clear warn"), Summary("Clears a warn that a user received.")]
        public async Task ClearWarn([Summary("The index of the warn to clear.")] int index, [Summary("The user whose warn to clear.")] SocketGuildUser mention = null)
        {
            if (Moderation.CommandAllowed("clear warn", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.ClearWarn((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, Convert.ToInt32(index), mention);
            }
        }

        // Show all warns for all members, or a specific member if specified
        [Command("show warns"), Summary("Show all current warns.")]
        public async Task ShowWarns([Summary("The user whose warns to show.")] SocketGuildUser mention = null)
        {
            if (Moderation.CommandAllowed("show warns", Context))
            {
                var embed = Embeds.ShowWarns((SocketTextChannel)Context.Channel, mention);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        // Remove all users with the Lurkers role
        [Command("prune lurkers"), Summary("Removes all users with the Lurkers role.")]
        public async Task PruneLurkers()
        {
            if (Moderation.CommandAllowed("prune lurkers", Context))
            {
                await Context.Message.DeleteAsync();
                var users = await Context.Guild.GetUsersAsync();
                Moderation.PruneLurkers((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, users);
            }
        }

        // Get the ID of a role without pinging it
        [Command("get id"), Summary("Get the ID of a role without pinging it.")]
        public async Task GetID([Remainder, Summary("The name of the role to get the ID of.")] string roleName = null)
        {
            if (Moderation.CommandAllowed("get id", Context))
            {
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
                await Context.Channel.SendMessageAsync(role.Id.ToString());
            }
        }

        // Create a role with a specific color
        [Command("create color"), Summary("Create a role with a specific color")]
        public async Task CreateColor([Summary("The hex value of the Color Role.")] string colorValue, [Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            if (Moderation.CommandAllowed("create color", Context))
            {
                try
                {
                    //Create color role at highest possible position
                    var lowestModeratorRole = Context.Guild.Roles.FirstOrDefault(x => !x.Permissions.Administrator).Position;
                    colorValue = colorValue.Replace("#", "");
                    Discord.Color roleColor = new Discord.Color(uint.Parse(colorValue, NumberStyles.HexNumber));

                    var colorRole = await Context.Guild.CreateRoleAsync($"Color: {roleName}", null, roleColor, false, null);
                    await colorRole.ModifyAsync(r => r.Position = lowestModeratorRole - 1);

                    await Context.Channel.SendMessageAsync("Role successfully created!");

                    //Sort color roles by hue as high as possible on list
                    var orderedRoles = Context.Guild.Roles.Where(x => x.Name.StartsWith("Color: ")).OrderBy(x => System.Drawing.Color.FromArgb(x.Color.R, x.Color.R, x.Color.G, x.Color.B).GetHue());
                    foreach (var role in orderedRoles)
                        await role.ModifyAsync(x => x.Position = lowestModeratorRole - 1);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Role couldn't be created. Make sure you entered a valid hexadecimal value!");
                }
            }
        }

        // Assign yourself a role with a specific color
        [Command("give color"), Summary("Assigns yourself a role with a specific color")]
        public async Task CreateColor([Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            if (Moderation.CommandAllowed("give color", Context))
            {
                try
                {
                    SocketGuildUser user = (SocketGuildUser)Context.User;
                    var clrRole = Context.Guild.Roles.FirstOrDefault(r => r.Name.Equals($"Color: {roleName}", StringComparison.CurrentCultureIgnoreCase));
                    //Give role
                    await user.AddRoleAsync(clrRole);
                    await Context.Channel.SendMessageAsync("Role successfully added!");
                    //Remove other color role user already has
                    foreach (var role in user.Roles)
                        if (role.Name.ToUpper().Contains("COLOR: ") && !role.Name.ToUpper().Contains(roleName.ToUpper()))
                            await user.RemoveRoleAsync(role);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"Role 'Color: {roleName}' couldn't be found. Make sure you entered the exact role name!");
                }
            }

        }

        // List all color roles that you can assign to yourself
        [Command("show colors"), Summary("Lists all color roles that you can assign to yourself")]
        public async Task ShowColors()
        {
            if (Moderation.CommandAllowed("show colors", Context))
            {
                List<IRole> colorRoles = Context.Guild.Roles.Where(x => x.Name.StartsWith("Color: ")).OrderBy(x => System.Drawing.Color.FromArgb(x.Color.R, x.Color.R, x.Color.G, x.Color.B).GetHue()).ToList();

                Bitmap bitmap = new Bitmap(320, 15 * colorRoles.Count, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(System.Drawing.Color.FromArgb(255, 54, 57, 63));
                    using (System.Drawing.Font fnt = new System.Drawing.Font("Whitney Medium", 10))
                        for (int i = 0; i < colorRoles.Count; i++)
                        {
                            System.Drawing.Color roleColor = System.Drawing.Color.FromArgb(Convert.ToInt32(colorRoles[i].Color.R), Convert.ToInt32(colorRoles[i].Color.G), Convert.ToInt32(colorRoles[i].Color.B));
                            System.Drawing.Brush brush = new System.Drawing.SolidBrush(roleColor);
                            graphics.DrawString(colorRoles[i].Name, fnt, brush, 5, 15 * i);
                        }
                }
                    
                bitmap.Save("colors.png", System.Drawing.Imaging.ImageFormat.Png);
                await Context.Channel.SendFileAsync("colors.png");
            }
        }

        // Change an existing role's color value
        [Command("update color"), Summary("Change an existing role's color value.")]
        public async Task UpdateColor([Summary("The hex value of the Color Role.")] string colorValue, [Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            if (Moderation.CommandAllowed("update color", Context))
            {
                var users = await Context.Guild.GetUsersAsync();
                var colorRole = Context.Guild.Roles.FirstOrDefault(r => r.Name.ToUpper().Contains($"COLOR: {roleName.ToUpper()}"));

                if (users.Any(x => x.RoleIds.Contains(colorRole.Id) && x.Id != Context.User.Id))
                    await Context.Channel.SendMessageAsync($"Role 'Color: {roleName}' is already in use by a different Member, so you can't update it. Try creating a new color role with ``?create color``");
                else
                {
                    colorValue = colorValue.Replace("#", "");
                    Discord.Color roleColor = new Discord.Color(uint.Parse(colorValue, NumberStyles.HexNumber));

                    try
                    {
                        //Sort color roles by hue
                        var lowestModeratorRole = Context.Guild.Roles.FirstOrDefault(x => !x.Permissions.Administrator).Position;
                        await colorRole.ModifyAsync(r => r.Color = roleColor);
                        await colorRole.ModifyAsync(r => r.Position = lowestModeratorRole - 1);
                        await Context.Channel.SendMessageAsync("Role successfully updated!");

                        var orderedRoles = Context.Guild.Roles.Where(x => x.Name.StartsWith("Color: ")).OrderBy(x => System.Drawing.Color.FromArgb(x.Color.R, x.Color.R, x.Color.G, x.Color.B).GetHue());
                        //Try to move color role to highest spot possible
                        foreach (var role in orderedRoles)
                            await role.ModifyAsync(x => x.Position = lowestModeratorRole - 1);
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync($"Role 'Color: {roleName}' couldn't be found. Make sure you entered the exact role name!");
                    }
                }
            }

        }

        // Remove unused color roles
        [Command("prune colors"), Summary("Remove unused Color Roles.")]
        public async Task PruneColors()
        {
            if (Moderation.CommandAllowed("prune colors", Context))
            {
                var users = await Context.Guild.GetUsersAsync();
                var roles = Context.Guild.Roles.Where(r => r.Name.ToUpper().Contains($"COLOR: "));
                int pruneCount = 0;

                foreach (var role in roles)
                {
                    if (users.Any(x => x.RoleIds.Contains(role.Id)))
                        Processing.LogDebugMessage($"Role {role.Name} is in use.");
                    else
                    {
                        Processing.LogDebugMessage($"Role {role.Name} is unused, deleting...");
                        await role.DeleteAsync();
                        pruneCount++;
                    }
                }
                await Context.Channel.SendMessageAsync(embed: Embeds.ColorMsg($"Deleted {pruneCount} unused Color Roles.", Context.Guild.Id));
            }

        }

        // Change an existing color role's name
        [Command("rename color"), Summary("Change an existing color role's name.")]
        public async Task RenameColor([Summary("The name of the Color Role to update.")] string oldRoleName, [Remainder, Summary("The new name of the Color Role.")] string newRoleName)
        {
            if (Moderation.CommandAllowed("rename color", Context))
            {
                var users = await Context.Guild.GetUsersAsync();
                var user = (SocketGuildUser)Context.User;
                var colorRole = user.Guild.Roles.FirstOrDefault(r => r.Name.ToUpper().Equals($"COLOR: {oldRoleName.ToUpper()}"));

                if (users.Any(x => x.RoleIds.Contains(colorRole.Id) && x.Id != Context.User.Id))
                {
                    await Context.Channel.SendMessageAsync($"Role '{colorRole.Name}' is already in use by a different Member, so you can't update it. Try creating a new color role with ``?create color``");
                }
                else
                {
                    try
                    {
                        await colorRole.ModifyAsync(r => r.Name = $"Color: {newRoleName}");
                        await Context.Channel.SendMessageAsync("Color name successfully updated!");
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync($"Role could not be found. Make sure you have a color role first!");
                    }
                }
            }

        }

        // Reply with a randomly generated message
        [Command("markov"), Summary("Replies with a randomly generated message.")]
        public async Task Markov([Remainder, Summary("The rest of your message.")] string msg = "")
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("markov", Context))
            {
                if (!Context.Message.Author.IsBot && Moderation.IsPublicChannel((SocketGuildChannel)Context.Message.Channel))
                    await Processing.Markov((SocketUserMessage)Context.Message, (SocketGuildChannel)Context.Channel, selectedServer, true);
            }
        }

        // Reset the markov dictionary
        [Command("reset markov"), Summary("Resets the markov dictionary.")]
        public async Task ResetMarkov()
        {
            if (Moderation.CommandAllowed("reset markov", Context))
            {
                File.Delete($"Servers\\{Context.Guild.Id.ToString()}\\{Context.Guild.Id.ToString()}.bin");
                await Context.Channel.SendMessageAsync("Markov dictionary successfully reset!");
            }
        }

        // Run a message through multiple languages and back
        [Command("translate"), Summary("Run the message through multiple languages and back.")]
        public async Task Translate([Remainder, Summary("The text to translate.")] string text)
        {
            if (Moderation.CommandAllowed("translate", Context))
                await Context.Channel.SendMessageAsync(BadTranslator.Translate(text));
        }

        // Delete a number of messages
        [Command("delete"), Summary("Deletes a set number of messages from the channel.")]
        public async Task DeleteMessages([Summary("The number of messages to delete.")] int amount)
        {
            if (Moderation.CommandAllowed("delete", Context))
            {
                var channel = (ITextChannel)Context.Channel;
                var msgs = await channel.GetMessagesAsync(amount).FlattenAsync();

                await channel.DeleteMessagesAsync(msgs);
            }
        }

        // Saves message to a "pinned" message channel
        [Command("pin"), Summary("Saves message to a pinned message channel.")]
        public async Task PinMessage([Summary("The ID of the message to pin.")] string messageId, [Remainder, Summary("The ID of the channel the message is in.")] IMessageChannel channel)
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("pin", Context))
            {
                if (channel == null)
                    channel = (ITextChannel)Context.Channel;
                var msg = await channel.GetMessageAsync(Convert.ToUInt64(messageId));

                try
                {
                    var pinChannel = await Context.Guild.GetTextChannelAsync(selectedServer.Channels.Pins);

                    var embed = Embeds.Pin((SocketTextChannel)Context.Channel, msg, Context.Message.Author);
                    await pinChannel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Could not find referenced message in the specified channel.");
                }
            }
        }

        // Give a user an amount of currency
        [Command("award"), Summary("Give a user currency.")]
        public async Task Award([Summary("The user to award.")] SocketGuildUser mention, [Summary("The amount to award."), Remainder] int amount = 1)
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            var botlog = await Context.Guild.GetTextChannelAsync(selectedServer.Channels.BotLogs);
            if (Moderation.CommandAllowed("award", Context))
            {
                await Context.Message.DeleteAsync();

                Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).Currency.Add(new Currency() { UserName = mention.Username, UserID = mention.Id, Amount = amount });
                var embed = Embeds.LogAward((SocketGuildUser)Context.Message.Author, mention.Username, amount);
                await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                embed = Embeds.Award((SocketGuildUser)Context.Message.Author, mention.Username, amount);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                
            }
        }

        // Take an amount of user's currency
        [Command("redeem"), Summary("Take a user's currency.")]
        public async Task Redeem([Summary("The user to take from.")] SocketGuildUser mention, [Summary("The amount to take."), Remainder] int amount = 1)
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            var botlog = await Context.Guild.GetTextChannelAsync(selectedServer.Channels.BotLogs);
            if (Moderation.CommandAllowed("redeem", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author, Context.Guild.Id))
                {
                    var userCurrency = selectedServer.Currency.First(x => x.UserID.Equals(mention.Id)).Amount;
                    Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).Currency.First(x => x.UserID.Equals(mention.Id)).Amount = userCurrency - amount;
                    var embed = Embeds.LogRedeem((SocketGuildUser)Context.Message.Author, mention.Username, amount);
                    await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                    embed = Embeds.Redeem((SocketGuildUser)Context.Message.Author, mention.Username, amount);
                    await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                }
            }
        }

        // Check your currency balance
        [Command("balance"), Summary("Check your balance.")]
        public async Task Balance([Summary("The user to check the balance of."), Remainder] SocketGuildUser mention = null)
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("balance", Context))
            {
                int amount = 0;
                SocketGuildUser user = (SocketGuildUser)Context.Message.Author;
                if (mention != null)
                    user = mention;

                amount = selectedServer.Currency.First(x => x.UserID.Equals(user.Id)).Amount;
                await Context.Channel.SendMessageAsync($"{user.Username} has {amount} {selectedServer.Strings.CurrencyName}.");
            }
        }

        //Send currency to another user
        [Command("send"), Summary("Send currency to another user.")]
        public async Task Send([Summary("The user to send currency to.")] SocketGuildUser mention = null, [Summary("The amount to send."), Remainder] int amount = 1)
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("send", Context))
            {
                // Ensure both users have a currency entry
                if (!selectedServer.Currency.Any(x => x.UserID.Equals(Context.Message.Author.Id)))
                    Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).Currency.Add(new Currency() { 
                        UserName = Context.Message.Author.Username, UserID = Context.Message.Author.Id, Amount = 0 });
                if (!selectedServer.Currency.Any(x => x.UserID.Equals(mention.Id)))
                    Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).Currency.Add(new Currency() { 
                        UserName = mention.Username, UserID = mention.Id, Amount = 0 });
                selectedServer = Botsettings.GetServer(Context.Guild.Id);

                // Check that the post author has enough currency to send the amount
                var authorAmount = selectedServer.Currency.First(x => x.UserID.Equals(Context.Message.Author.Id)).Amount;
                if (authorAmount >= amount)
                {
                    var recipientAmount = selectedServer.Currency.First(x => x.UserID.Equals(mention.Id)).Amount;
                    Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).Currency.First(x => x.UserID.Equals(Context.Message.Author.Id)).Amount = authorAmount - amount;
                    Program.settings.Servers.First(x => x.Id.Equals(Context.Guild.Id)).Currency.First(x => x.UserID.Equals(mention.Id)).Amount = recipientAmount - amount;

                    var botlog = await Context.Guild.GetTextChannelAsync(selectedServer.Channels.BotLogs);
                    var embed = Embeds.Send((SocketGuildUser)Context.Message.Author, mention.Username, amount);
                    await botlog.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                    await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"You don't have enough {selectedServer.Strings.CurrencyName}!");
                }
            }
        }

        // Disable bot
        [Command("logoff"), Summary("End bot process.")]
        public async Task LogOff()
        {
            if (Moderation.CommandAllowed("logoff", Context))
                Program.settings.Active = false;
            await Context.Channel.SendMessageAsync($"Logging off...");
        }
    }
}
