using Telegram.Bot.Types;

namespace Volta.Bot.Application.Interfaces
{
    public interface IMediaConverter
    {
        Task<Stream> ConvertVideoAsync(string filePath);
        Task<Stream> ConvertImageAsync(string filePath, PhotoSize photoSize);

        Task<Stream> ConvertDocumentAsync(string filePath);
    }
}
