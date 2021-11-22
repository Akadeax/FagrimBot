using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FagrimBot.Core.Managers
{
    public static class CommandManager
    {
        private static readonly CommandService commandService = ServiceManager.GetService<CommandService>();

        public static async Task LoadCommandsAsync()
        {
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceManager.Provider);
        }
    }
}
