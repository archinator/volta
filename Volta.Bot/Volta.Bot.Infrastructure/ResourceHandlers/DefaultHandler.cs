using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Volta.Bot.Application.Interfaces;

namespace Volta.Bot.Infrastructure.ResourceHandlers
{
    public class DefaultHandler : IResourceHandler
    {
        private readonly ILogger<DefaultHandler> _logger;
        public DefaultHandler(ILogger<DefaultHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(Message message)
        {
            _logger.LogInformation($"Input message of type {message.Type} has no handlers");
            return Task.CompletedTask;
        }
    }
}
