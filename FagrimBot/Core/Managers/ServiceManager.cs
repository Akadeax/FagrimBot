using Microsoft.Extensions.DependencyInjection;

namespace FagrimBot.Core.Managers
{
    public static class ServiceManager
    {
        public static IServiceProvider? Provider { get; private set; }

        public static void SetProvider(ServiceCollection collection)
        {
            Provider = collection.BuildServiceProvider();
        }

        public static T GetService<T>() where T : notnull
        {
            if (Provider == null) throw new Exception("Service Provider not Initialized");
            return Provider.GetRequiredService<T>();
        }
    }
}
