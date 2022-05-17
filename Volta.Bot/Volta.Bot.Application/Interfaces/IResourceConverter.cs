using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Volta.Bot.Application.Interfaces
{
    public interface IResourceConverter
    {
        Task<Stream> ConvertAsync(string filePath, int? width = null, int? height = null);
    }
}
