using FagrimBot.Core;

namespace FagrimBot
{
    class Program
    {
        static void Main()
            => new Bot().MainAsync().GetAwaiter().GetResult();
    }
}