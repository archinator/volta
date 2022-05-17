﻿using Microsoft.Extensions.Logging;

namespace Volta.Bot.Infrastructure.Converters
{
    public class VideoConverter : ResourceConverter
    {
        public VideoConverter(ILogger<ResourceConverter> logger) : base(logger)
        {
        }

        protected override string GetConversionParams(string filePath, string watermarkPath, string outputPath, int mainFileWidth, int mainFileHeight, double scaleRatio)
        {
            return $"-i \"{filePath}\" -i \"{watermarkPath}\" -c:v h264 -s {mainFileWidth}x{mainFileHeight} -c:a aac -filter_complex \"[1]scale=iw*{scaleRatio}:-1[wm];[0][wm]overlay=(main_w-overlay_w)/2:(main_h-overlay_h)/2\" -map 0:1 -map 1:1? -map 0:0 -n \"{outputPath}\"";
        }

        protected override string GetExtension()
        {
            return ".mp4";
        }
    }
}
