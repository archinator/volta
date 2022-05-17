using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Volta.Bot.ConsoleApp;

NLog.Logger logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();

//try
//{
//    using var host = Host.CreateDefaultBuilder(args)
//    .ConfigureServices((builder, services) =>
//    {
//        ConfigureServices(services, builder.Configuration);
//    })
//    .Build();

//    host.Services.GetRequiredService<TelegramBot>();
//    host.Run();
//}
//catch (Exception ex)
//{
//    logger.Fatal(ex, "Stopped hosting because of exception");
//}
//finally
//{
//    NLog.LogManager.Shutdown();
//}

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    ConfigureServices(builder.Services, builder.Configuration);

    using var app = builder.Build();
    app.Services.GetRequiredService<TelegramBot>();
    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Stopped hosting because of exception");
}
finally
{
    NLog.LogManager.Shutdown();
}


void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    try
    {
        services.ConfigureSettings(configuration);
        services.ConfigureApplicationServices();
    }
    catch (Exception e)
    {
        logger.Error(e, "Error during configuring services");
        throw;
    }

    logger.Info("Console configuration was completed successfully");
}