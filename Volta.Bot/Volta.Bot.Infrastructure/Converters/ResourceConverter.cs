using Microsoft.Extensions.Logging;
using Volta.Bot.Application.Interfaces;
using Xabe.FFmpeg;

namespace Volta.Bot.Infrastructure.Converters
{
    public class ResourceConverter : IResourceConverter
    {
        private const double MainFileToWatermarkRatio = 4;
        private const int DefaultWatermarkHeight = 110;
        private readonly ILogger<ResourceConverter> _logger;

        public ResourceConverter(ILogger<ResourceConverter> logger)
        {
            _logger = logger;
        }

        public async Task<Stream> ConvertAsync(string filePath)
        {
            var tempFile = Path.GetTempFileName();
            var outputPath = Path.ChangeExtension(tempFile, ".jpg");

            var ms = new MemoryStream();

            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
                var watermarkPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "volta.png");
                var vStream = mediaInfo.VideoStreams.FirstOrDefault();
                var (mainFileWidth, mainFileHeight) = (vStream?.Width ?? 1280, vStream?.Height ?? 768);
                var scaleRatio = await GetWatermarkScaleRatio(watermarkPath, Math.Max(mainFileWidth, mainFileHeight));

                _logger.LogInformation($"Scale ratio: {scaleRatio}");
                _logger.LogInformation("Start conversion. Output: " + outputPath);

                var result = await FFmpeg.Conversions.New()
                    .Start($"-i \"{filePath}\" -i \"{watermarkPath}\" -s {mainFileWidth}x{mainFileHeight} "
                    + $"-filter_complex \"[1]scale=iw*{scaleRatio}:-1[wm];[0][wm]overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2\" "
                    + $"-map 0:1? -map 1:1? -map 0:0? -n \"{outputPath}\"");

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

        private async Task<double> GetWatermarkScaleRatio(string watermarkPath, int mainFileSideValue)
        {
            var watermarkInfo = await FFmpeg.GetMediaInfo(watermarkPath);
            var waterMarkHeight = watermarkInfo.VideoStreams.FirstOrDefault()?.Height ?? DefaultWatermarkHeight;

            var ratio = (mainFileSideValue / MainFileToWatermarkRatio) / waterMarkHeight;
            return Math.Max(ratio, 3);
        }
    }
}
