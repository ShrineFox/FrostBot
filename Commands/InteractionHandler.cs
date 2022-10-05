using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ShrineFox.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FrostBot
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
        {
            _client = client;
            _handler = handler;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Process when the client is ready, so we can register our commands.
            _client.Ready += ReadyAsync;
            _handler.Log += LogAsync;

            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
            _client.ModalSubmitted += HandleModalSubmission;
        }

        private async Task HandleModalSubmission(SocketModal modal)
        {
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            // Specify the AllowedMentions so we don't actually ping everyone.
            AllowedMentions mentions = new AllowedMentions();
            mentions.AllowedTypes = AllowedMentionTypes.Users;

            string text = "";
            Embed embed = null;
            MessageComponent cmpnt = null;

            switch(modal.Data.CustomId)
            {
                case "embed_menu":
                    embed = Embeds.Build(
                        title: components.First(x => x.CustomId == "embed_title").Value,
                        desc: components.First(x => x.CustomId == "embed_desc").Value,
                        url: components.First(x => x.CustomId == "embed_url").Value,
                        imgUrl: components.First(x => x.CustomId == "embed_img").Value,
                        color: Embeds.GetDiscordColor(components.First(x => x.CustomId == "embed_color").Value)
                        );
                    await modal.Channel.SendMessageAsync(text, false, embed, components: cmpnt, allowedMentions: mentions);
                    break;
                case "embed_menu_author":
                    embed = Embeds.Build(
                        title: components.First(x => x.CustomId == "embed_title").Value,
                        desc: components.First(x => x.CustomId == "embed_desc").Value,
                        url: components.First(x => x.CustomId == "embed_url").Value,
                        imgUrl: components.First(x => x.CustomId == "embed_img").Value,
                        color: Embeds.GetDiscordColor(components.First(x => x.CustomId == "embed_color").Value),
                        authorName: modal.User.Username,
                        authorImgUrl: modal.User.GetAvatarUrl()
                        );
                    await modal.Channel.SendMessageAsync(text, false, embed, components: cmpnt, allowedMentions: mentions);
                    break;
                case "report_menu":
                    embed = Embeds.Build(
                        title: "Report Details",
                        desc: $"**Report Reason**: {components.First(x => x.CustomId == "report_reason").Value}" +
                        $"\n**Action Requested:** {components.First(x => x.CustomId == "report_action").Value}" +
                        $"\n**Followup**:{components.First(x => x.CustomId == "report_followup").Value}",
                        color: Discord.Color.Blue,
                        authorName: modal.User.Username,
                        authorImgUrl: modal.User.GetAvatarUrl()
                        );
                    var guild = _client.Guilds.First(x => x.Id.Equals(modal.GuildId));
                    Server serverSettings = Program.settings.Servers.First(x => x.ServerID.Equals(guild.Id.ToString()));
                    var modChannel = guild.GetTextChannel(Convert.ToUInt64(serverSettings.ModMailChannel.ID));
                    await modChannel.SendMessageAsync(text, false, embed, components: cmpnt, allowedMentions: mentions);
                    break;
                default:
                    break;
            }

            await modal.RespondAsync("", new Embed[] { Embeds.Build(Color.Blue, 
                desc: "The info you entered has been sent successfully.") }, ephemeral: true);
        }

        private async Task LogAsync(LogMessage log)
        {
            Output.Log(log.Message, ConsoleColor.DarkGray);
            await Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            // Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
            // Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
            foreach (var server in Program.settings.Servers)
            {
                await _handler.RegisterCommandsToGuildAsync(Convert.ToUInt64(server.ServerID), true);
                Output.Log($"Registered commands in server \"{server.ServerName}\".", ConsoleColor.Green);
#if !DEBUG
            await _handler.RegisterCommandsGloballyAsync(true);
            Output.Log($"Registered commands globally.", Color.Green);
#endif
            }

        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
                var context = new SocketInteractionContext(_client, interaction);

                // Execute the incoming command.
                var result = await _handler.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnmetPrecondition:
                            Output.Log(result.Error + ": " + result.ErrorReason, ConsoleColor.Red);
                            break;
                        default:
                            Output.Log(result.Error + ": " + result.ErrorReason, ConsoleColor.Red);
                            break;
                    }
            }
            catch
            {
                // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}