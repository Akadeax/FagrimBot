using Discord.Commands;
using FagrimBot.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FagrimBot.Core
{
    public class SyntaxCommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("syntax")]
        [Alias("help", "h")]
        public async Task SyntaxCommand()
        {
            CommandService commandService = ServiceManager.GetService<CommandService>();
            StringBuilder helpBuilder = new();
            helpBuilder.Append("**Commands**:\n");
            foreach (var command in commandService.Commands)
            {
                helpBuilder.Append($"{command.Name} ");
                foreach (var parameter in command.Parameters)
                {
                    helpBuilder.Append($"{parameter.Name}({(parameter.IsOptional ? " " : "x")}) ");
                }
                helpBuilder.Append('\n');
            }

            await ReplyAsync(helpBuilder.ToString());
        }
    }
}
