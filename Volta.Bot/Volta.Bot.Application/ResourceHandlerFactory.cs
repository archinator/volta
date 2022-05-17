using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Volta.Bot.Application.Interfaces;

namespace Volta.Bot.Application
{
    public static class ResourceHandlerFactory
    {
        public static IResourceHandler GetResourceHandlerByMessageType(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Video:
                    request = new MessageConversionRequest(message.Video.FileId, message.Video.FileSize);
                    break;

                case MessageType.Photo:
                    request = new MessageConversionRequest(message.Video.FileId, message.Video.FileSize);
                    break;

                case MessageType.Document:
                    request = new MessageConversionRequest(message.Video.FileId, message.Video.FileSize);
                    break;
            }
        }
    }
}
