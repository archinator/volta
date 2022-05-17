using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Volta.Bot.Application.Interfaces;

namespace Volta.Bot.Infrastructure.Converters
{
    public class DocumentConverter : IResourceConverter
    {
        public Task<Stream> ConvertAsync(string filePath, PhotoSize photoSize = null)
        {
            throw new NotImplementedException();
        }
    }
}
