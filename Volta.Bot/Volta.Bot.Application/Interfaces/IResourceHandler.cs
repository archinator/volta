using Telegram.Bot.Types;

namespace Volta.Bot.Application.Interfaces
{
    public interface IResourceHandler
    {
        Task Handle(Message message);
    }
}
