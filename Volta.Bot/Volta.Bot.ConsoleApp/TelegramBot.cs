using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Volta.Bot.Application.Interfaces;
using Volta.Bot.Application.Settings;
using Volta.Bot.Application.Utils;
using Volta.Bot.Infrastructure.Converters;
using Volta.Bot.Infrastructure.ResourceHandlers;

namespace Volta.Bot.ConsoleApp
{
    public delegate Task NewContentHandler(Stream fileStream, string caption = null);

    public sealed class TelegramBot : IDisposable
    {
        private readonly TelegramBotClient _client;
        private readonly BotSettings _settings;
        private readonly ILogger<TelegramBot> _logger;
        private readonly ILogger<ResourceHandler> _resourceHandlerLogger;
        private readonly ILogger<DefaultHandler> _defaultHandlerLogger;
        private readonly ILogger<ResourceConverter> _resourceConverterLogger;

        private long _botId;

        public event NewContentHandler FileUploaded;

        public TelegramBot(
            BotSettings settings,
            ILogger<TelegramBot> logger,
            ILogger<ResourceHandler> resourceHandlerLogger,
            ILogger<DefaultHandler> defaultHandlerLogger,
            ILogger<ResourceConverter> resourceConverterLogger)
        {
            _settings = settings;
            _logger = logger;
            _resourceHandlerLogger = resourceHandlerLogger;
            _defaultHandlerLogger = defaultHandlerLogger;
            _resourceConverterLogger = resourceConverterLogger;

            _logger.LogInformation("Starting Telegram Bot");

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            //_client = new TelegramBotClient(_settings.Token);
            _client = new TelegramBotClient(_settings.Token, null, "http://localhost:8081");
            _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            var (id, name) = GetBotInfoAsync().GetAwaiter().GetResult();
            _botId = id;

            _logger.LogInformation($"Telegram Bot init successfully. ID: {id} | Name: {name}");
        }

        public async Task<(long Id, string Name)> GetBotInfoAsync()
        {
            var me = await _client.GetMeAsync();
            return (me.Id, me.FirstName);
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is Message message)
            {
                if (message.ForwardFrom != null) return;

                _logger.LogInformation($"User {message.GetFromFullName()} initiated message {message.MessageId}");
                var resourceHandler = GetResourceHandlerByMessageType(message);
                await resourceHandler.Handle(message);
            }
        }

        public IResourceHandler GetResourceHandlerByMessageType(Message message) => message.Type switch
        {
            MessageType.Photo => new PhotoHandler(_client, new PhotoConverter(_resourceConverterLogger), _settings, _botId, 
                message.GetPhotoWithBestResolution().FileSize, message.GetPhotoWithBestResolution().FileId, _resourceHandlerLogger),
            MessageType.Video => new VideoHandler(_client, new VideoConverter(_resourceConverterLogger), _settings, _botId,
                message.Video.FileSize, message.Video.FileId, _resourceHandlerLogger),
            MessageType.Document => new DocumentHandler(_client, new DocumentConverter(_resourceConverterLogger), _settings, _botId,
                message.Document.FileSize, message.Document.FileId, _resourceHandlerLogger),
            _ => new DefaultHandler(_defaultHandlerLogger),
        };

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                _logger.LogError(apiRequestException.Message);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _client.CloseAsync();
        }
    }
}
