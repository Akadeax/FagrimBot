using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FagrimBot.Core.Managers;
using FagrimBot.Music;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace FagrimBot.Core
{
    public class Bot
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commandService;

        public Bot()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug
            });

            commandService = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
            });

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commandService)
                .AddLavaNode(x =>
                {
                    x.SelfDeaf = true;
                    x.Hostname = "lava.link";
                    x.Port = 80;
                });


            client.Log += log =>
            {
                Console.WriteLine(log.ToString());
                return Task.CompletedTask;
            };

            if (collection == null) throw new Exception("error initializing services");
            ServiceManager.SetProvider((ServiceCollection)collection);
        }


        public async Task MainAsync()
        {
            if (string.IsNullOrWhiteSpace(ConfigManager.Config.Token))
            {
                Console.WriteLine("Token not initialized.");
                return;
            }

            await CommandManager.LoadCommandsAsync();
            await EventManager.LoadCommands();
            await DatabaseManager.LoadConnection();
            AudioManager.InitAudio();

            await client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
