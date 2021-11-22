using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace FagrimBot.Core.Managers
{
    public static class EventManager
    {
        private static readonly LavaNode lavaNode = ServiceManager.GetService<LavaNode>();
        private static readonly DiscordSocketClient client = ServiceManager.GetService<DiscordSocketClient>();
        private static readonly CommandService commandService = ServiceManager.GetService<CommandService>();

        public static Task LoadCommands()
        {
            client.Ready += OnReady;
            client.MessageReceived += Client_MessageReceived;
            return Task.CompletedTask;
        }

        private static async Task Client_MessageReceived(SocketMessage messageParam)
        {
            SocketUserMessage? message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasStringPrefix(ConfigManager.Config.Prefix, ref argPos) ||
                message.HasMentionPrefix(client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(client, message);

            var result = await commandService.ExecuteAsync(context, argPos, ServiceManager.Provider);
            if (!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand) return;
            }
        }

        private static async Task OnReady()
        {
            try
            {
                await lavaNode.ConnectAsync();
            }
            catch
            {
                throw;
            }

            Console.WriteLine("Ready");
            await client.SetStatusAsync(UserStatus.Online);
            await client.SetGameAsync($"DnD, prefix: {ConfigManager.Config.Prefix}", null, ActivityType.Watching);
        }
    }
}
