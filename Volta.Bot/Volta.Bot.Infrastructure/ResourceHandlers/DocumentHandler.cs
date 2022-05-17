using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Volta.Bot.Application.Interfaces;
using Volta.Bot.Application.Settings;

namespace Volta.Bot.Infrastructure.ResourceHandlers
{
    public class DocumentHandler : ResourceHandler
    {
        public DocumentHandler(
            TelegramBotClient client,
            IResourceConverter converter,
            BotSettings settings,
            long botId,
            int? fileSize,
            string fileId,
            ILogger<ResourceHandler> logger) : base(client, converter, settings, botId, fileSize, fileId, logger)
        {
        }

        public override async Task SendResourceAsync(long chatId, int messageId, InputOnlineFile file, string caption)
        {
            await Client.SendDocumentAsync(chatId, file,
                    caption: caption,
                    replyToMessageId: messageId);
        }
    }
}
