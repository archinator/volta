using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Volta.Bot.Application.Interfaces;
using Volta.Bot.Application.Settings;
using Xabe.FFmpeg;
using File = System.IO.File;

namespace Volta.Bot.Infrastructure.Converters
{
    public class XabeConverter : IMediaConverter
    {
        private const double MainFileToWatermarkRatio = 4;
        private const int DefaultWatermarkHeight = 110;
        private readonly ILogger<XabeConverter> _logger;
        public XabeConverter(
            BotSettings settings,
            ILogger<XabeConverter> logger)
        {
            _logger = logger;
            if (!string.IsNullOrWhiteSpace(settings.FFMpegPath))
            {
                FFmpeg.SetExecutablesPath(settings.FFMpegPath);
            }
        }

        public async Task<Stream> ConvertVideoAsync(string filePath)
        {
            var tempFile = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempFile, ".mp4");

            var ms = new MemoryStream();

            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                var vStream = mediaInfo.VideoStreams.FirstOrDefault();
                var watermarkPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "volta.png");
                var (mainFileWidth, mainFileHeight) = (vStream?.Width ?? 1280, vStream?.Height ?? 768);
                var scaleRatio = await GetWatermarkScaleRatio(watermarkPath, Math.Max(mainFileWidth, mainFileHeight));

                IStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                //?.SetCodec(VideoCodec.h264);
                //?.SetSize(VideoSize.Hvga)
                //?.SetWatermark(watermarkPath, Position.Center);
                IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();
                    //?.SetCodec(AudioCodec.aac);

                var streams = new List<IStream>();

                if (videoStream != null) streams.Add(videoStream);
                if (audioStream != null) streams.Add(audioStream);

                if (!streams.Any()) throw new Exception("Video and Audio stream not found");

                _logger.LogInformation($"Scale ratio: {scaleRatio}");
                _logger.LogInformation("Start conversion. Output: " + outputPath);
                
                var result = await FFmpeg.Conversions.New()
                    //.SetWatermark(filePath, outputPath, watermarkPath, Position.Center))
                    //.AddParameter("-map 1:1?", ParameterPosition.PostInput)
                    //.AddStream(streams)
                    //.SetOutput(outputPath)
                    .Start($"-i \"{filePath}\" -i \"{watermarkPath}\" -c:v h264 -s {mainFileWidth}x{mainFileHeight} -c:a aac -filter_complex \"[1]scale=iw*{scaleRatio}:-1[wm];[0][wm]overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2\" -map 0:1 -map 1:1? -map 0:0 -n \"{outputPath}\"");
                //[1] overlay = (main_w - overlay_w):main_h - overlay_h
                _logger.LogInformation($"Conversion finished. Duration: {result.Duration}");

                using var fs = File.OpenRead(outputPath);
                await fs.CopyToAsync(ms);
                fs.Flush();

                ms.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Convert error: " + e.Message);
                throw;
            }
            finally
            {
                File.Delete(outputPath);
            }

            return ms;
        }

        public async Task<Stream> ConvertImageAsync(string filePath, PhotoSize photo)
        {
            var tempFile = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempFile, ".jpg");

            var ms = new MemoryStream();

            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                var watermarkPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "volta.png");
                var (mainFileWidth, mainFileHeight) = (photo?.Width ?? 1280, photo?.Height ?? 768);
                var scaleRatio = await GetWatermarkScaleRatio(watermarkPath, Math.Max(mainFileWidth, mainFileHeight));

                _logger.LogInformation($"Scale ratio: {scaleRatio}");
                _logger.LogInformation("Start conversion. Output: " + outputPath);
                //var vStream = mediaInfo.VideoStreams.FirstOrDefault();
                var result = await FFmpeg.Conversions.New()
                    //.SetWatermark(filePath, outputPath, watermarkPath, Position.Center))
                    //.AddParameter("-map 1:1?", ParameterPosition.PostInput)
                    //.AddStream(streams)
                    //.SetOutput(outputPath)
                    .Start($"-i \"{filePath}\" -i \"{watermarkPath}\" -s {mainFileWidth}x{mainFileHeight} -filter_complex \"[1]scale=iw*{scaleRatio}:-1[wm];[0][wm]overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2\" -map 0:1? -map 1:1? -map 0:0? -n \"{outputPath}\"");
                //[1] overlay = (main_w - overlay_w):main_h - overlay_h
                _logger.LogInformation($"Conversion finished. Duration: {result.Duration}");

                using var fs = File.OpenRead(outputPath);
                await fs.CopyToAsync(ms);
                fs.Flush();

                ms.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Convert error: " + e.Message);
                throw;
            }
            finally
            {
                File.Delete(outputPath);
            }

            return ms;
        }

        public async Task<Stream> ConvertDocumentAsync(string filePath)
        {
            var tempFile = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempFile, ".jpg");

            var ms = new MemoryStream();

            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                var watermarkPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "volta.png");
                var (mainFileWidth, mainFileHeight) = (mediaInfo.VideoStreams.FirstOrDefault()?.Width ?? 1280, mediaInfo.VideoStreams.FirstOrDefault()?.Height ?? 768);
                var scaleRatio = await GetWatermarkScaleRatio(watermarkPath, Math.Max(mainFileWidth, mainFileHeight));

                _logger.LogInformation($"Scale ratio: {scaleRatio}");
                _logger.LogInformation("Start conversion. Output: " + outputPath);
                //var vStream = mediaInfo.VideoStreams.FirstOrDefault();
                var result = await FFmpeg.Conversions.New()
                    //.SetWatermark(filePath, outputPath, watermarkPath, Position.Center))
                    //.AddParameter("-map 1:1?", ParameterPosition.PostInput)
                    //.AddStream(streams)
                    //.SetOutput(outputPath)
                    .Start($"-i \"{filePath}\" -i \"{watermarkPath}\" -s {mainFileWidth}x{mainFileHeight} -filter_complex \"[1]scale=iw*{scaleRatio}:-1[wm];[0][wm]overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2\" -map 0:1? -map 1:1? -map 0:0? -n \"{outputPath}\"");
                //[1] overlay = (main_w - overlay_w):main_h - overlay_h
                _logger.LogInformation($"Conversion finished. Duration: {result.Duration}");

                using var fs = File.OpenRead(outputPath);
                await fs.CopyToAsync(ms);
                fs.Flush();

                ms.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Convert error: " + e.Message);
                throw;
            }
            finally
            {
                File.Delete(outputPath);
            }

            return ms;
        }

        private async Task<double> GetWatermarkScaleRatio (string watermarkPath, int mainFileSideValue)
        {
            var watermarkInfo = await FFmpeg.GetMediaInfo(watermarkPath);
            var waterMarkHeight = watermarkInfo.VideoStreams.FirstOrDefault()?.Height ?? DefaultWatermarkHeight;

            var ratio = (mainFileSideValue / MainFileToWatermarkRatio) / waterMarkHeight;
            return Math.Max(ratio, 3);
        }
    }
}
