using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Volta.Bot.Application.Utils
{
    public static class TelegramMessageExtensions
    {
        public const string DefaultFirstName = "My friend";
        public static string GetFromFullName(this Message message, bool withComma = true)
        {
            var from = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(message?.From?.FirstName))
            {
                from.Append(message.From.FirstName);
            }

            if (!string.IsNullOrWhiteSpace(message?.From?.LastName))
            {
                from.Append(" ");
                from.Append(message.From.LastName);
            }

            if (withComma && from.Length > 0) from.Append(", ");

            return from.ToString();
        }

        public static string GetFromFirstName(this Message message, bool withComma = true)
        {
            if (string.IsNullOrWhiteSpace(message?.From?.FirstName)) return DefaultFirstName;

            return withComma ? message.From.FirstName + ", " : message.From.FirstName;
        }

        public static PhotoSize GetPhotoWithBestResolution(this Message message)
        {
            return message.Photo.OrderByDescending(p => p.FileSize).FirstOrDefault();
        }

        public static PhotoSize GetDocumentWithBestResolution(this Message message)
        {
            return message.Document.Thumb;
        }
    }
}
