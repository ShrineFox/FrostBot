﻿using System;
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

        [Command("say"), Summary("Make the bot repeat a message.")]
        public async Task Say([Remainder, Summary("The text to repeat.")] string message)
        {
            if (Moderation.CommandAllowed("say", Context))
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync(message);
            }
        }

        [Command("embed"), Summary("Make the bot embed a message.")]
        public async Task Embed([Remainder, Summary("The text to embed.")] string message)
        {
            if (Moderation.CommandAllowed("embed", Context))
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync(embed: Embeds.ColorMsg(message, Context.Guild.Id, 0, true));
            }
        }

        [Command("wiki"), Summary("Get info from a wiki page.")]
        public async Task GetInfo([Remainder, Summary("The page to get info from.")] string keyword)
        {
            if (Moderation.CommandAllowed("wiki", Context))
            {
                var embed = Embeds.Wiki(keyword, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        [Command("help"), Summary("Get info about using the bot.")]
        public async Task GetHelp()
        {
            if (Moderation.CommandAllowed("help", Context))
            {
                var embed = Embeds.Help(Context.Guild.Id, Moderation.IsModerator((SocketGuildUser)Context.Message.Author, Context.Guild.Id));
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

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

        [Command("color"), Summary("Choose a username color.")]
        public async Task Color([Summary("The hex value of the Color Role.")] string colorValue = "", [Remainder, Summary("The name of the Color Role.")] string roleName = "")
        {
            if (Moderation.CommandAllowed("color", Context))
            {
                if (colorValue == "" || roleName == "")
                {
                    // Show color role picker embed
                    await Context.Message.DeleteAsync();
                    var msgProps = Interactions.ColorRoles.Start((SocketGuildUser)Context.User, Botsettings.GetServer(Context.Guild.Id), "colorroles-start", "");
                    var components = (MessageComponent)msgProps.Components;
                    await Context.Channel.SendMessageAsync(embed: (Embed)msgProps.Embed, component: components);
                }
                else if (!Context.Guild.Roles.Any( x => x.Name.ToLower().Equals($"color: {roleName.ToLower()}")))
                {
                    // If role doesn't already exist...
                    try
                    {
                        await Context.Message.DeleteAsync();
                        // Create color role at highest possible position
                        var lowestModeratorRole = Context.Guild.Roles.FirstOrDefault(x => !x.Permissions.Administrator).Position;
                        colorValue = colorValue.Replace("#", "");
                        Discord.Color roleColor = new Discord.Color(uint.Parse(colorValue, NumberStyles.HexNumber));

                        var colorRole = await Context.Guild.CreateRoleAsync($"Color: {roleName}", null, roleColor, false, null);
                        await colorRole.ModifyAsync(r => r.Position = lowestModeratorRole - 1);

                        var orderedRoles = Context.Guild.Roles.Where(x => x.Name.StartsWith("Color: ")).OrderBy(x => System.Drawing.Color.FromArgb(x.Color.R, x.Color.R, x.Color.G, x.Color.B).GetHue());
                        foreach (var role in orderedRoles)
                            await role.ModifyAsync(x => x.Position = lowestModeratorRole - 1);

                        var user = (SocketGuildUser)Context.User;
                        // Remove other color role user already has
                        foreach (var userRole in user.Roles)
                            if (userRole.Name.ToLower().StartsWith("color: "))
                                await user.RemoveRoleAsync(userRole);
                        // Grant new color role
                        await user.AddRoleAsync(colorRole);
                        // Announce change
                        await Context.Channel.SendMessageAsync(embed: Embeds.ColorMsg($"🎨 **Username color created** by {Context.User.Mention}: **{colorRole.Name.Replace("Color: ", "")}**", Context.Guild.Id, Embeds.GetRoleColor(colorRole)));
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync(embed: Embeds.ErrorMsg("**Color Role couldn't be created**. Make sure you entered a valid hexadecimal value!"));
                    }
                }
                else
                    await Context.Channel.SendMessageAsync(embed: Embeds.ErrorMsg("**Color Role couldn't be created**. The specified name may already be in use by another role!"));
            }
        }

        [Command("set game"), Summary("Change the Currently Playing text.")]
        public async Task SetGame([Remainder, Summary("The text to set as the Game.")] string game = "")
        {
            if (Moderation.CommandAllowed("set game", Context))
            {
                var client = (DiscordSocketClient)Context.Client;
                await client.SetGameAsync(game);
            }
        }

        [Command("set status"), Summary("Change the Online status.")]
        public async Task SetStatus([Remainder, Summary("The status to set.")] string status)
        {
            if (Moderation.CommandAllowed("set status", Context))
            {
                var client = (DiscordSocketClient)Context.Client;
                await client.SetStatusAsync((UserStatus)Enum.Parse(typeof(UserStatus), status));
            }
        }

        [Command("set activity"), Summary("Change the Activity type.")]
        public async Task SetActivity([Remainder, Summary("The type of activity to set.")] string activity)
        {
            if (Moderation.CommandAllowed("set status", Context))
            {
                var client = (DiscordSocketClient)Context.Client;
                if (client.Activity != null)
                    await client.SetActivityAsync(new Game(client.Activity.Name, (ActivityType)Enum.Parse(typeof(ActivityType), activity)));
                else
                    await Context.Channel.SendMessageAsync(embed: Embeds.ErrorMsg("**Can't set activity without game name**. Try using ``?set game <gameName>`` first!"));
            }
        }

        [Command("roles"), Summary("Join or leave an opt-in role.")]
        public async Task Roles()
        {
            if (Moderation.CommandAllowed("roles", Context))
            {
                // Show opt-in roles embed
                await Context.Message.DeleteAsync();
                var msgProps = Interactions.OptInRoles.Start((SocketGuildUser)Context.User, Botsettings.GetServer(Context.Guild.Id), "optinroles-start", "");
                var components = (MessageComponent)msgProps.Components;
                await Context.Channel.SendMessageAsync(embed: (Embed)msgProps.Embed, component: components);
            }
        }

        [Command("warn"), Summary("Warn a user.")]
        public async Task Warn([Summary("The user to warn.")] SocketGuildUser mention, [Remainder, Summary("The reason for the warn.")] string reason = "No reason given.")
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("warn", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Warn(mention, (SocketGuildUser)Context.User, (ITextChannel)Context.Channel, reason);
            }
        }

        [Command("mute"), Summary("Mute a user.")]
        public async Task Mute([Summary("The user to mute.")] SocketGuildUser mention, [Remainder, Summary("The duration of mute in minutes.")] int duration = 0)
        {
            if (Moderation.CommandAllowed("mute", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Mute(mention, (SocketGuildUser)Context.User, (ITextChannel)Context.Channel, duration);
            }
        }

        [Command("unmute"), Summary("Unmute a muted user.")]
        public async Task Unmute([Summary("The user to unmute.")] SocketGuildUser mention)
        {
            if (Moderation.CommandAllowed("unmute", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Unmute(mention, (SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
            }
        }

        [Command("lock"), Summary("Lock a channel.")]
        public async Task Lock([Remainder, Summary("The duration of lock in minutes.")] int duration = 0)
        {
            if (Moderation.CommandAllowed("lock", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Lock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, duration);
            }
        }

        [Command("unlock"), Summary("Unlock a channel.")]
        public async Task Unlock()
        {
            if (Moderation.CommandAllowed("unlock", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Unlock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
            }
        }

        [Command("kick"), Summary("Kick a user.")]
        public async Task Kick([Summary("The user to kick.")] SocketGuildUser mention, [Summary("The reason for the kick."), Remainder] string reason = "No reason given.")
        {
            if (Moderation.CommandAllowed("kick", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Kick(mention, (SocketGuildUser)Context.User, (ITextChannel)Context.Channel, reason);
            }
        }

        [Command("ban"), Summary("Ban a user.")]
        public async Task Ban([Summary("The user to ban.")] SocketGuildUser mention, [Summary("The reason for the ban."), Remainder] string reason = "No reason given.")
        {
            if (Moderation.CommandAllowed("ban", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Ban(mention, (SocketGuildUser)Context.User, (ITextChannel)Context.Channel, reason);
            }
        }

        [Command("redirect"), Summary("Redirect discussion to another channel.")]
        public async Task Redirect([Summary("The channel to move discussion to.")] ITextChannel channel)
        {
            if (Moderation.CommandAllowed("redirect", Context))
            {
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync(embed: Embeds.ColorMsg($"Move this conversation to {channel.Mention}!", channel.Guild.Id));
            }
        }

        [Command("clear warns"), Summary("Clears all warns that a user received.")]
        public async Task ClearWarns([Summary("The user whose warns to clear.")] SocketGuildUser mention)
        {
            if (Moderation.CommandAllowed("clear warns", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.ClearWarns(mention, (SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
            }
        }

        [Command("clear warn"), Summary("Clears a warn that a user received.")]
        public async Task ClearWarn([Summary("The index of the warn to clear.")] int index, [Remainder, Summary("The user whose warn to clear.")] SocketGuildUser mention = null)
        {
            if (Moderation.CommandAllowed("clear warn", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.ClearWarn((SocketGuildUser)Context.Message.Author, (ITextChannel)Context.Channel, index, mention);
            }
        }

        [Command("show warns"), Summary("Show all current warns.")]
        public async Task ShowWarns([Remainder, Summary("The user whose warns to show.")] SocketGuildUser mention = null)
        {
            if (Moderation.CommandAllowed("show warns", Context))
            {
                var embed = Embeds.ShowWarns((ITextChannel)Context.Channel, mention);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

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

        [Command("reset markov"), Summary("Resets the markov dictionary.")]
        public async Task ResetMarkov()
        {
            if (Moderation.CommandAllowed("reset markov", Context))
            {

                File.Delete(Program.ymlPath.Replace("settings.yml", $"Servers\\{Context.Guild.Id}\\markov.bin"));
                await Context.Channel.SendMessageAsync("Markov dictionary successfully reset!");
            }
        }

        [Command("translate"), Summary("Run the message through multiple languages and back.")]
        public async Task Translate([Remainder, Summary("The text to translate.")] string text)
        {
            if (Moderation.CommandAllowed("translate", Context))
                await Context.Channel.SendMessageAsync(BadTranslator.Translate(text));
        }

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

        [Command("award"), Summary("Give a user currency.")]
        public async Task Award([Summary("The user to award.")] SocketGuildUser mention, [Summary("The amount to award.")] int amount = 1, [Remainder] string remainder = "")
        {
            if (Moderation.CommandAllowed("award", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Award(mention, (SocketGuildUser)Context.Message.Author, (ITextChannel)Context.Channel, amount);
            }
        }

        [Command("redeem"), Summary("Take a user's currency.")]
        public async Task Redeem([Summary("The user to take from.")] SocketGuildUser mention, [Summary("The amount to take.")] int amount = 1, [Remainder] string remainder = "")
        {
            if (Moderation.CommandAllowed("redeem", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Redeem(mention, (SocketGuildUser)Context.Message.Author, (ITextChannel)Context.Channel, amount);

            }
        }

        [Command("balance"), Summary("Check your balance.")]
        public async Task Balance([Summary("The user to check the balance of.")] SocketGuildUser mention = null, [Remainder] string remainder = "")
        {
            var selectedServer = Botsettings.GetServer(Context.Guild.Id);

            if (Moderation.CommandAllowed("balance", Context))
            {
                int amount = 0;
                SocketGuildUser user = (SocketGuildUser)Context.Message.Author;
                if (mention != null)
                    user = mention;

                if (selectedServer.Currency.Any(x => x.UserID.Equals(user.Id)))
                {
                    amount = selectedServer.Currency.First(x => x.UserID.Equals(user.Id)).Amount;
                    await Context.Channel.SendMessageAsync($"{user.Username} has {amount} {selectedServer.Strings.CurrencyName}.");
                }
                else
                    await Context.Channel.SendMessageAsync($"{user.Username} has 0 {selectedServer.Strings.CurrencyName}.");
            }
        }

        [Command("send"), Summary("Send currency to another user.")]
        public async Task Send([Summary("The user to send currency to.")] SocketGuildUser mention = null, [Summary("The amount to send.")] int amount = 1, [Remainder] string remainder = "")
        {
            if (Moderation.CommandAllowed("send", Context))
            {
                await Context.Message.DeleteAsync();
                Moderation.Send(mention, (SocketGuildUser)Context.Message.Author, (ITextChannel)Context.Channel, amount);
            }
        }

        [Command("logoff"), Summary("End bot process.")]
        public async Task LogOff()
        {
            if (Moderation.CommandAllowed("logoff", Context))
            {
                Program.settings.Active = false;
                Botsettings.Save();
            }
            await Context.Channel.SendMessageAsync($"Logging off...");
        }
    }
}
