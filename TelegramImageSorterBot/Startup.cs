using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramImageSorterBot.db;
using TelegramImageSorterBot.Services;
using TelegramImageSorterBot.Services.imp;

namespace TelegramImageSorterBot
{
    public static class Startup
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public static void Init(string[] args)
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.ConfigureServices((context, services) => ConfiguringServices(args ,context, services));

            ServiceProvider = hostBuilder.Build().Services;
        }

        private static void ConfiguringServices(string[] args, HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ITelegramBotService, TelegramBotService>();

            services.AddDbContext<BotContext>();
        }
    }
}
