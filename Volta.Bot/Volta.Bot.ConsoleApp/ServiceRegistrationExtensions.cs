using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volta.Bot.Application.Settings;

namespace Volta.Bot.ConsoleApp
{
    public static class ServiceRegistrationExtensions
    {
        public static void ConfigureSettings(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(config.GetSection("TgBot").Get<BotSettings>());
        }

        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<TelegramBot>();
        }
    }
}
