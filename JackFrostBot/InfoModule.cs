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
using AtlusRandomizer;

namespace JackFrostBot
{
    public class InfoModule : ModuleBase
    {
        // ~about cvm
        [Command("say"), Summary("Make the bot repeat a message.")]
        public async Task Say([Remainder, Summary("The format to get info about.")] string message)
        {
            if (Xml.CommandAllowed("say", Context))
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync(message);
            }
        }

        // ~about cvm
        [Command("about"), Summary("Get info about a file format.")]
        public async Task GetInfo([Remainder, Summary("The format to get info about.")] string keyword)
        {
            if (Xml.CommandAllowed("about", Context))
            {
                var embed = Embeds.FormatInfo(keyword, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        [Command("help"), Summary("Get info about using the bot.")]
        public async Task GetHelp()
        {
            if (Xml.CommandAllowed("help", Context))
            {
                var embed = Embeds.Help(Context.Guild.Id, Moderation.IsModerator((IGuildUser)Context.Message.Author));
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        [Command("set game"), Summary("Change the Currently Playing text.")]
        public async Task SetGame([Remainder, Summary("The text to set as the Game.")] string game)
        {
            if (Xml.CommandAllowed("set game", Context))
            {
                var client = (DiscordSocketClient)Context.Client;
                await client.SetGameAsync(game);
            }
        }

        [Command("grant"), Summary("Grant yourself the specified opt-in role.")]
        public async Task GrantRole([Remainder, Summary("The name of the role.")] string roleName)
        {
            if (Xml.CommandAllowed("grant", Context))
            {
                var user = (IGuildUser)Context.Message.Author;
                var role = user.Guild.GetRole(Setup.OptInRoleId(user.Guild, roleName));
                if (role != null)
                {
                    await user.AddRoleAsync(role);
                    await Context.Channel.SendMessageAsync("Role successfully added!");
                }
                else
                    await Context.Channel.SendMessageAsync("The specified role isn't available for opt-in!");
            }
        }

        [Command("remove"), Summary("Remove the specified role from yourself.")]
        public async Task RemoveRole([Remainder, Summary("The name of the role.")] string roleName)
        {
            if (Xml.CommandAllowed("remove", Context))
            {
                var user = (IGuildUser)Context.Message.Author;
                var role = user.Guild.GetRole(Setup.OptInRoleId(user.Guild, roleName));
                if (role != null)
                {
                    await user.RemoveRoleAsync(role);
                    await Context.Channel.SendMessageAsync("Role successfully removed!");
                }
                else
                    await Context.Channel.SendMessageAsync("The specified role cannot be found!");
            }
        }

        //List info keywords
        [Command("list"), Summary("Lists info keywords.")]
        public async Task List()
        {
            if (Xml.CommandAllowed("list", Context))
            {
                var embed = Embeds.Keywords(Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        //Warn a user and log it
        [Command("warn"), Summary("Warn a user.")]
        public async Task Warn([Summary("The user to warn.")] SocketGuildUser mention, [Summary("The reason for the warn."), Remainder] string reason = "No reason given.")
        {
            if (Xml.CommandAllowed("warn", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                    Moderation.Warn(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Mute a user and log it
        [Command("mute"), Summary("Mute a user.")]
        public async Task Mute([Summary("The user to mute.")] SocketGuildUser mention, [Summary("The reason for the mute."), Remainder] string reason = "No reason given.")
        {
            if (Xml.CommandAllowed("mute", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                    Moderation.Mute(Context.User.Username, (ITextChannel)Context.Channel, mention);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Unmute a user and log it
        [Command("unmute"), Summary("Unmute a muted user.")]
        public async Task Unmute([Summary("The user to unmute.")] SocketGuildUser mention)
        {
            if (Xml.CommandAllowed("unmute", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                    Moderation.Unmute(Context.User.Username, (ITextChannel)Context.Channel, mention);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Lock a channel and log it
        [Command("lock"), Summary("Lock a channel.")]
        public async Task Lock()
        {
            if (Xml.CommandAllowed("lock", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                    Moderation.Lock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Unlock a channel and log it
        [Command("unlock"), Summary("Unlock a channel.")]
        public async Task Unlock()
        {
            if (Xml.CommandAllowed("unlock", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                    Moderation.Unlock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Kick a user and log it
        [Command("kick"), Summary("Kick a user.")]
        public async Task Kick([Summary("The user to kick.")] SocketGuildUser mention, [Summary("The reason for the kick."), Remainder] string reason = "No reason given.")
        {
            if (Xml.CommandAllowed("kick", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                    Moderation.Kick(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Ban a user and log it
        [Command("ban"), Summary("Ban a user.")]
        public async Task Ban([Summary("The user to ban.")] SocketGuildUser mention, [Summary("The reason for the ban."), Remainder] string reason = "No reason given.")
        {
            if (Xml.CommandAllowed("ban", Context))
            {
                SocketGuildUser author = (SocketGuildUser)Context.Message.Author;

                //Check if a user is a moderator, then if the user has a role that enables them to ban other users using the bot
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)author))
                    Moderation.Ban(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Direct users in a channel to another channel
        [Command("redirect"), Summary("Redirect discussion to another channel.")]
        public async Task Redirect([Summary("The channel to move discussion to.")] ITextChannel channel)
        {
            if (Xml.CommandAllowed("redirect", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                    await Context.Channel.SendMessageAsync($"Move this discussion to <#{channel.Id}>!");
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Remove all of a user's warns
        [Command("clear warns"), Summary("Clears all warns that a user received.")]
        public async Task ClearWarns([Summary("The user whose warns to clear.")] SocketGuildUser mention)
        {
            if (Xml.CommandAllowed("clear warns", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((SocketGuildUser)Context.User))
                    Moderation.ClearWarns((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, mention);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Remove one of a user's warns
        [Command("clear warn"), Summary("Clears a warn that a user received.")]
        public async Task ClearWarn([Summary("The index of the warn to clear.")] int index, [Summary("The user whose warn to clear.")] SocketGuildUser mention = null)
        {
            if (Xml.CommandAllowed("clear warn", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((SocketGuildUser)Context.User))
                    Moderation.ClearWarn((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, Convert.ToInt32(index), mention);
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        //Show all warns for all members, or a specific member if specified
        [Command("show warns"), Summary("Show all current warns.")]
        public async Task ShowWarns([Summary("The user whose warns to show.")] SocketGuildUser mention = null)
        {
            if (Xml.CommandAllowed("show warns", Context))
            {
                var embed = Embeds.ShowWarns((IGuildChannel)Context.Channel, mention);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        //Show information involving the latest message automatically removed by the bot.
        [Command("show msginfo"), Summary("Show info about the last deleted message.")]
        public async Task ShowMsgInfo()
        {
            if (Xml.CommandAllowed("show msginfo", Context))
            {
                var embed = Embeds.ShowMsgInfo(Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        //Remove all users with the Lurkers role
        [Command("prune lurkers"), Summary("Removes all users with the Lurkers role.")]
        public async Task PruneLurkers()
        {
            if (Xml.CommandAllowed("prune lurkers", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                {
                    var users = await Context.Guild.GetUsersAsync();
                    Moderation.PruneLurkers((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, users);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
                }
            }
        }

        //Remove all users without the Members role
        [Command("prune nonmembers"), Summary("Removes all users without the Members role.")]
        public async Task PruneNonmembers()
        {
            if (Xml.CommandAllowed("prune nonmembers", Context))
            {
                await Context.Message.DeleteAsync();
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                {
                    var users = await Context.Guild.GetUsersAsync();
                    Moderation.PruneNonmembers((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, users);
                }
                else
                {
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
                }
            }
        }

        //Get the ID of a role without pinging it
        [Command("get id"), Summary("Get the ID of a role without pinging it.")]
        public async Task GetID([Remainder, Summary("The name of the role to get the ID of.")] string roleName = null)
        {
            if (Xml.CommandAllowed("get id", Context))
            {
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
                await Context.Channel.SendMessageAsync(role.Id.ToString());
            }
        }

        //Create a role with a specific color
        [Command("create color"), Summary("Create a role with a specific color")]
        public async Task CreateColor([Summary("The hex value of the Color Role.")] string colorValue, [Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            if (Xml.CommandAllowed("create color", Context))
            {
                try
                {
                    colorValue = colorValue.Replace("#", "");
                    Discord.Color roleColor = new Discord.Color(uint.Parse(colorValue, NumberStyles.HexNumber));

                    await Context.Guild.CreateRoleAsync($"Color: {roleName}", null, roleColor);
                    await Context.Channel.SendMessageAsync("Role successfully created!");
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Role couldn't be created. Make sure you entered a valid hexadecimal value!");
                }
            }
            
        }

        //Assign yourself a role with a specific color
        [Command("give color"), Summary("Assigns yourself a role with a specific color")]
        public async Task CreateColor([Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            if (Xml.CommandAllowed("give color", Context))
            {
                try
                {
                    SocketGuildUser user = (SocketGuildUser)Context.User;
                    await user.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(r => r.Name.Equals($"Color: {roleName}", StringComparison.CurrentCultureIgnoreCase)));
                    await Context.Channel.SendMessageAsync("Role successfully added!");
                    foreach (var role in user.Roles)
                    {
                        if (role.Name.ToUpper().Contains("COLOR: ") && !role.Name.ToUpper().Contains(roleName.ToUpper()))
                        {
                            await user.RemoveRoleAsync(role);
                        }
                    }
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"Role 'Color: {roleName}' couldn't be found. Make sure you entered the exact role name!");
                }
            }

        }

        //List all color roles that you can assign to yourself
        [Command("show colors"), Summary("Lists all color roles that you can assign to yourself")]
        public async Task ShowColors()
        {
            if (Xml.CommandAllowed("show colors", Context))
            {
                List<string> colorRoleNames = new List<string>();

                foreach (var role in Context.Guild.Roles)
                {
                    if (role.Name.Contains("Color: "))
                    {
                        colorRoleNames.Add(role.Name);
                    }
                }

                var embed = Embeds.ShowColors((IGuildChannel)Context.Channel, colorRoleNames, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        //Change an existing role's color value
        [Command("update color"), Summary("Change an existing role's color value.")]
        public async Task UpdateColor([Summary("The hex value of the Color Role.")] string colorValue, [Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            if (Xml.CommandAllowed("update color", Context))
            {
                var users = await Context.Guild.GetUsersAsync();
                var colorRole = Context.Guild.Roles.FirstOrDefault(r => r.Name.ToUpper().Contains($"COLOR: {roleName.ToUpper()}"));
                bool inUse = false;

                foreach (var user in users)
                {
                    if (user.RoleIds.Contains(colorRole.Id) && user.Id != Context.User.Id)
                    {
                        inUse = true;
                    }
                }

                if (!inUse)
                {

                    colorValue = colorValue.Replace("#", "");
                    Discord.Color roleColor = new Discord.Color(uint.Parse(colorValue, NumberStyles.HexNumber));

                    try
                    {
                        await colorRole.ModifyAsync(r => r.Color = roleColor);
                        await Context.Channel.SendMessageAsync("Role successfully updated!");
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync($"Role 'Color: {roleName}' couldn't be found. Make sure you entered the exact role name!");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"Role 'Color: {roleName}' is already in use by a different Member, so you can't update it. Try creating a new color role with ``?create color``");
                }
            }

        }

        //Change an existing color role's name
        [Command("rename color"), Summary("Change an existing color role's name.")]
        public async Task RenameColor([Remainder, Summary("The new name of the Color Role.")] string roleName)
        {
            if (Xml.CommandAllowed("rename color", Context))
            {
                var users = await Context.Guild.GetUsersAsync();
                var user = (SocketGuildUser)Context.User;
                var colorRole = user.Roles.FirstOrDefault(r => r.Name.ToUpper().Contains($"COLOR: "));
                bool inUse = false;

                foreach (var guildUser in users)
                {
                    if (guildUser.RoleIds.Contains(colorRole.Id) && guildUser.Id != Context.User.Id)
                    {
                        inUse = true;
                    }

                }

                if (!inUse)
                {
                    try
                    {
                        await colorRole.ModifyAsync(r => r.Name = $"Color: {roleName}");
                        await Context.Channel.SendMessageAsync("Color name successfully updated!");
                    }
                    catch
                    {
                        await Context.Channel.SendMessageAsync($"Role could not be found. Make sure you have a color role first!");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"Role 'Color: {colorRole.Name}' is already in use by a different Member, so you can't update it. Try creating a new color role with ``?create color``");
                }
            }

        }

        [Command("markov"), Summary("Replies with a randomly generated message.")]
        public async Task Markov([Remainder, Summary("The rest of your message.")] string msg = "")
        {
            if (Xml.CommandAllowed("markov", Context))
            {
                if (!Context.Message.Author.IsBot && Moderation.IsPublicChannel((SocketGuildChannel)Context.Message.Channel))
                    await Processing.Markov(Context.Message.Content, (SocketGuildChannel)Context.Channel, 100);
            }
        }

        [Command("reset markov"), Summary("Resets the markov dictionary.")]
        public async Task ResetMarkov()
        {
            if (Xml.CommandAllowed("reset markov", Context))
            {
                if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                {
                    File.Delete($"Servers\\{Context.Guild.Id.ToString()}\\{Context.Guild.Id.ToString()}.bin");
                    await Context.Channel.SendMessageAsync("Markov dictionary successfully reset!");
                }
                else
                    await Context.Channel.SendMessageAsync(UserSettings.BotOptions.GetString("NoPermissionMessage", Context.Guild.Id));
            }
        }

        [Command("translate"), Summary("Run the message through multiple languages and back.")]
        public async Task Translate([Remainder, Summary("The text to translate.")] string text)
        {
            if (Xml.CommandAllowed("translate", Context))
                await Context.Channel.SendMessageAsync(BadTranslator.Translate(text));
        }

        //Delete a number of messages
        [Command("delete"), Summary("Deletes a set number of messages from the channel.")]
        public async Task DeleteMessages([Summary("The number of messages to delete.")] int amount)
        {
            if (Xml.CommandAllowed("delete", Context))
            {
                var channel = (ITextChannel)Context.Channel;
                var msgs = await channel.GetMessagesAsync(amount).FlattenAsync();

                await channel.DeleteMessagesAsync(msgs);
            }
        }

        //Saves message to a "pinned" message channel
        [Command("pin"), Summary("Saves message to a pinned message channel.")]
        public async Task PinMessage([Remainder, Summary("The ID of the message to pin.")] string messageId)
        {
            if (Xml.CommandAllowed("pin", Context))
            {
                var channel = (ITextChannel)Context.Channel;
                var msg = await channel.GetMessageAsync(Convert.ToUInt64(messageId));

                try
                {
                    var pinChannel = await Context.Guild.GetTextChannelAsync(JackFrostBot.UserSettings.Channels.PinsChannelId(Context.Guild.Id));

                    var embed = Embeds.Pin((IGuildChannel)Context.Channel, msg);
                    await pinChannel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Could not find referenced message in this channel.");

                }

            }
        }
    }

}
