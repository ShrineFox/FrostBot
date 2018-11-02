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

namespace JackFrostBot
{
    public class InfoModule : ModuleBase
    {
        // ~say hello -> hello
        [Command("say"), Summary("Echos a message.")]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author) && Context.Message.Author.IsBot == false)
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync(echo);
            }
            else if (Context.Channel.Id == Setup.BotSandBoxChannelId(Context.Guild.Id) && Setup.EnableSayCommand(Context.Guild.Id) && Context.Message.Author.IsBot == false)
            {
                await ReplyAsync(echo);
            }
        }

        // ~about cvm
        [Command("about"), Summary("Get info about a file format.")]
        public async Task GetInfo([Remainder, Summary("The format to get info about.")] string keyword)
        {
                var embed = Embeds.FormatInfo(keyword, Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        [Command("help"), Summary("Get info about using the bot.")]
        public async Task GetHelp()
        {
            var embed = Embeds.BotInfo(Context.Guild.Id);
            await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        [Command("set game"), Summary("Change the Currently Playing text.")]
        public async Task SetGame([Remainder, Summary("The text to set as the Game.")] string game)
        {
            var client = (DiscordSocketClient)Context.Client;
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
            {
                await client.SetGameAsync(game);
                using (StreamWriter stream = File.CreateText(@"game.txt"))
                    stream.Write($"{game}");
            } 
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        [Command("grant serious"), Summary("Grant yourself the Serious Talk role.")]
        public async Task GrantSrs()
        {
            await Context.Message.DeleteAsync();
            var user = (IGuildUser)Context.Message.Author;
            var srsRole = user.Guild.GetRole(Setup.NsfwRoleId(user.Guild.Id));
            if (srsRole != null)
                await user.AddRoleAsync(srsRole);
            else
                await Context.Channel.SendMessageAsync("The specified role has to be saved in the setup.ini first!");
        }

        [Command("remove serious"), Summary("Remove your Serious Talk role.")]
        public async Task RemoveSrs()
        {
            await Context.Message.DeleteAsync();
            var user = (IGuildUser)Context.Message.Author;
            var srsRole = user.Guild.GetRole(Setup.NsfwRoleId(user.Guild.Id));
            if (srsRole != null)
                await user.RemoveRoleAsync(srsRole);
            else
                await Context.Channel.SendMessageAsync("The specified role has to be saved in the setup.ini first!");
        }

        [Command("grant artist"), Summary("Grant yourself the Artist role.")]
        public async Task GrantArtist()
        {
            await Context.Message.DeleteAsync();
            var user = (IGuildUser)Context.Message.Author;
            var artRole = user.Guild.GetRole(Setup.ArtRoleId(user.Guild.Id));
            if (artRole != null)
                await user.AddRoleAsync(artRole);
            else
                await Context.Channel.SendMessageAsync("The specified role has to be saved in the setup.ini first!");
        }
    }

    // Create a module with no prefix
    public class Info : ModuleBase
    {
        //List info commands
        [Command("list"), Summary("Lists info commands.")]
        public async Task List()
        {
            if (Context.Channel.Id != Setup.BotSandBoxChannelId(Context.Guild.Id))
            {
                await Context.Channel.SendMessageAsync(
                    $"Use that command in <#{Setup.BotSandBoxChannelId(Context.Guild.Id)}> for all topics I know about!");
            }
            else
            {
                var embed = Embeds.List(Context.Guild.Id);
                await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
            }
        }

        //Warn a user and log it
        [Command("release"), Summary("Release a mod in the mod-releases channel.")]
        public async Task Warn([Summary("The reason for the warn."), Remainder] string message)
        {
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author) || Moderation.IsModder((IGuildUser)Context.Message.Author))
            {
                ulong ModReleaseChannelId = Setup.ModReleaseChannelId(Context.Guild.Id);
                var releaseChannel = await Context.Guild.GetTextChannelAsync(Setup.ModReleaseChannelId(Context.Guild.Id));
                bool includesDownload = false;
                string download = "";

                var links = message.Split("\t\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("http://") || s.StartsWith("www.") || s.StartsWith("https://"));
                foreach (string link in links)
                {
                    if (link.ToLower().Contains("youtube") || link.ToLower().Contains("youtu.be") || link.ToLower().Contains(".png") || link.ToLower().Contains(".gif") || link.ToLower().Contains(".jpg"))
                    {
                        await releaseChannel.SendMessageAsync(link);
                        message = message.Replace(link, "");
                    }
                    foreach (string requiredlink in Setup.RequiredURLs(Context.Guild.Id))
                    {
                        if (link.ToLower().Contains(requiredlink))
                        {
                            includesDownload = true;
                            message = message.Replace(link, "");
                            download = link;
                        }
                        else if (Context.Message.Attachments.Count > 0) {
                            includesDownload = true;
                            download = Context.Message.Attachments.FirstOrDefault().Url;
                        }
                    }
                }
                if (!includesDownload && Setup.RequireDownloadsForRelease(Context.Guild.Id)) {
                    await Context.Channel.SendMessageAsync("The release didn't include a link to an allowed domain!");
                }
                else
                {
                    var embed = Embeds.PostRelease(message, Context.Message.Author.Username, download);
                    await releaseChannel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
                }
                    
            }
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Warn a user and log it
        [Command("warn"), Summary("Warn a user.")]
        public async Task Warn([Summary("The user to warn.")] SocketGuildUser mention, [Summary("The reason for the warn."), Remainder] string reason = "No reason given.")
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                Moderation.Warn(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Mute a user and log it
        [Command("mute"), Summary("Mute a user.")]
        public async Task Mute([Summary("The user to mute.")] SocketGuildUser mention, [Summary("The reason for the mute."), Remainder] string reason = "No reason given.")
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                Moderation.Mute(Context.User.Username, (ITextChannel)Context.Channel, mention);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Unmute a user and log it
        [Command("unmute"), Summary("Unmute a muted user.")]
        public async Task Unmute([Summary("The user to unmute.")] SocketGuildUser mention)
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                Moderation.Unmute((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, mention);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Lock a channel and log it
        [Command("lock"), Summary("Lock a channel.")]
        public async Task Lock()
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                Moderation.Lock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Unlock a channel and log it
        [Command("unlock"), Summary("Unlock a channel.")]
        public async Task Unlock()
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                Moderation.Unlock((SocketGuildUser)Context.User, (ITextChannel)Context.Channel);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Kick a user and log it
        [Command("kick"), Summary("Kick a user.")]
        public async Task Kick([Summary("The user to kick.")] SocketGuildUser mention, [Summary("The reason for the kick."), Remainder] string reason = "No reason given.")
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                Moderation.Kick(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Ban a user and log it
        [Command("ban"), Summary("Ban a user.")]
        public async Task Ban([Summary("The user to ban.")] SocketGuildUser mention, [Summary("The reason for the ban."), Remainder] string reason = "No reason given.")
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                Moderation.Ban(Context.User.Username, (ITextChannel)Context.Channel, mention, reason);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Direct users in a channel to another channel
        [Command("redirect"), Summary("Redirect discussion to another channel.")]
        public async Task Redirect([Summary("The channel to move discussion to.")] ITextChannel channel)
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                await Context.Channel.SendMessageAsync($"Move this discussion to <#{channel.Id}> pront-ho!");
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Remove all of a user's warns
        [Command("clear warns"), Summary("Clears all warns that a user received.")]
        public async Task ClearWarns([Summary("The user whose warns to clear.")] SocketGuildUser mention)
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((SocketGuildUser)Context.User))
                Moderation.ClearWarns((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, mention);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Remove one of a user's warns
        [Command("clear warn"), Summary("Clears a warn that a user received.")]
        public async Task ClearWarn([Summary("The index of the warn to clear.")] int index, [Summary("The user whose warn to clear.")] SocketGuildUser mention = null)
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((SocketGuildUser)Context.User))
                Moderation.ClearWarn((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, Convert.ToInt32(index), mention);
            else
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
        }

        //Show all warns for all members, or a specific member if specified
        [Command("show warns"), Summary("Show all current warns.")]
        public async Task ShowWarns([Summary("The user whose warns to show.")] SocketGuildUser mention = null)
        {
            var embed = Embeds.ShowWarns((IGuildChannel)Context.Channel, mention);
            await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        //Show information involving the latest message automatically removed by the bot.
        [Command("show msginfo"), Summary("Show info about the last deleted message.")]
        public async Task ShowMsgInfo()
        {
            var embed = Embeds.ShowMsgInfo(Context.Guild.Id);
            await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);
        }

        //Remove all users with the Lurkers role
        [Command("prune lurkers"), Summary("Removes all users with the Lurkers role.")]
        public async Task PruneLurkers()
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
            {
                var users = await Context.Guild.GetUsersAsync();
                Moderation.PruneLurkers((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, users);
            }
            else
            {
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
            }
        }

        //Remove all users without the Members role
        [Command("prune nonmembers"), Summary("Removes all users without the Members role.")]
        public async Task PruneNonmembers()
        {
            await Context.Message.DeleteAsync();
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
            {
                var users = await Context.Guild.GetUsersAsync();
                Moderation.PruneNonmembers((SocketGuildUser)Context.User, (ITextChannel)Context.Channel, users);
            }
            else
            {
                await Context.Channel.SendMessageAsync(Setup.NoPermissionMessage(Context.Guild.Id));
            }
        }

        //Post the entirety of the .ini file
        [Command("show ini"), Summary("Post the entirety of the setup.ini file.")]
        public async Task ShowIni([Remainder, Summary("The category from the ini to print.")] string category = "")
        {
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author))
                await Context.Channel.SendMessageAsync($"```cs\n{Setup.GetIniCategory(Context.Guild.Id, category)}```");
        }

        //Post the entirety of the .ini file
        [Command("set"), Summary("Change the value of a key in the setup.ini file.")]
        public async Task SetIni([Remainder, Summary("The string to add to the ini.")] string value)
        {
            if (Moderation.IsModerator((IGuildUser)Context.Message.Author) && Setup.ModeratorsCanUpdateSetup(Context.Guild.Id) && Context.Message.Content.Contains("="))
            {
                string[] valueParts = value.Split('=');
                if (Setup.SetIniValue(Context.Guild.Id, valueParts[0], valueParts[1]))
                    await Context.Channel.SendMessageAsync("Key successfully updated!");
                else
                    await Context.Channel.SendMessageAsync(
                        "Failed to find the specified key. Make sure you're using the right case and separating the new value with an = sign.");
            } 
        }

        //Get the ID of a role without pinging it
        [Command("get id"), Summary("Get the ID of a role without pinging it.")]
        public async Task GetID([Remainder, Summary("The name of the role to get the ID of.")] string roleName = null)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            await Context.Channel.SendMessageAsync(role.Id.ToString());
        }

        //Create a role with a specific color
        [Command("create color"), Summary("Create a role with a specific color")]
        public async Task CreateColor([Summary("The hex value of the Color Role.")] string colorValue, [Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            try
            {
                colorValue = colorValue.Replace("#","");
                Discord.Color roleColor = new Discord.Color(uint.Parse(colorValue, NumberStyles.HexNumber));

                await Context.Guild.CreateRoleAsync($"Color: {roleName}", null, roleColor);
                await Context.Channel.SendMessageAsync("Role successfully created!");
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Role couldn't be created. Make sure you entered a valid hexadecimal value!");
            }
            
        }

        //Assign yourself a role with a specific color
        [Command("give color"), Summary("Assigns yourself a role with a specific color")]
        public async Task CreateColor([Remainder, Summary("The name of the Color Role.")] string roleName)
        {
            try
            {
                SocketGuildUser user = (SocketGuildUser)Context.User;
                await user.AddRoleAsync(Context.Guild.Roles.FirstOrDefault(r => r.Name.Equals($"Color: {roleName}", StringComparison.CurrentCultureIgnoreCase)));
                await Context.Channel.SendMessageAsync("Role successfully added!");
                foreach (var role in user.Roles) {
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

        //List all color roles that you can assign to yourself
        [Command("show colors"), Summary("Lists all color roles that you can assign to yourself")]
        public async Task ShowColors()
        {
            List<string> colorRoleNames = new List<string>();

            foreach (var role in Context.Guild.Roles)
            {
                if (role.Name.Contains("Color: ")) {
                    colorRoleNames.Add(role.Name);
                }
            }

            var embed = Embeds.ShowColors((IGuildChannel)Context.Channel, colorRoleNames, Context.Guild.Id);
            await Context.Channel.SendMessageAsync("", embed: embed).ConfigureAwait(false);

        }

        //Change an existing role's color value
        [Command("update color"), Summary("Change an existing role's color value.")]
        public async Task UpdateColor([Summary("The hex value of the Color Role.")] string colorValue, [Remainder, Summary("The name of the Color Role.")] string roleName)
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

    // Create a module with the 'sample' prefix
    [Group("sample")]
    public class Sample : ModuleBase
    {
        // ~sample square 20 -> 400
        [Command("square"), Summary("Squares a number.")]
        public async Task Square([Summary("The number to square.")] int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        // ~sample userinfo --> foxbot#0282
        // ~sample userinfo @Khionu --> Khionu#8708
        // ~sample userinfo Khionu#8708 --> Khionu#8708
        // ~sample userinfo Khionu --> Khionu#8708
        // ~sample userinfo 96642168176807936 --> Khionu#8708
        // ~sample whois 96642168176807936 --> Khionu#8708
        [Command("userinfo"), Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
    }

}
