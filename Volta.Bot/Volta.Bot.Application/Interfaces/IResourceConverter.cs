namespace Volta.Bot.Application.Interfaces
{
    public interface IResourceConverter
    {
        Task<Stream> ConvertAsync(string filePath);
    }
}
