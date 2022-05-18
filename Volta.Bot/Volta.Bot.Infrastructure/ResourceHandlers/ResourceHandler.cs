using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Volta.Bot.Application.Interfaces;
using Volta.Bot.Application.Settings;
using Volta.Bot.Application.Utils;

namespace Volta.Bot.Infrastructure.ResourceHandlers
{
    public abstract class ResourceHandler : IResourceHandler
    {
        private readonly IResourceConverter _converter;
        private readonly BotSettings _settings;
        private readonly long _botId;
        private readonly int? _fileSize;
        private readonly string _fileId;
        private readonly ILogger<ResourceHandler> _logger;

        public ResourceHandler(
            TelegramBotClient client, 
            IResourceConverter converter, 
            BotSettings settings,
            long botId,
            int? fileSize,
            string fileId,
            ILogger<ResourceHandler> logger)
        {
            Client = client;
            _converter = converter;
            _settings = settings;
            _botId = botId;
            _fileSize = fileSize;
            _fileId = fileId;
            _logger = logger;
        }

        protected TelegramBotClient Client { get; set; }

        public abstract Task SendResourceAsync(long chatId, int messageId, InputOnlineFile file, string caption);

        public async Task Handle(Message message)
        {
            try
            {
                Notificator.ResourceHandled(message, _fileSize.GetValueOrDefault());

                var resource = await Client.GetFileAsync(_fileId);
                using var resourceStream = await _converter.ConvertAsync(resource.FilePath);

                Notificator.ResourceConverted(message, _fileSize.GetValueOrDefault(), resourceStream.Length);

                var caption = "Upgraded with water mark";

                var inputFile = new InputOnlineFile(resourceStream, _settings.ResultFileName);
                await SendResourceAsync(message.Chat.Id, message.MessageId, inputFile, caption);

                await TryDeleteMessageAsync(message);
                Notificator.ResourceSendSuccess(message, caption);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Failed to convert resource. Message id: {message.MessageId}. Exception details: {exc.Message}");
                await Client.SendTextMessageAsync(message.Chat.Id,
                    $"{message.GetFromFirstName()}, conversion failed",
                    replyToMessageId: message?.MessageId);
            }
        }

        private async Task TryDeleteMessageAsync(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
            {
                return;
            }
            
            var chatAdmins = await Client.GetChatAdministratorsAsync(message.Chat.Id);
            var isBotAdmin = chatAdmins.Any(ca => ca.Status == ChatMemberStatus.Administrator && ca.User.Id == _botId);
            if (isBotAdmin)
            {
                await Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }
        }
    }
}
