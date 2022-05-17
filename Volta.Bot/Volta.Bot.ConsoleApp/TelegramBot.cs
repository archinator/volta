using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Volta.Bot.Application.Interfaces;
using Volta.Bot.Application.Settings;
using Volta.Bot.Application.Utils;
using Xabe.FFmpeg;
using File = System.IO.File;

namespace Volta.Bot.ConsoleApp
{
    public delegate Task NewContentHandler(Stream fileStream, string caption = null);

    public sealed class TelegramBot : IDisposable
    {
        private readonly TelegramBotClient _client;
        private readonly IMediaConverter _mediaСonverter;
        private readonly BotSettings _settings;
        private readonly ILogger<TelegramBot> _logger;

        public event NewContentHandler FileUploaded;

        public TelegramBot(
            BotSettings settings, 
            IMediaConverter xabeConverter,
            ILogger<TelegramBot> logger)
        {
            _settings = settings;
            _mediaСonverter = xabeConverter;
            _logger = logger;

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

            var (id, name) = GetBotInfoAsync().Result;

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

                _logger.LogInformation($"User initiated message: {message.GetFromFullName()}");

                switch (message.Type)
                {
                    case MessageType.Video:
                        await ProcessConvertVideoAsync(message);
                        break;

                    case MessageType.Photo:
                        await ProcessConvertImageAsync(message);
                        break;

                    case MessageType.Document:
                        await ProcessConvertDocumentAsync(message);
                        break;
                }
            }
            else if (update.ChannelPost is Message channelPost)
            {
                await ProcessChannelMessageAsync(channelPost);
            }
        }

        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                _logger.LogInformation(apiRequestException.Message);
            }
            return Task.CompletedTask;
        }

        private async Task ProcessChannelMessageAsync(Message message)
        {
            if (message.Type != MessageType.Photo) return;

            var maxResolutionPhoto = message.Photo.First(x => x.FileSize == message.Photo.Max(x => x.FileSize));

            var ms = new MemoryStream();
            var stream = await _client.GetInfoAndDownloadFileAsync(maxResolutionPhoto.FileId, ms);

            await Task.Delay(5000);
            FileUploaded?.Invoke(ms, message.Caption);
        }

        private async Task ProcessConvertVideoAsync(Message message)
        {
            try
            {
                //var tempFile = Path.GetTempFileName();
                //var fileName = Path.ChangeExtension(tempFile, ".mp4");
                //using var file = File.Create(fileName);

                try
                {
                    Notificator.ResourceHandled(message, message.Video.FileSize.GetValueOrDefault());

                    var video = await _client.GetFileAsync(message.Video.FileId);
                    //await _client.DownloadFileAsync(video.FilePath, file);

                    //file.Flush();
                    //file.Close();

                    using var videoStream = await _mediaСonverter.ConvertVideoAsync(video.FilePath);

                    Notificator.ResourceConverted(message, message.Video.FileSize.GetValueOrDefault(), videoStream.Length);

                    var caption = "Upgraded with water mark";

                    var inputFile = new InputOnlineFile(videoStream, _settings.ResultFileName);
                    await _client.SendVideoAsync(message.Chat.Id, inputFile,
                        caption: caption,
                        replyToMessageId: message.MessageId);
                    await _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);

                    Notificator.ResourceSendSuccess(message, caption);
                }
                catch (Exception exc)
                {
                    Notificator.Error("Exception: " + exc.Message);
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"{message.GetFromFirstName()},\n" +
                        $"{exc.Message}\n" +
                        $"{exc.StackTrace}",
                        replyToMessageId: message?.MessageId);
                }
                //finally
                //{
                //    File.Delete(file.Name);
                //}
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        private async Task ProcessConvertImageAsync(Message message)
        {
            try
            {
                //var tempFile = Path.GetTempFileName();
                //var fileName = Path.ChangeExtension(tempFile, ".jpg");
                //using var file = File.Create(fileName);

                try
                {
                    var photo = message.GetPhotoWithBestResolution();
                    var photoSize = photo.FileSize.GetValueOrDefault();
                    Notificator.ResourceHandled(message, photoSize);

                    var image = await _client.GetFileAsync(photo.FileId);
                    //await _client.DownloadFileAsync("photos\\file_2.jpg", file);

                    //file.Flush();
                    //file.Close();

                    using var videoStream = await _mediaСonverter.ConvertImageAsync(image.FilePath, photo);

                    Notificator.ResourceConverted(message, photoSize, videoStream.Length);

                    var caption = "Upgraded with water mark";

                    var inputFile = new InputOnlineFile(videoStream, _settings.ResultFileName);
                    //var inputMedia = new InputMedia(videoStream, _settings.ResultFileName);
                    //await _client.EditMessageMediaAsync(message.Chat.Id, message.MessageId, new InputMediaPhoto(inputMedia));
                    
                    await _client.SendPhotoAsync(message.Chat.Id, inputFile,
                        caption: caption,
                        replyToMessageId: message.MessageId);
                    await _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);

                    Notificator.ResourceSendSuccess(message, caption);
                }
                catch (Exception exc)
                {
                    Notificator.Error("Exception: " + exc.Message);
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"{message.GetFromFirstName()},\n" +
                        $"Something went wrong\n",
                        replyToMessageId: message?.MessageId);
                }
                //finally
                //{
                //    File.Delete(file.Name);
                //}
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        private async Task ProcessConvertDocumentAsync(Message message)
        {
            try
            {
                //var tempFile = Path.GetTempFileName();
                //var fileName = Path.ChangeExtension(tempFile, ".jpg");
                //using var file = File.Create(fileName);

                try
                {
                    var photoSize = message.Document.FileSize.Value;
                    Notificator.ResourceHandled(message, photoSize);

                    var image = await _client.GetFileAsync(message.Document.FileId);
                    //await _client.DownloadFileAsync("photos\\file_2.jpg", file);

                    //file.Flush();
                    //file.Close();

                    using var videoStream = await _mediaСonverter.ConvertDocumentAsync(image.FilePath);

                    Notificator.ResourceConverted(message, photoSize, videoStream.Length);

                    var caption = "Upgraded with water mark";
                    var fileName = $"{_settings.ResultFileName}.jpg";
                    var inputFile = new InputOnlineFile(videoStream, fileName);
                    await _client.SendDocumentAsync(message.Chat.Id, inputFile,
                        caption: caption,
                        replyToMessageId: message.MessageId
                        );

                    Notificator.ResourceSendSuccess(message, caption);
                }
                catch (Exception exc)
                {
                    Notificator.Error("Exception: " + exc.Message);
                    await _client.SendTextMessageAsync(message.Chat.Id,
                        $"{message.GetFromFirstName()},\n" +
                        $"Something went wrong\n",
                        replyToMessageId: message?.MessageId);
                }
                //finally
                //{
                //    File.Delete(file.Name);
                //}
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
            }
        }

        public void Dispose()
        {
            _client.CloseAsync();
        }
    }
}
